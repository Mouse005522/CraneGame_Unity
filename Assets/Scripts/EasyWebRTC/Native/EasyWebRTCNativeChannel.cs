#if !UNITY_WEBGL || UNITY_EDITOR
using System;
using Unity.WebRTC;

namespace Kun.Tool
{
    public sealed class EasyWebRTCNativeChannel : EasyWebRTCChannel
    {
        RTCDataChannel channel;
        public EasyWebRTCNativeChannel (string name, bool ordered, int maxRetransmits) : base (name, ordered, maxRetransmits)
        {
        }

        internal void Attach (RTCDataChannel nativeChannel, Action<Exception> onError = null)
        {
            channel = nativeChannel;
            channel.OnOpen = () =>
            {
                SetState (EasyWebRTCChannelState.Open);
            };
            channel.OnClose = () =>
            {
                SetState (EasyWebRTCChannelState.Closed);
            };
            channel.OnMessage = bytes =>
            {
                Receive (bytes);
            };
            channel.OnError = error =>
            {
                var exception = new InvalidOperationException (error.message);
                onError?.Invoke (exception);
            };
            switch (channel.ReadyState)
            {
                case RTCDataChannelState.Open:
                    SetState (EasyWebRTCChannelState.Open);
                    break;
                case RTCDataChannelState.Closing:
                    SetState (EasyWebRTCChannelState.Closing);
                    break;
                case RTCDataChannelState.Closed:
                    SetState (EasyWebRTCChannelState.Closed);
                    break;
            }
        }

        protected override bool Send (IntPtr pointer, int length)
        {
            ValidateBuffer (pointer, length);
            if (!IsConnected || channel == null)
            {
                return false;
            }
            channel.Send (pointer, length);
            return true;
        }

        public override void Close ()
        {
            if (channel != null)
            {
                var current = channel;
                channel = null;
                current.OnOpen = null;
                current.OnClose = null;
                current.OnMessage = null;
                current.OnError = null;
                current.Close ();
                current.Dispose ();
            }
            SetState (EasyWebRTCChannelState.Closed);
        }

        internal static EasyWebRTCConnectionState Map (RTCPeerConnectionState state)
        {
            switch (state)
            {
                case RTCPeerConnectionState.Connecting:
                    return EasyWebRTCConnectionState.Connecting;
                case RTCPeerConnectionState.Connected:
                    return EasyWebRTCConnectionState.Connected;
                case RTCPeerConnectionState.Disconnected:
                    return EasyWebRTCConnectionState.Disconnected;
                case RTCPeerConnectionState.Failed:
                    return EasyWebRTCConnectionState.Failed;
                case RTCPeerConnectionState.Closed:
                    return EasyWebRTCConnectionState.Closed;
                default:
                    return EasyWebRTCConnectionState.New;
            }
        }
    }
}
#endif
