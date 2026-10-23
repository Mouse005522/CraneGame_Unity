using System;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Unity.WebRTC;
using Cysharp.Threading.Tasks;
using CraneGameServer.WebApi;

namespace Kun.Tool
{
    public class EasyWebRTCClient : EasyWebRTCBase
    {
        RTCPeerConnection peerConnection;
        RTCDataChannel dataChannel;
        HttpClient httpClient;
        RoomClient roomClient;

        public void Setup (string ip, float iceGatheringTimeoutSeconds = 3f)
        {
            if (string.IsNullOrWhiteSpace (ip))
                throw new ArgumentException ("Room API 位址不可為空。", nameof (ip));

            string baseUrl = ip.Contains ("://") ? ip : $"http://{ip}";
            if (!Uri.TryCreate (baseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                string.IsNullOrEmpty (uri.Host))
                throw new ArgumentException ("Room API 位址必須是 IP、主機名稱或 HTTP(S) URL。", nameof (ip));

            base.Setup (iceGatheringTimeoutSeconds);
            httpClient = new HttpClient ();
            roomClient = new RoomClient (uri.AbsoluteUri, httpClient);
            ConnectToTargetServer ().Forget ();
        }

        async UniTask ConnectToTargetServer ()
        {
            try
            {
                RTCConfiguration config = EasyWebRTCUtility.GetRTCConfiguration ();
                peerConnection = new RTCPeerConnection (ref config);
                peerConnection.OnConnectionStateChange = state =>
                    Debug.Log ($"[Client] PeerConnection 狀態變更: {state}");

                dataChannel = peerConnection.CreateDataChannel ("rudp-channel", new RTCDataChannelInit ());
                dataChannel.OnOpen = () =>
                {
                    Debug.Log ("[Client] RUDP 資料通道已開啟！");
                    dataChannel.Send (Encoding.UTF8.GetBytes ("Hello from Client!"));
                };
                dataChannel.OnMessage = bytes => Debug.Log ($"[Client 收到回應]: {Encoding.UTF8.GetString (bytes)}");
                dataChannel.OnClose = () => Debug.Log ("[Client] RUDP 通道已關閉");

                var offerOp = peerConnection.CreateOffer ();
                await UniTask.WaitUntil (() => offerOp.IsDone);
                if (offerOp.IsError)
                    throw new InvalidOperationException ($"建立 Offer 失敗: {offerOp.Error.message}");

                var offerDesc = offerOp.Desc;
                var setLocalOp = peerConnection.SetLocalDescription (ref offerDesc);
                await UniTask.WaitUntil (() => setLocalOp.IsDone);
                if (setLocalOp.IsError)
                    throw new InvalidOperationException ($"設定本地 SDP 失敗: {setLocalOp.Error.message}");

                // 單次 SDP 交換，需先收齊 ICE candidates。
                await WaitForIceGathering (peerConnection);
                var clientJoinRequest = new ClientJoinRoomRequest (peerConnection.LocalDescription.sdp);
                ServerJoinRoomResponse response = await roomClient.JoinRoomWithDiagnosticsAsync (clientJoinRequest);
                await UniTask.SwitchToMainThread ();
                if (IsClosed)
                    return;
                if (response == null || !response.IsSuccess || string.IsNullOrWhiteSpace (response.Answer))
                    throw new InvalidOperationException ("Room API 加入房間失敗，或未回傳 Answer SDP。");

                var answerDesc = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = response.Answer };
                var setRemoteOp = peerConnection.SetRemoteDescription (ref answerDesc);
                await UniTask.WaitUntil (() => setRemoteOp.IsDone);
                if (setRemoteOp.IsError)
                    throw new InvalidOperationException ($"設定遠端 SDP 失敗: {setRemoteOp.Error.message}");

                Debug.Log ("[Client] Room API 的 Answer SDP 已套用，等待 ICE 建立連線。");
            }
            catch (Exception ex)
            {
                await UniTask.SwitchToMainThread ();
                if (!IsClosed)
                {
                    Debug.LogError (RoomClient.FormatRequestException (ex), this);
                    Close ();
                }
            }
        }

        void Update ()
        {
            if (Input.GetKeyDown (KeyCode.Space) && dataChannel != null && dataChannel.ReadyState == RTCDataChannelState.Open)
            {
                dataChannel.Send (Encoding.UTF8.GetBytes ("Space pressed from Client!"));
                Debug.Log ("[Client] 已發送 RUDP 訊息");
            }
        }

        public override void Close ()
        {
            if (IsClosed)
                return;

            base.Close ();
            dataChannel?.Close ();
            dataChannel?.Dispose ();
            dataChannel = null;
            peerConnection?.Close ();
            peerConnection?.Dispose ();
            peerConnection = null;
            httpClient?.Dispose ();
            httpClient = null;
            roomClient = null;
        }
    }
}
