using System;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_WEBGL || UNITY_EDITOR
using Unity.WebRTC;
#endif
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public class EasyWebRTCServer : EasyWebRTCBase
    {
        public event Action<EasyWebRTCChannel> OnChannelBound;

#if UNITY_WEBGL && !UNITY_EDITOR
        public UniTask<string> HandleOfferRequest (string offerSdp)
        {
            throw new PlatformNotSupportedException ("Use a native platform for EasyWebRTCServer.");
        }
#else
        RTCPeerConnection peerConnection;
        readonly Dictionary<string, EasyWebRTCNativeChannel> channels = new Dictionary<string, EasyWebRTCNativeChannel> ();
        EasyWebRTCNativeChannel defaultChannel;
        public IReadOnlyDictionary<string, EasyWebRTCNativeChannel> Channels => channels;
        public event Action<EasyWebRTCConnectionState> OnConnectionStateChanged;
        public EasyWebRTCConnectionState ConnectionState { get; private set; }

        public override bool IsConnected => !IsClosed && ConnectionState == EasyWebRTCConnectionState.Connected;

        public override bool Send (byte[] data)
        {
            base.Send (data);
            return !IsClosed && defaultChannel != null && defaultChannel.Send (data);
        }


        /// <summary>
        /// 處理外部傳入的 Offer SDP 並回傳 Answer SDP；不負責 HTTP 傳輸。
        /// 請於 Setup 後，在 Unity 主執行緒呼叫。
        /// </summary>
        public async UniTask<string> HandleOfferRequest (string offerSdp)
        {
            if (string.IsNullOrWhiteSpace (offerSdp))
            {
                throw new ArgumentException ("Offer SDP 不可為空。", nameof (offerSdp));
            }
            if (IsClosed || IceGatheringTimeoutSeconds <= 0f)
            {
                throw new InvalidOperationException ("請先初始化 WebRTC，且不可在關閉後處理 Offer。");
            }
            if (peerConnection != null)
            {
                throw new InvalidOperationException ("目前已有 PeerConnection，無法重複處理 Offer。");
            }

            try
            {
                RTCConfiguration config = EasyWebRTCUtility.GetRTCConfiguration ();
                peerConnection = new RTCPeerConnection (ref config);
                peerConnection.OnConnectionStateChange = state =>
                {
                    if (IsClosed)
                    {
                        return;
                    }
                    ConnectionState = EasyWebRTCNativeChannel.Map (state);
                    foreach (var channel in channels.Values)
                    {
                        channel.SetConnectionState (ConnectionState);
                    }
                    OnConnectionStateChanged?.Invoke (ConnectionState);
                };
                peerConnection.OnDataChannel = native =>
                {
                    if (IsClosed)
                    {
                        native.Close ();
                        native.Dispose ();
                        return;
                    }
                    if (channels.ContainsKey (native.Label))
                    {
                        native.Close ();
                        native.Dispose ();
                        return;
                    }
                    // The native getter uses ushort.MaxValue for reliable channels.
                    int retries = native.MaxRetransmits == ushort.MaxValue ? -1 : native.MaxRetransmits;
                    var channel = new EasyWebRTCNativeChannel (native.Label, native.Ordered, retries);
                    channels.Add (channel.Name, channel);
                    channel.OnDataReceived += ReceiveData;
                    channel.Attach (native);
                    channel.SetConnectionState (ConnectionState);
                    if (defaultChannel == null)
                    {
                        defaultChannel = channel;
                    }
                    OnChannelBound?.Invoke (channel);
                };

                var offerDesc = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = offerSdp };
                var setRemoteOp = peerConnection.SetRemoteDescription (ref offerDesc);
                await UniTask.WaitUntil (
                    () =>
                    {
                        return setRemoteOp.IsDone;
                    });
                if (setRemoteOp.IsError)
                {
                    throw new InvalidOperationException ($"設定遠端 SDP 失敗: {setRemoteOp.Error.message}");
                }

                var answerOp = peerConnection.CreateAnswer ();
                await UniTask.WaitUntil (
                    () =>
                    {
                        return answerOp.IsDone;
                    });
                if (answerOp.IsError)
                {
                    throw new InvalidOperationException ($"建立 Answer 失敗: {answerOp.Error.message}");
                }

                var answerDesc = answerOp.Desc;
                var setLocalOp = peerConnection.SetLocalDescription (ref answerDesc);
                await UniTask.WaitUntil (
                    () =>
                    {
                        return setLocalOp.IsDone;
                    });
                if (setLocalOp.IsError)
                {
                    throw new InvalidOperationException ($"設定本地 SDP 失敗: {setLocalOp.Error.message}");
                }

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
            ConnectionState = EasyWebRTCConnectionState.Closed;
            foreach (var channel in channels.Values)
            {
                channel.OnDataReceived -= ReceiveData;
                channel.SetConnectionState (ConnectionState);
                channel.Close ();
            }
            channels.Clear ();
            defaultChannel = null;
            OnConnectionStateChanged?.Invoke (ConnectionState);
            peerConnection?.Close ();
            peerConnection?.Dispose ();
            peerConnection = null;
        }

        public override void Close ()
        {
            if (IsClosed)
            {
                return;
            }

            base.Close ();
            ClosePeerConnection ();
        }
#endif
    }
}
