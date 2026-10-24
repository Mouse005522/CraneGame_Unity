using System;
using UnityEngine;
#if !UNITY_WEBGL || UNITY_EDITOR
using Unity.WebRTC;
#endif
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public class EasyWebRTCBase : MonoBehaviour
    {
        Coroutine updateCoroutine;
        protected bool IsClosed { get; private set; }
        protected float IceGatheringTimeoutSeconds { get; private set; }
        bool setup;
        protected virtual bool RequiresNativeUpdate => true;

        public event EasyWebRTCDataReceived OnDataReceived;
        public virtual bool IsConnected => false;

        /// <summary>Call on Unity's main thread. Returns false when disconnected.</summary>
        public virtual bool Send (byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException (nameof (data));
            }
            return false;
        }

        protected void ReceiveData (ReadOnlySpan<byte> data)
        {
            OnDataReceived?.Invoke (data);
        }

        protected virtual void OnDestroy ()
        {
            Close ();
        }

        public virtual void Setup (float iceGatheringTimeoutSeconds = 3f)
        {
            if (setup || IsClosed)
            {
                throw new InvalidOperationException ("WebRTC 已初始化或已關閉。");
            }
            if (float.IsNaN (iceGatheringTimeoutSeconds) || float.IsInfinity (iceGatheringTimeoutSeconds) || iceGatheringTimeoutSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException (nameof (iceGatheringTimeoutSeconds));
            }

            IceGatheringTimeoutSeconds = iceGatheringTimeoutSeconds;
            setup = true;
#if !UNITY_WEBGL || UNITY_EDITOR
            if (RequiresNativeUpdate)
            {
                updateCoroutine = StartCoroutine (WebRTC.Update ());
            }
#endif
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        protected async UniTask WaitForIceGathering (RTCPeerConnection connection)
        {
            float startedAt = Time.realtimeSinceStartup;
            while (!IsClosed && connection.GatheringState != RTCIceGatheringState.Complete)
            {
                if (Time.realtimeSinceStartup - startedAt >= IceGatheringTimeoutSeconds)
                {
                    throw new TimeoutException ("ICE 收集未完成，無法送出完整 SDP。");
                }
                await UniTask.Yield ();
            }

            if (IsClosed)
            {
                throw new InvalidOperationException ("WebRTC 已關閉。");
            }
        }

#endif
        public virtual void Close ()
        {
            if (IsClosed)
            {
                return;
            }

            IsClosed = true;
            if (updateCoroutine != null)
            {
                StopCoroutine (updateCoroutine);
                updateCoroutine = null;
            }
        }
    }
}
