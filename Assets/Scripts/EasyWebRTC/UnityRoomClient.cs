using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CraneGame.UnityWebApi
{
    [Serializable]
    public sealed class ClientJoinRoomRequest
    {
        public string sdp;

        public ClientJoinRoomRequest (string sdp)
        {
            this.sdp = sdp;
        }
    }

    [Serializable]
    public sealed class ServerJoinRoomResponse
    {
        public bool isSuccess;
        public string answer;
    }

    /// <summary>依 Room API schema 封裝，Editor 與輸出版本共用 Unity HTTP 傳輸。</summary>
    public sealed class UnityRoomClient
    {
        readonly string joinRoomUrl;

        public UnityRoomClient (string baseUrl)
        {
            string normalizedUrl = baseUrl.TrimEnd ('/') + "/";
            var baseUri = new Uri (normalizedUrl, UriKind.Absolute);
            var endpoint = new Uri (baseUri, "api/Room/JoinRoom");
            joinRoomUrl = endpoint.AbsoluteUri;
        }

        // 由 Unity 主執行緒呼叫，等待期間交還 PlayerLoop。
        public async UniTask<ServerJoinRoomResponse> JoinRoomAsync (
            ClientJoinRoomRequest body, CancellationToken cancellationToken = default)
        {
            if (body == null)
            {
                throw new ArgumentNullException (nameof (body));
            }

            cancellationToken.ThrowIfCancellationRequested ();
            Debug.Log ("[Room API] 序列化請求前。");
            string json = JsonUtility.ToJson (body);
            byte[] bytes = Encoding.UTF8.GetBytes (json);
            Debug.Log ("[Room API] 請求序列化完成。");

            using (var request = new UnityWebRequest (joinRoomUrl, UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw (bytes);
                request.downloadHandler = new DownloadHandlerBuffer ();
                request.timeout = 30;
                request.SetRequestHeader ("Content-Type", "application/json");
                request.SetRequestHeader ("Accept", "application/json");

                Debug.Log ($"[Room API] POST {joinRoomUrl}");
                var operation = request.SendWebRequest ();
                Debug.Log ("[Room API] SendWebRequest 已返回，等待回應。");
                try
                {
                    await UniTask.WaitUntil (() => operation.isDone, cancellationToken: cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested ();

                    string responseText = request.downloadHandler.text;
                    if (request.result != UnityWebRequest.Result.Success || request.responseCode != 200)
                    {
                        throw new InvalidOperationException (
                            $"Room API POST {joinRoomUrl} 失敗。HTTP {request.responseCode}：{request.error}\n{responseText}");
                    }

                    if (string.IsNullOrWhiteSpace (responseText))
                    {
                        throw new InvalidOperationException ("Room API 回傳空白內容。");
                    }

                    var response = JsonUtility.FromJson<ServerJoinRoomResponse> (responseText);
                    Debug.Log ($"[Room API] JoinRoom 完成，HTTP {request.responseCode}");
                    return response;
                }
                finally
                {
                    if (!operation.isDone)
                    {
                        request.Abort ();
                    }
                }
            }
        }
    }
}
