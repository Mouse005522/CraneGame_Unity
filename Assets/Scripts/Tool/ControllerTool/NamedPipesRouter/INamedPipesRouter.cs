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
        void Start (string pipeName, float sendTimeoutSeconds);

        Task<NamedPipeRouteResult<TResponse>> SendJsonAsync<TRequest, TResponse> (string route,
                                                                                   TRequest payload,
                                                                                   CancellationToken cancellationToken = default);

        void RegisterHandler (string route, Func<string, CancellationToken, Task<string>> handler);

        bool UnregisterHandler (string route);
    }
}
