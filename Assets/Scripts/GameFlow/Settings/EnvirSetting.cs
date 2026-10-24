using System;

namespace Kun.Tool 
{
    [Serializable]
    public class EnvirSetting
    {
        public EntryType entryType;
        public bool DisplayDebugGUI;
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
