using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CraneGameServer.WebApi
{
    public partial class ClientJoinRoomRequest
    {
        public ClientJoinRoomRequest (string sdp)
        {
            this.Sdp = sdp;
        }
    }

    public partial class RoomClient
    {
        const string RequestLocationKey = "RoomClient.RequestLocation";
        const string RequestCallStackKey = "RoomClient.RequestCallStack";

        public event Action<string> RequestPrepared;

        partial void PrepareRequest (System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
        {
            RequestPrepared?.Invoke ("[RoomClient] JSON 序列化與 HTTP 請求建立完成。");
        }

        partial void PrepareRequest (System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
        {
            RequestPrepared?.Invoke ($"[RoomClient] 即將呼叫 HttpClient.SendAsync：{request.Method} {url}");
        }

        /// <summary>
        /// 保留原例外，另記錄跨 await 前的呼叫位置；不修改 NSwag 產生的 API。
        /// </summary>
        public async Task<ServerJoinRoomResponse> JoinRoomWithDiagnosticsAsync (
            ClientJoinRoomRequest body, CancellationToken cancellationToken = default, Action<string> logProgress = null)
        {
            string requestLocation = $"POST {BaseUrl}api/Room/JoinRoom";
            logProgress?.Invoke ("[RoomClient] 開始擷取呼叫堆疊。");
            string requestCallStack = new StackTrace (1, true).ToString ();
            logProgress?.Invoke ("[RoomClient] 呼叫堆疊擷取完成，開始呼叫 Swagger JoinRoomAsync。");

            try
            {
                Task<ServerJoinRoomResponse> requestTask = JoinRoomAsync (body, cancellationToken);
                logProgress?.Invoke ("[RoomClient] Swagger 已返回 Task，開始等待 HTTP 回應。");
                return await requestTask.ConfigureAwait (false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ex.Data[RequestLocationKey] = requestLocation;
                ex.Data[RequestCallStackKey] = requestCallStack;
                throw;
            }
        }

        /// <summary>
        /// 明確輸出原例外、內層例外堆疊與送出請求時的呼叫堆疊。
        /// Exception.ToString() 本身不會輸出 Data 中的診斷資訊。
        /// Unity Mono 的 SocketException 訊息可能含空字元，須移除以免日誌截斷。
        /// </summary>
        public static string FormatRequestException (Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException (nameof (exception));

            if (!exception.Data.Contains (RequestLocationKey))
                return exception.ToString ().Replace ("\0", string.Empty);

            string message = $"[RoomClient] {exception.Data[RequestLocationKey]}\n" +
                $"{exception}\n" +
                $"Request call stack (captured before await):\n{exception.Data[RequestCallStackKey]}";
            return message.Replace ("\0", string.Empty);
        }
    }
}
