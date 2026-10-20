using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.WebRTC;
using Cysharp.Threading.Tasks;
using Kun.Tool;

public class EasyWebRTCServer : MonoBehaviour
{
    private const string HeaderAccessControlAllowOrigin = "Access-Control-Allow-Origin";
    private const string HeaderAccessControlAllowMethods = "Access-Control-Allow-Methods";
    private const string HeaderAccessControlAllowHeaders = "Access-Control-Allow-Headers";
    private const string HttpMethodOptions = "OPTIONS";
    private const string HttpMethodPost = "POST";
    private const string ContentTypeTextPlain = "text/plain";

    public int httpPort = 8080;

    private EasyHttp easyHttp;
    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;

    void Start ()
    {
        StartCoroutine (WebRTC.Update ());
        StartHttpServer ().Forget ();
    }

    private async UniTaskVoid StartHttpServer ()
    {
        easyHttp = new EasyHttp (httpPort, "/offer/");
        easyHttp.SetHeaderHandler (HandleHttpHeader);
        easyHttp.RegisterMethodHandler (HttpMethodOptions, HandleOptionsRequest);
        easyHttp.RegisterMethodHandler (HttpMethodPost, HandleOfferRequest);

        await easyHttp.Start ();
    }

    private UniTask HandleHttpHeader (HttpListenerContext context)
    {
        context.Response.AddHeader (HeaderAccessControlAllowOrigin, "*");
        context.Response.AddHeader (HeaderAccessControlAllowMethods, $"{HttpMethodPost}, {HttpMethodOptions}");
        context.Response.AddHeader (HeaderAccessControlAllowHeaders, "Content-Type");

        return UniTask.CompletedTask;
    }

    private UniTask HandleOptionsRequest (HttpListenerContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        return UniTask.CompletedTask;
    }

    private async UniTask HandleOfferRequest (HttpListenerContext context)
    {
        using (var reader = new StreamReader (context.Request.InputStream, context.Request.ContentEncoding))
        {
            string clientOfferSdp = await reader.ReadToEndAsync ();
            string serverAnswerSdp = await HandleClientOffer (clientOfferSdp);

            byte[] responseBuffer = Encoding.UTF8.GetBytes (serverAnswerSdp);
            context.Response.ContentType = ContentTypeTextPlain;
            context.Response.ContentLength64 = responseBuffer.Length;
            await context.Response.OutputStream.WriteAsync (responseBuffer, 0, responseBuffer.Length);
        }

        easyHttp.Stop ();
        easyHttp = null;
        Debug.Log ("[Server] SDP 交握完成，HTTP 監聽器已安全關閉。");
    }

    private async Task<string> HandleClientOffer (string offerSdp)
    {
        RTCConfiguration config = EasyWebRTCUtility.GetRTCConfiguration ();
        peerConnection = new RTCPeerConnection (ref config);

        // 當 Client 建立 DataChannel 時觸發
        peerConnection.OnDataChannel = channel =>
        {
            Debug.Log ("[Server] 收到 Client 的 RUDP 資料通道連線請求...");

            dataChannel = channel;
            dataChannel.OnOpen = () =>
            {
                Debug.Log ("[Server] 🎉 RUDP 資料通道已成功開啟！");
            };
            dataChannel.OnMessage = bytes =>
            {
                string msg = Encoding.UTF8.GetString (bytes);
                Debug.Log ($"[Server 收到 RUDP 訊息]: {msg}");

                byte[] reply = Encoding.UTF8.GetBytes ($"Server 已收到: {msg}");
                dataChannel.Send (reply);
            };
        };

        var offerDesc = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = offerSdp };

        var setRemoteOp = peerConnection.SetRemoteDescription (ref offerDesc);
        while (!setRemoteOp.IsDone) await Task.Delay (10);

        var answerOp = peerConnection.CreateAnswer ();
        while (!answerOp.IsDone) await Task.Delay (10);
        var answerDesc = answerOp.Desc;

        var setLocalOp = peerConnection.SetLocalDescription (ref answerDesc);
        while (!setLocalOp.IsDone) await Task.Delay (10);

        // 等待 ICE 收集
        float timer = 0f;
        while (peerConnection.GatheringState != RTCIceGatheringState.Complete)
        {
            timer += 0.1f;
            if (timer > 3f) break;
            await Task.Delay (100);
        }

        answerDesc = peerConnection.LocalDescription;
        Debug.Log ("[Server] WebRTC Answer 準備就緒，回傳給 Client");
        return answerDesc.sdp;
    }

    private void OnDestroy ()
    {
        easyHttp?.Stop ();
        dataChannel?.Close ();
        peerConnection?.Close ();
    }
}