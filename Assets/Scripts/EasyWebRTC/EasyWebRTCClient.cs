using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unity.WebRTC;
using System;
using Cysharp.Threading.Tasks;

public class EasyWebRTCClient : MonoBehaviour
{
    [SerializeField]
    public string serverIp = "your-aws-domain-or-ip.com";
    public int serverPort = 8080;

    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;

    void Start ()
    {
        StartCoroutine (WebRTC.Update ());
        ConnectToTargetServer ().Forget ();
    }

    private async UniTaskVoid ConnectToTargetServer ()
    {
        RTCConfiguration config = EasyWebRTCUtility.GetRTCConfiguration ();

        peerConnection = new RTCPeerConnection (ref config);

        peerConnection.OnConnectionStateChange = state =>
        {
            Debug.Log ($"[Client] PeerConnection 狀態變更: {state}");
        };

        // 建立 RUDP DataChannel
        var channelInit = new RTCDataChannelInit ();
        dataChannel = peerConnection.CreateDataChannel ("rudp-channel", channelInit);

        dataChannel.OnOpen = () =>
        {
            Debug.Log ($"[Client] 🎉 RUDP 通道已成功開啟！狀態: {dataChannel.ReadyState}");
            byte[] msg = Encoding.UTF8.GetBytes ("Hello from Client!");
            dataChannel.Send (msg);
        };

        dataChannel.OnMessage = bytes =>
        {
            string reply = Encoding.UTF8.GetString (bytes);
            Debug.Log ($"[Client 收到回應]: {reply}");
        };

        dataChannel.OnClose = () =>
        {
            Debug.Log ($"[Client] RUDP 通道已關閉");
        };

        // 建立 Offer
        var offerOp = peerConnection.CreateOffer ();
        await UniTask.WaitUntil (() => offerOp.IsDone);

        if (offerOp.IsError)
        {
            Debug.LogError ("[Client] 建立 Offer 失敗");
            return;
        }

        var offerDesc = offerOp.Desc;
        var setLocalOp = peerConnection.SetLocalDescription (ref offerDesc);
        await UniTask.WaitUntil (() => setLocalOp.IsDone);

        // 等待 ICE 收集完成（因為是連 AWS 固定的 IP，本地直連通常瞬間完成）
        float timer = 0f;
        while (peerConnection.GatheringState != RTCIceGatheringState.Complete)
        {
            timer += Time.deltaTime;
            if (timer > 3f) break;
            await UniTask.Yield ();
        }

        offerDesc = peerConnection.LocalDescription;

        // 透過 HTTP POST 將 Offer SDP 傳送給 AWS Server
        string serverUrl = $"http://{serverIp}:{serverPort}/offer/";
        using (UnityWebRequest www = new UnityWebRequest (serverUrl, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes (offerDesc.sdp);
            www.uploadHandler = new UploadHandlerRaw (bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer ();
            www.SetRequestHeader ("Content-Type", "text/plain");

            await www.SendWebRequest ();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string answerSdp = www.downloadHandler.text;
                var answerDesc = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = answerSdp };

                var setRemoteOp = peerConnection.SetRemoteDescription (ref answerDesc);
                await UniTask.WaitUntil (() => setRemoteOp.IsDone);

                Debug.Log ("[Client] 已成功與 Server 完成交握！");
            }
            else
            {
                Debug.LogError ($"[Client] 無法連線至 Server: {www.error}");
            }
        }
    }

    void Update ()
    {
        // 按下空白鍵發送測試 RUDP 訊息
        if (Input.GetKeyDown (KeyCode.Space) && dataChannel != null && dataChannel.ReadyState == RTCDataChannelState.Open)
        {
            byte[] msg = Encoding.UTF8.GetBytes ("Space pressed from Client!");
            unsafe
            {
                var span = msg.AsSpan ();
                fixed (byte* ptr = span)
                {
                    dataChannel.Send (ptr, span.Length);
                }
            }
            Debug.Log ("[Client] 已發送 RUDP 訊息");
        }
    }

    private void OnDestroy ()
    {
        dataChannel?.Close ();
        peerConnection?.Close ();
    }
}