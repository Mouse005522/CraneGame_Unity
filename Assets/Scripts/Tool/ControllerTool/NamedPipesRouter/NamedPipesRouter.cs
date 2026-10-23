using Kun.Tool;
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
            public const string PipeName = "PipeName";
            public const string DefaultPipeName = "CraneGameServerPipe";
            public const string TimeoutSeconds = "TimeoutSeconds";
            public const int DefaultTimeoutSeconds = 5;
        }

        readonly ConcurrentDictionary<string, Func<string, CancellationToken, Task<string>>> handlers = new ConcurrentDictionary<string, Func<string, CancellationToken, Task<string>>> (StringComparer.OrdinalIgnoreCase);
        readonly CancellationTokenSource serverCancellationTokenSource = new CancellationTokenSource ();
        ILogger<NamedPipesRouter> logger;

        TimeSpan sendTimeout = TimeSpan.FromSeconds (ConfigScope.DefaultTimeoutSeconds);
        string pipeName = ConfigScope.DefaultPipeName;
        bool isStarted;

        static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        void IDisposable.Dispose ()
        {
            serverCancellationTokenSource.Cancel ();
            serverCancellationTokenSource.Dispose ();
        }

        void INamedPipesRouter.Start (string pipeName, float sendTimeoutSeconds)
        {
            StartInternal (pipeName, sendTimeoutSeconds);
        }

        public void BindLogger (ILogger<NamedPipesRouter> logger) 
        {
            this.logger = logger;
        }

        void StartInternal (string pipeName, float sendTimeoutSeconds)
        {
            if (isStarted)
            {
                logger.LogWarning ("NamedPipesRouter already started. Ignore duplicated start call.");
                return;
            }

            if (string.IsNullOrWhiteSpace (pipeName) == false)
            {
                this.pipeName = pipeName;
            }
            else
            {
                this.pipeName = ConfigScope.DefaultPipeName;
                logger.LogWarning ("Start pipeName is empty, fallback to '{DefaultPipeName}'.", ConfigScope.DefaultPipeName);
            }

            if (sendTimeoutSeconds > 0)
            {
                this.sendTimeout = TimeSpan.FromSeconds (sendTimeoutSeconds);
            }
            else
            {
                this.sendTimeout = TimeSpan.FromSeconds (ConfigScope.DefaultTimeoutSeconds);
                logger.LogWarning ("Start sendTimeout is invalid, fallback to {DefaultTimeoutSeconds} seconds.", ConfigScope.DefaultTimeoutSeconds);
            }

            isStarted = true;
            _ = StartServerAsync (serverCancellationTokenSource.Token);
        }

        async Task StartServerAsync (CancellationToken cancellationToken)
        {
            logger.LogInformation ("NamedPipesRouter server started. pipeName={PipeName}, sendTimeout={SendTimeoutSeconds} seconds.", pipeName, sendTimeout.TotalSeconds);

            while (cancellationToken.IsCancellationRequested == false)
            {
                var server = new NamedPipeServerStream (pipeName,
                                                        PipeDirection.InOut,
                                                        NamedPipeServerStream.MaxAllowedServerInstances,
                                                        PipeTransmissionMode.Byte,
                                                        PipeOptions.Asynchronous);

                try
                {
                    await server.WaitForConnectionAsync (cancellationToken);
                    _ = HandleConnectionAsync (server, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    server.Dispose ();
                    break;
                }
                catch (Exception ex)
                {
                    server.Dispose ();
                    logger.LogError (ex, "NamedPipesRouter start server loop exception.");
                }
            }
        }

        Task<NamedPipeRouteResult<TResponse>> INamedPipesRouter.SendJsonAsync<TRequest, TResponse> (string route,
                                                                                                      TRequest payload,
                                                                                                      CancellationToken cancellationToken)
        {
            return SendJsonAsyncInternal<TRequest, TResponse> (route, payload, cancellationToken);
        }

        async Task<NamedPipeRouteResult<TResponse>> SendJsonAsyncInternal<TRequest, TResponse> (string route,
                                                                                                  TRequest payload,
                                                                                                  CancellationToken cancellationToken)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken);
            timeoutCts.CancelAfter (sendTimeout);

            string payloadJson = JsonSerializer.Serialize (payload, serializerOptions);

            var request = new NamedPipeRequest
            {
                Route = route,
                Json = payloadJson
            };

            try
            {
                using var client = new NamedPipeClientStream (".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await client.ConnectAsync (timeoutCts.Token);

                using var writer = new StreamWriter (client, Encoding.UTF8, -1, leaveOpen: true);
                using var reader = new StreamReader (client, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, -1, leaveOpen: true);

                string requestJson = JsonSerializer.Serialize (request, serializerOptions);
                logger.LogInformation ("NamedPipesRouter send request. route={Route}, requestJson={RequestJson}", route, requestJson);
                await writer.WriteLineAsync (requestJson);
                await writer.FlushAsync ();

                string responseJson = await reader.ReadLineAsync ();
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
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new NamedPipeRouteResult<TResponse> (NamedPipeResponseStatus.Timeout, string.Empty, default);
            }
            catch (OperationCanceledException)
            {
                throw;
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
            using (server)
            using (var reader = new StreamReader (server, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, -1, leaveOpen: true))
            using (var writer = new StreamWriter (server, Encoding.UTF8, -1, leaveOpen: true))
            {
                try
                {
                    string? requestJson = await reader.ReadLineAsync ();
                    if (string.IsNullOrWhiteSpace (requestJson))
                    {
                        return;
                    }

                    logger.LogInformation ("NamedPipesRouter handle request. requestJson={RequestJson}", requestJson);

                    NamedPipeRequest? request = JsonSerializer.Deserialize<NamedPipeRequest> (requestJson, serializerOptions);
                    NamedPipeResponse response;

                    if (request != null && handlers.TryGetValue (request.Route, out var handler))
                    {
                        string responseJson = await handler.Invoke (request.Json ?? string.Empty, cancellationToken);
                        response = new NamedPipeResponse
                        {
                            Status = NamedPipeResponseStatus.Success,
                            Json = responseJson ?? string.Empty
                        };
                    }
                    else
                    {
                        string routeName = request?.Route ?? string.Empty;
                        logger.LogError ("No handler found for route: {Route}", routeName);
                        response = new NamedPipeResponse
                        {
                            Status = NamedPipeResponseStatus.NoHandler,
                            Json = string.Empty
                        };
                    }

                    string serializedResponse = JsonSerializer.Serialize (response, serializerOptions);
                    logger.LogInformation ("NamedPipesRouter handle response. status={Status}, responseJson={ResponseJson}", response.Status, response.Json ?? string.Empty);
                    await writer.WriteLineAsync (serializedResponse);
                    await writer.FlushAsync ();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    logger.LogError (ex, "NamedPipesRouter handle request exception.");
                }
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
