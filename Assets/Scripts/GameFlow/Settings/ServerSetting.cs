using System;

namespace Kun.Tool
{
    [Serializable]
    public class ServerSetting
    {
        public float iceGatheringTimeoutSeconds = 3f;
        public string gameServerPipleName = "CraneGameServerPipe";
        public string apiServerPipleName = "CraneGameApiPipe";
        public float TimeoutSeconds = 5f;
    }
}
