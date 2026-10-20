using System;
using UnityEngine;

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
        Server,
        Client
    }
}
