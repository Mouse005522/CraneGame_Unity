using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.WebRTC;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Kun.Tool
{
    public class EasyWebRTCBase : MonoBehaviour
    {
        protected string Ip { get; private set; }
        protected int Port { get; private set; }

        protected const string SDP_OFFER_PATH = "/sdp-offer";

        public virtual void Setup (string ip, int port)
        {
            Ip = ip;
            Port = port;

            
        }

        public virtual void Close () 
        {

        }
    }
}