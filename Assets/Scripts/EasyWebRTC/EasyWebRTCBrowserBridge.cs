using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kun.Tool
{
    // 用整數 context 將靜態反向 callback 分派回對應的接收元件。
    public class EasyWebRTCBrowserBridge : MonoBehaviour
    {
        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void DataCallback (int context, int channelId, IntPtr pointer, int length);

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void ChannelStateCallback (int context, int channelId, int state);

        [UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        delegate void PeerStateCallback (int context, int state);

        [DllImport ("__Internal")]
        static extern void EasyRTC_Init (string receiver, int context, DataCallback dataCallback,
            ChannelStateCallback channelStateCallback, PeerStateCallback peerStateCallback);

        static readonly Dictionary<int, EasyWebRTCBrowserBridge> receivers = new Dictionary<int, EasyWebRTCBrowserBridge> ();
        static int nextContext;
        // 保留 delegate 參考，避免 JavaScript 還在使用函式指標時被 GC 回收。
        static readonly DataCallback dataCallback = ReceiveData;
        static readonly ChannelStateCallback channelStateCallback = ReceiveChannelState;
        static readonly PeerStateCallback peerStateCallback = ReceivePeerState;

        int context;
        bool registered;
        internal Action<int, IntPtr, int> ChannelData;
        internal Action<int, EasyWebRTCChannelState> ChannelState;
        internal Action<EasyWebRTCConnectionState> PeerState;
        internal Action<string> Failed;
        internal Action<string> OfferCreated;
        internal Action AnswerApplied;

        internal void Initialize ()
        {
            context = checked (++nextContext);
            receivers.Add (context, this);
            registered = true;
            try
            {
                EasyRTC_Init (name, context, dataCallback, channelStateCallback, peerStateCallback);
            }
            catch
            {
                Release ();
                throw;
            }
        }

        internal void Release ()
        {
            if (registered)
            {
                receivers.Remove (context);
                registered = false;
            }
        }

        void OnDestroy ()
        {
            Release ();
        }

        [MonoPInvokeCallback (typeof (DataCallback))]
        static void ReceiveData (int context, int channelId, IntPtr pointer, int length)
        {
            // callback 期間有效的指標不能被保留；managed 例外也不能跨回 WASM。
            try
            {
                if (receivers.TryGetValue (context, out var receiver))
                {
                    receiver.ChannelData?.Invoke (channelId, pointer, length);
                }
                else
                {
                    Debug.LogError ($"找不到 WebRTC receiver context = {context}。");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException (exception);
            }
        }

        [MonoPInvokeCallback (typeof (ChannelStateCallback))]
        static void ReceiveChannelState (int context, int channelId, int state)
        {
            try
            {
                if (receivers.TryGetValue (context, out var receiver))
                {
                    receiver.ChannelState?.Invoke (channelId, (EasyWebRTCChannelState)state);
                }
                else
                {
                    Debug.LogError ($"找不到 WebRTC receiver context = {context}。");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException (exception);
            }
        }

        [MonoPInvokeCallback (typeof (PeerStateCallback))]
        static void ReceivePeerState (int context, int state)
        {
            try
            {
                if (receivers.TryGetValue (context, out var receiver))
                {
                    receiver.PeerState?.Invoke ((EasyWebRTCConnectionState)state);
                }
                else
                {
                    Debug.LogError ($"找不到 WebRTC receiver context = {context}。");
                }
            }
            catch (Exception exception)
            {
                Debug.LogException (exception);
            }
        }

        // SDP 與錯誤屬低頻文字，沿用 SendMessage；二進位資料與狀態不走這裡。
        [Preserve]
        public void OnBrowserError (string message)
        {
            Failed?.Invoke (message);
        }

        [Preserve]
        public void OnBrowserOffer (string sdp)
        {
            OfferCreated?.Invoke (sdp);
        }

        [Preserve]
        public void OnBrowserAnswer (string unused)
        {
            AnswerApplied?.Invoke ();
        }
    }
}
