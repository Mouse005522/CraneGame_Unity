using System;
using System.Text;
using UnityEngine;
using Unity.WebRTC;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public class EasyWebRTCServer : EasyWebRTCBase
    {
        RTCPeerConnection peerConnection;
        RTCDataChannel dataChannel;

        /// <summary>
        /// 處理外部傳入的 Offer SDP 並回傳 Answer SDP；不負責 HTTP 傳輸。
        /// 請於 Setup 後，在 Unity 主執行緒呼叫。
        /// </summary>
        public async UniTask<string> HandleOfferRequest (string offerSdp)
        {
            if (string.IsNullOrWhiteSpace (offerSdp))
                throw new ArgumentException ("Offer SDP 不可為空。", nameof (offerSdp));
            if (IsClosed || IceGatheringTimeoutSeconds <= 0f)
                throw new InvalidOperationException ("請先初始化 WebRTC，且不可在關閉後處理 Offer。");
            if (peerConnection != null)
                throw new InvalidOperationException ("目前已有 PeerConnection，無法重複處理 Offer。");

            try
            {
                RTCConfiguration config = EasyWebRTCUtility.GetRTCConfiguration ();
                peerConnection = new RTCPeerConnection (ref config);
                peerConnection.OnDataChannel = channel =>
                {
                    Debug.Log ("[Server] 收到 Client 的 RUDP 資料通道連線請求...");
                    dataChannel = channel;
                    dataChannel.OnOpen = () => Debug.Log ("[Server] RUDP 資料通道已開啟！");
                    dataChannel.OnMessage = bytes =>
                    {
                        string msg = Encoding.UTF8.GetString (bytes);
                        Debug.Log ($"[Server 收到 RUDP 訊息]: {msg}");
                        dataChannel.Send (Encoding.UTF8.GetBytes ($"Server 已收到: {msg}"));
                    };
                };

                var offerDesc = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = offerSdp };
                var setRemoteOp = peerConnection.SetRemoteDescription (ref offerDesc);
                await UniTask.WaitUntil (() => setRemoteOp.IsDone);
                if (setRemoteOp.IsError)
                    throw new InvalidOperationException ($"設定遠端 SDP 失敗: {setRemoteOp.Error.message}");

                var answerOp = peerConnection.CreateAnswer ();
                await UniTask.WaitUntil (() => answerOp.IsDone);
                if (answerOp.IsError)
                    throw new InvalidOperationException ($"建立 Answer 失敗: {answerOp.Error.message}");

                var answerDesc = answerOp.Desc;
                var setLocalOp = peerConnection.SetLocalDescription (ref answerDesc);
                await UniTask.WaitUntil (() => setLocalOp.IsDone);
                if (setLocalOp.IsError)
                    throw new InvalidOperationException ($"設定本地 SDP 失敗: {setLocalOp.Error.message}");

                await WaitForIceGathering (peerConnection);
                Debug.Log ("[Server] WebRTC Answer 準備就緒，回傳給呼叫端");
                return peerConnection.LocalDescription.sdp;
            }
            catch
            {
                ClosePeerConnection ();
                throw;
            }
        }

        void ClosePeerConnection ()
        {
            dataChannel?.Close ();
            dataChannel?.Dispose ();
            dataChannel = null;
            peerConnection?.Close ();
            peerConnection?.Dispose ();
            peerConnection = null;
        }

        public override void Close ()
        {
            if (IsClosed)
                return;

            base.Close ();
            ClosePeerConnection ();
        }
    }
}
