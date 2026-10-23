using Unity.WebRTC;
using UnityEngine;

public static class EasyWebRTCUtility
{
    /// <summary>
    /// 建立僅使用本地候選位址的 RTC 組態；跨 NAT／公網部署時需依網路環境設定 STUN/TURN。
    /// </summary>
    /// <returns></returns>
    public static RTCConfiguration GetRTCConfiguration ()
    {
        return new RTCConfiguration
        {
            iceServers = new RTCIceServer[0]
        };
    }
}
