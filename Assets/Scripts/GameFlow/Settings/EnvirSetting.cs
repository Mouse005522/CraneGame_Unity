using System;
using UnityEngine;

namespace Kun.Tool 
{
    [Serializable]
    public class EnvirSetting
    {
        public EntryType entryType;
        public bool DisplayDebugGUI;

        public string serverIp = "127.0.0.1";
        public int serverPort = 8088;
    }

    public enum EntryType
    {
        Server = 1,
        Client = 2,
        /// <summary>
        /// 測試用二合一模式，僅用於本地測試，正式發佈時請勿使用
        /// </summary>
        Host = Server | Client
    }
}
