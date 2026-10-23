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

        /// <summary>
        /// 保留原例外，另記錄跨 await 前的呼叫位置；不修改 NSwag 產生的 API。
        /// </summary>
        public async Task<ServerJoinRoomResponse> JoinRoomWithDiagnosticsAsync (
            ClientJoinRoomRequest body, CancellationToken cancellationToken = default)
        {
            string requestLocation = $"POST {BaseUrl}api/Room/JoinRoom";
            string requestCallStack = new StackTrace (1, true).ToString ();

            try
            {
                return await JoinRoomAsync (body, cancellationToken).ConfigureAwait (false);
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
