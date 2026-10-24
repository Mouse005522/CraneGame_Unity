using System;
using System.Runtime.InteropServices;

namespace Kun.Tool
{
    public sealed class EasyWebRTCWebGLChannel : EasyWebRTCChannel
    {
        [DllImport ("__Internal")] static extern int EasyRTC_Send (string receiver, int id, IntPtr pointer, int length);
        [DllImport ("__Internal")] static extern void EasyRTC_CloseChannel (string receiver, int id);
        string receiver;
        internal int Id { get; }
        public EasyWebRTCWebGLChannel (int id, string name, bool ordered, int maxRetransmits) : base (name, ordered, maxRetransmits)
        {
            Id = id;
        }

        internal void Attach (string target)
        {
            receiver = target;
        }

        protected override bool Send (IntPtr pointer, int length)
        {
            ValidateBuffer (pointer, length);
            return IsConnected && receiver != null && EasyRTC_Send (receiver, Id, pointer, length) != 0;
        }

        public override void Close ()
        {
            if (receiver != null)
            {
                string target = receiver;
                receiver = null;
                EasyRTC_CloseChannel (target, Id);
            }
            SetState (EasyWebRTCChannelState.Closed);
        }
    }
}
