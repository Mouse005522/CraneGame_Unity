using System;

namespace Kun.Tool
{
    [Serializable]
    public class ClientSetting
    {
        // 可填 IP、主機名稱或完整 HTTP(S) URL；API 的 port 包含在 URL 中。
        public string roomServerIp = "http://127.0.0.1:8088";
        public float iceGatheringTimeoutSeconds = 3f;
    }
}
