using System;
using System.Threading;
using System.Threading.Tasks;

namespace CraneGameServer.Services
{
    public enum NamedPipeResponseStatus
    {
        Success,
        Timeout,
        NoHandler
    }

    public readonly struct NamedPipeRouteResult<TResponse>
    {
        public NamedPipeRouteResult (NamedPipeResponseStatus status, string json, TResponse? response)
        {
            Status = status;
            Json = json;
            Response = response;
        }

        public NamedPipeResponseStatus Status { get; }
        public string Json { get; }
        public TResponse? Response { get; }
    }

    public interface INamedPipesRouter
    {
        /// <summary>listenPipeName 是本端收件入口，targetPipeName 是對端收件入口。</summary>
        void Start (string listenPipeName, string targetPipeName, float sendTimeoutSeconds);

        /// <summary>取消並等待監聽與連線工作結束，再釋放資源。</summary>
        Task StopAsync ();

        Task<NamedPipeRouteResult<TResponse>> SendJsonAsync<TRequest, TResponse> (string route,
                                                                                   TRequest payload,
                                                                                   CancellationToken cancellationToken = default);

        /// <summary>Handler 的非同步工作需遵守 cancellationToken，StopAsync 會等待它收尾。</summary>
        void RegisterHandler (string route, Func<string, CancellationToken, Task<string>> handler);

        bool UnregisterHandler (string route);
    }
}
