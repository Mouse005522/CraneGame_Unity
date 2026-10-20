using Unity.WebRTC;
using UnityEngine;

public static class EasyWebRTCUtility
{
    /// <summary>
    /// 建立乾淨的 RTC 組態（直連 AWS，不需要 STUN/TURN）
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
