using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public interface IEasyWebRTCClientBridge
    {
        event Action<EasyWebRTCConnectionState> OnConnectionStateChanged;
        EasyWebRTCConnectionState ConnectionState { get; }
        EasyWebRTCChannel BindChannel (string name, bool ordered, int maxRetransmits);
        event Action<Exception> OnError;
        bool IsConnected { get; }
        void Setup (float iceGatheringTimeoutSeconds = 3f);
        UniTask<string> CreateOffer (CancellationToken cancellationToken);
        UniTask SetAnswer (string sdp, CancellationToken cancellationToken);
        void Close ();
    }
}
