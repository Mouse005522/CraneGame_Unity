using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CraneGameServer.Services
{
    public class NamedPipesRouter : INamedPipesRouter, IDisposable
    {
        static class ConfigScope
        {
            public const string ScopeName = "NamedPipesRouter";
            public const string ListenPipeName = "ListenPipeName";
            public const string TargetPipeName = "TargetPipeName";
            public const string DefaultListenPipeName = "CraneGameServerPipe";
            public const string DefaultTargetPipeName = "CraneGameApiPipe";
            public const string TimeoutSeconds = "TimeoutSeconds";
            public const int DefaultTimeoutSeconds = 5;
        }

        readonly ConcurrentDictionary<string, Func<string, CancellationToken, Task<string>>> handlers = new ConcurrentDictionary<string, Func<string, CancellationToken, Task<string>>> (StringComparer.OrdinalIgnoreCase);
        readonly CancellationTokenSource serverCancellationTokenSource = new CancellationTokenSource ();
        ILogger<NamedPipesRouter> logger;

        TimeSpan sendTimeout = TimeSpan.FromSeconds (ConfigScope.DefaultTimeoutSeconds);
        string listenPipeName = ConfigScope.DefaultListenPipeName;
        string targetPipeName = ConfigScope.DefaultTargetPipeName;
        bool isStarted;
        readonly object lifetimeLock = new object ();
        readonly HashSet<PipeStream> activePipes = new HashSet<PipeStream> ();
        readonly HashSet<Task> activeTasks = new HashSet<Task> ();
        Task serverTask = Task.CompletedTask;
        Task stopTask;
        bool stopping;
        const int StreamBufferSize = 1024;
        static readonly Encoding PipeEncoding = new UTF8Encoding (false);

        static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        void IDisposable.Dispose ()
        {
            // Unity 主執行緒不可阻塞；由 StopAsync 在背景工作結束後釋放 CTS。
            _ = StopAsync ().ContinueWith (task =>
            {
                logger?.LogError (task.Exception, "NamedPipesRouter shutdown exception.");
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public Task StopAsync ()
        {
            TaskCompletionSource<bool> completion;
            lock (lifetimeLock)
            {
                if (stopTask != null)
                    return stopTask;

                stopping = true;
                completion = new TaskCompletionSource<bool> (TaskCreationOptions.RunContinuationsAsynchronously);
                stopTask = completion.Task;
            }

            _ = StopCoreAsync (completion);
            return completion.Task;
        }

        async Task StopCoreAsync (TaskCompletionSource<bool> completion)
        {
            var errors = new List<Exception> ();
            try
            {
                serverCancellationTokenSource.Cancel ();
            }
            catch (Exception ex)
            {
                errors.Add (ex);
            }

            PipeStream[] pipes;
            lock (lifetimeLock)
            {
                pipes = activePipes.ToArray ();
            }
            foreach (var pipe in pipes)
            {
                try { pipe.Dispose (); }
                catch (Exception ex) { errors.Add (ex); }
            }

            try { await serverTask.ConfigureAwait (false); }
            catch (Exception ex) { errors.Add (ex); }

            Task[] tasks;
            lock (lifetimeLock)
            {
                tasks = activeTasks.ToArray ();
            }
            try
            {
                await Task.WhenAll (tasks).ConfigureAwait (false);
            }
            catch (OperationCanceledException)
            {
                // 停止時取消送出中的要求屬於正常流程。
            }
            catch (Exception ex)
            {
                errors.Add (ex);
            }

            handlers.Clear ();
            // Cancel 並不代表 native I/O callback 已完成；只能在所有工作收尾後釋放。
            serverCancellationTokenSource.Dispose ();
            if (errors.Count == 0)
                completion.TrySetResult (true);
            else
                completion.TrySetException (errors);
        }
        void TrackTask (Task task)
        {
            activeTasks.Add (task);
            _ = task.ContinueWith (completed =>
            {
                // 工作本身負責記錄例外，此處也觀察 fault，避免被丟棄的 Task 留下未觀察例外。
                var error = completed.Exception;
                lock (lifetimeLock)
                {
                    activeTasks.Remove (completed);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        bool TrackPipe (PipeStream pipe)
        {
            lock (lifetimeLock)
            {
                if (stopping)
                    return false;
                activePipes.Add (pipe);
                return true;
            }
        }

        void ReleasePipe (PipeStream pipe)
        {
            pipe.Dispose ();
            lock (lifetimeLock)
            {
                activePipes.Remove (pipe);
            }
        }

        void INamedPipesRouter.Start (string listenPipeName, string targetPipeName, float sendTimeoutSeconds)
        {
            StartInternal (listenPipeName, targetPipeName, sendTimeoutSeconds);
        }

        public void BindLogger (ILogger<NamedPipesRouter> logger)
        {
            this.logger = logger;
        }

        void StartInternal (string listenPipeName, string targetPipeName, float sendTimeoutSeconds)
        {
            lock (lifetimeLock)
            {
                if (stopping)
                    throw new ObjectDisposedException (nameof (NamedPipesRouter));
                if (isStarted)
                {
                    logger.LogWarning ("NamedPipesRouter already started. Ignore duplicated start call.");
                    return;
                }

                var listenName = string.IsNullOrWhiteSpace (listenPipeName) ? ConfigScope.DefaultListenPipeName : listenPipeName;
                var targetName = string.IsNullOrWhiteSpace (targetPipeName) ? ConfigScope.DefaultTargetPipeName : targetPipeName;
                if (string.Equals (listenName, targetName, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException ("監聽與發送目標必須使用不同的 pipe 名稱。", nameof (targetPipeName));

                this.listenPipeName = listenName;
                this.targetPipeName = targetName;
                sendTimeout = TimeSpan.FromSeconds (sendTimeoutSeconds > 0 ? sendTimeoutSeconds : ConfigScope.DefaultTimeoutSeconds);
                isStarted = true;
                var token = serverCancellationTokenSource.Token;
                // 純 pipe I/O 不依賴 Unity SynchronizationContext，停止後仍可完成清理。
                serverTask = Task.Run (() => StartServerAsync (token));
            }
        }

        async Task StartServerAsync (CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation ("NamedPipesRouter started. listenPipeName={ListenPipeName}, targetPipeName={TargetPipeName}, sendTimeout={SendTimeoutSeconds} seconds.", listenPipeName, targetPipeName, sendTimeout.TotalSeconds);
                while (!cancellationToken.IsCancellationRequested)
                {
                    NamedPipeServerStream server = null;
                    try
                    {
                        server = new NamedPipeServerStream (listenPipeName, PipeDirection.InOut,
                            NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous);
                        await server.WaitForConnectionAsync (cancellationToken).ConfigureAwait (false);

                        lock (lifetimeLock)
                        {
                            if (stopping)
                                break;

                            activePipes.Add (server);
                            TrackTask (HandleConnectionAsync (server, cancellationToken));
                            server = null; // 已交給連線工作持有與釋放。
                        }
                    }
                    finally
                    {
                        server?.Dispose ();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                logger.LogError (ex, "NamedPipesRouter server loop exception.");
            }
        }

        Task<NamedPipeRouteResult<TResponse>> INamedPipesRouter.SendJsonAsync<TRequest, TResponse> (string route,
                                                                                                      TRequest payload,
                                                                                                      CancellationToken cancellationToken)
        {
            lock (lifetimeLock)
            {
                if (stopping)
                    throw new ObjectDisposedException (nameof (NamedPipesRouter));
                var task = SendJsonAsyncInternal<TRequest, TResponse> (route, payload, cancellationToken);
                TrackTask (task);
                return task;
            }
        }

        async Task<NamedPipeRouteResult<TResponse>> SendJsonAsyncInternal<TRequest, TResponse> (string route,
                                                                                                  TRequest payload,
                                                                                                  CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken, serverCancellationTokenSource.Token);
            timeoutCts.CancelAfter (sendTimeout);

            string payloadJson = JsonSerializer.Serialize (payload, serializerOptions);

            var request = new NamedPipeRequest
            {
                Route = route,
                Json = payloadJson
            };

            try
            {
                using var client = new NamedPipeClientStream (".", targetPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                // 取消時關閉 stream，讓 ReadLineAsync / WriteLineAsync 也能解除等待。
                using var cancellation = timeoutCts.Token.Register (() => client.Dispose ());
                if (!TrackPipe (client))
                    throw new OperationCanceledException (serverCancellationTokenSource.Token);
                try
                {
                    await client.ConnectAsync (timeoutCts.Token).ConfigureAwait (false);

                    using var writer = new StreamWriter (client, PipeEncoding, StreamBufferSize, leaveOpen: true);
                    using var reader = new StreamReader (client, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, StreamBufferSize, leaveOpen: true);

                    string requestJson = JsonSerializer.Serialize (request, serializerOptions);
                    logger.LogInformation ("NamedPipesRouter send request. route={Route}, requestJson={RequestJson}", route, requestJson);
                    await writer.WriteLineAsync (requestJson).ConfigureAwait (false);
                    await writer.FlushAsync ().ConfigureAwait (false);

                    string responseJson = await reader.ReadLineAsync ().ConfigureAwait (false);
                    if (string.IsNullOrWhiteSpace (responseJson))
                    {
                        logger.LogInformation ("NamedPipesRouter receive response. route={Route}, status={Status}, responseJson={ResponseJson}", route, NamedPipeResponseStatus.NoHandler, string.Empty);
                        return new NamedPipeRouteResult<TResponse> (NamedPipeResponseStatus.NoHandler, string.Empty, default);
                    }

                    var response = JsonSerializer.Deserialize<NamedPipeResponse> (responseJson, serializerOptions);
                    if (response == null)
                    {
                        logger.LogInformation ("NamedPipesRouter receive response. route={Route}, status={Status}, responseJson={ResponseJson}", route, NamedPipeResponseStatus.NoHandler, responseJson);
                        return new NamedPipeRouteResult<TResponse> (NamedPipeResponseStatus.NoHandler, string.Empty, default);
                    }

                    logger.LogInformation ("NamedPipesRouter receive response. route={Route}, status={Status}, responseJson={ResponseJson}", route, response.Status, response.Json ?? string.Empty);

                    string routedJson = response.Json ?? string.Empty;
                    if (response.Status == NamedPipeResponseStatus.Success)
                    {
                        TResponse? typedResponse = JsonSerializer.Deserialize<TResponse> (routedJson, serializerOptions);
                        return new NamedPipeRouteResult<TResponse> (response.Status, routedJson, typedResponse);
                    }

                    return new NamedPipeRouteResult<TResponse> (response.Status, routedJson, default);
                }
                finally
                {
                    ReleasePipe (client);
                }
            }
            catch (Exception) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested && !serverCancellationTokenSource.IsCancellationRequested)
            {
                return new NamedPipeRouteResult<TResponse> (NamedPipeResponseStatus.Timeout, string.Empty, default);
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested || serverCancellationTokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException (cancellationToken.IsCancellationRequested ? cancellationToken : serverCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                logger.LogError (ex, "NamedPipesRouter send request exception. route={Route}", route);
                return new NamedPipeRouteResult<TResponse> (NamedPipeResponseStatus.NoHandler, string.Empty, default);
            }
        }

        void INamedPipesRouter.RegisterHandler (string route, Func<string, CancellationToken, Task<string>> handler)
        {
            RegisterHandlerInternal (route, handler);
        }

        bool INamedPipesRouter.UnregisterHandler (string route)
        {
            return UnregisterHandlerInternal (route);
        }

        void RegisterHandlerInternal (string route, Func<string, CancellationToken, Task<string>> handler)
        {
            handlers[route] = handler;
        }

        bool UnregisterHandlerInternal (string route)
        {
            return handlers.TryRemove (route, out _);
        }

        async Task HandleConnectionAsync (NamedPipeServerStream server, CancellationToken cancellationToken)
        {
            // catch 包含 using 的 Dispose，避免對端斷線時 Flush 的 IOException 外漏。
            try
            {
                using (var reader = new StreamReader (server, Encoding.UTF8, false, StreamBufferSize, leaveOpen: true))
                using (var writer = new StreamWriter (server, PipeEncoding, StreamBufferSize, leaveOpen: true))
                {
                    string requestJson = await reader.ReadLineAsync ().ConfigureAwait (false);
                    cancellationToken.ThrowIfCancellationRequested ();
                    if (string.IsNullOrWhiteSpace (requestJson))
                        return;

                    var request = JsonSerializer.Deserialize<NamedPipeRequest> (requestJson, serializerOptions);
                    NamedPipeResponse response;
                    if (request != null && handlers.TryGetValue (request.Route, out var handler))
                    {
                        string responseJson = await handler.Invoke (request.Json ?? string.Empty, cancellationToken).ConfigureAwait (false);
                        cancellationToken.ThrowIfCancellationRequested ();
                        response = new NamedPipeResponse { Status = NamedPipeResponseStatus.Success, Json = responseJson ?? string.Empty };
                    }
                    else
                    {
                        response = new NamedPipeResponse { Status = NamedPipeResponseStatus.NoHandler, Json = string.Empty };
                    }

                    string serializedResponse = JsonSerializer.Serialize (response, serializerOptions);
                    await writer.WriteLineAsync (serializedResponse).ConfigureAwait (false);
                    await writer.FlushAsync ().ConfigureAwait (false);
                }
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (IOException)
            {
                // 對端已斷線，結束這次連線即可。
            }
            catch (Exception ex)
            {
                logger.LogError (ex, "NamedPipesRouter handle request exception.");
            }
            finally
            {
                ReleasePipe (server);
            }
        }
        class NamedPipeRequest
        {
            public string Route { get; set; } = string.Empty;
            public string Json { get; set; } = string.Empty;
        }

        class NamedPipeResponse
        {
            public NamedPipeResponseStatus Status { get; set; }
            public string Json { get; set; } = string.Empty;
        }
    }
}
