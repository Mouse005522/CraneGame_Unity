using System;
using System.Text;

namespace Kun.Tool
{
    /// <summary>資料僅在同步 callback 期間有效；需要保留時由訂閱者自行複製。</summary>
    public delegate void EasyWebRTCDataReceived (ReadOnlySpan<byte> data);

    public enum EasyWebRTCChannelState
    {
        Connecting = 0,
        Open = 1,
        Closing = 2,
        Closed = 3
    }
    public enum EasyWebRTCConnectionState
    {
        New = 0,
        Connecting = 1,
        Connected = 2,
        Disconnected = 3,
        Failed = 4,
        Closed = 5
    }

    /// <summary>Bind 後即可快取；所有操作與事件皆在 Unity 主執行緒。</summary>
    public abstract class EasyWebRTCChannel : IDisposable
    {
        public string Name { get; }
        public bool Ordered { get; }
        /// <summary>-1 不限重試，0 不重試，正數為重試次數。</summary>
        public int MaxRetransmits { get; }
        public EasyWebRTCChannelState State { get; private set; } = EasyWebRTCChannelState.Connecting;
        public EasyWebRTCConnectionState ConnectionState { get; private set; } = EasyWebRTCConnectionState.New;
        public bool IsConnected => State == EasyWebRTCChannelState.Open && ConnectionState == EasyWebRTCConnectionState.Connected;
        public event EasyWebRTCDataReceived OnDataReceived;
        public event Action<EasyWebRTCChannelState> OnStateChanged;
        public event Action<EasyWebRTCConnectionState> OnConnectionStateChanged;

        protected EasyWebRTCChannel (string name, bool ordered, int maxRetransmits)
        {
            if (string.IsNullOrWhiteSpace (name) || Encoding.UTF8.GetByteCount (name) > ushort.MaxValue)
            {
                throw new ArgumentException ("Channel 名稱不可為空或超過 65535 UTF-8 bytes。", nameof (name));
            }
            if (maxRetransmits < -1 || maxRetransmits > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException (nameof (maxRetransmits));
            }
            Name = name;
            Ordered = ordered;
            MaxRetransmits = maxRetransmits;
        }

        // 指標只在同步 Send 呼叫期間有效，平台實作不可保留它。
        public unsafe bool Send (ReadOnlySpan<byte> data)
        {
            fixed (byte* pointer = data)
            {
                return Send ((IntPtr)pointer, data.Length);
            }
        }

        protected abstract bool Send (IntPtr pointer, int length);
        public abstract void Close ();
        public void Dispose ()
        {
            Close ();
        }

        protected static void ValidateBuffer (IntPtr pointer, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException (nameof (length));
            }
            if (pointer == IntPtr.Zero && length != 0)
            {
                throw new ArgumentNullException (nameof (pointer));
            }
        }

        internal void Receive (ReadOnlySpan<byte> bytes)
        {
            if (State != EasyWebRTCChannelState.Closed)
            {
                OnDataReceived?.Invoke (bytes);
            }
        }

        internal void SetState (EasyWebRTCChannelState state)
        {
            if (State == state || State == EasyWebRTCChannelState.Closed)
            {
                return;
            }
            State = state;
            OnStateChanged?.Invoke (state);
        }

        internal void SetConnectionState (EasyWebRTCConnectionState state)
        {
            if (ConnectionState == state)
            {
                return;
            }
            ConnectionState = state;
            OnConnectionStateChanged?.Invoke (state);
        }
    }
}
