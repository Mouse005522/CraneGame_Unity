using System;
using UnityEngine;
using Unity.WebRTC;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public class EasyWebRTCBase : MonoBehaviour
    {
        Coroutine updateCoroutine;
        protected bool IsClosed { get; private set; }
        protected float IceGatheringTimeoutSeconds { get; private set; }
        bool setup;

        public virtual void Setup (float iceGatheringTimeoutSeconds = 3f)
        {
            if (setup || IsClosed)
                throw new InvalidOperationException ("WebRTC 已初始化或已關閉。");
            if (float.IsNaN (iceGatheringTimeoutSeconds) || float.IsInfinity (iceGatheringTimeoutSeconds) || iceGatheringTimeoutSeconds <= 0f)
                throw new ArgumentOutOfRangeException (nameof (iceGatheringTimeoutSeconds));

            IceGatheringTimeoutSeconds = iceGatheringTimeoutSeconds;
            setup = true;
            updateCoroutine = StartCoroutine (WebRTC.Update ());
        }

        protected async UniTask WaitForIceGathering (RTCPeerConnection connection)
        {
            float startedAt = Time.realtimeSinceStartup;
            while (!IsClosed && connection.GatheringState != RTCIceGatheringState.Complete)
            {
                if (Time.realtimeSinceStartup - startedAt >= IceGatheringTimeoutSeconds)
                    throw new TimeoutException ("ICE 收集未完成，無法送出完整 SDP。");
                await UniTask.Yield ();
            }

            if (IsClosed)
                throw new InvalidOperationException ("WebRTC 已關閉。");
        }

        public virtual void Close ()
        {
            if (IsClosed)
                return;

            IsClosed = true;
            if (updateCoroutine != null)
            {
                StopCoroutine (updateCoroutine);
                updateCoroutine = null;
            }
        }
    }
}
