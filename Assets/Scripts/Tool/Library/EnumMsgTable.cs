using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kun.Tool
{
    public static class EnumMsgExtension 
    {
        /// <summary>
        /// 透過掛載EnumMsg綁定Msg
        /// </summary>
        /// <returns></returns>
        public static string GetMsg<T> (this T item) where T : Enum
        {
            return EnumMsgTable<T>.GetMsg (item);
        }

        public static string[] GetMsgs<T> () where T:Enum
        {
            return EnumMsgTable<T>.GetMsgs ();
        }

        /// <summary>
        /// 回傳 enum 各項對應的 int 值陣列，順序與 GetMsgs 一致
        /// </summary>
        public static int[] GetMsgIntValues<T> () where T : Enum
        {
            return EnumMsgTable<T>.GetIntValues ();
        }

        /// <summary>
        /// 透過掛載EnumColor綁定Color
        /// </summary>
        /// <returns></returns>
        public static Color GetColor<T> (this T item) where T : Enum
        {
            return EnumColorTable<T>.GetColor (item);
        }
    }

    public static class EnumMsgTable<T> where T : Enum
    {
        public static string GetMsg (T item)
        {
            return msgTable[item];
        }

        public static string[] GetMsgs ()
        {
            return msgs;
        }

        /// <summary>
        /// 回傳 enum 各項對應的 int 值陣列，順序與 GetMsgs 一致
        /// </summary>
        public static int[] GetIntValues ()
        {
            return intValues;
        }

        static EnumMsgTable () 
        {
            msgTable = new SortedDictionary<T, string> ();

            Type enumType = typeof (T);

            EnumTool.GetIter<T> ().ToList ().ForEach (item=>
            {
                FieldInfo fieldInfo = enumType.GetField (item.ToString ());

                EnumMsg enumMsg = fieldInfo.GetCustomAttribute<EnumMsg> ();

                if (enumMsg != null) 
                {
                    msgTable.Add (item, enumMsg.msg);
                }
                else
                {
                    msgTable.Add (item, item.ToString ());
                }
            });

            msgs = msgTable.Values.ToArray ();
            intValues = msgTable.Keys.Select (k => Convert.ToInt32 (k)).ToArray ();
        }

        static SortedDictionary<T, string> msgTable = new SortedDictionary<T, string> ();
        static string[] msgs;
        static int[] intValues;
    }

    public class EnumMsg : Attribute
    {
        public EnumMsg (string msg) 
        {
            this.msg = msg;
        }

        public string msg;
    }

    public static class EnumColorTable<T> where T : Enum
    {
        public static Color GetColor (T item)
        {
            return msgTable[item];
        }

        static EnumColorTable ()
        {
            msgTable = new Dictionary<T, Color> ();

            Type enumType = typeof (T);

            EnumTool.GetIter<T> ().ToList ().ForEach (item =>
            {
                FieldInfo fieldInfo = enumType.GetField (item.ToString ());

                EnumColor enumColor = fieldInfo.GetCustomAttribute<EnumColor> ();

                if (enumColor != null)
                {
                    msgTable.Add (item, enumColor.enumColor);
                }
                else
                {
                    Debug.LogError ($"{item} not set color");
                    msgTable.Add (item, Color.black);
                }
            });
        }

        static Dictionary<T, Color> msgTable = new Dictionary<T, Color> ();
    }

    public class EnumColor : Attribute
    {
        public EnumColor (string colorName)
        {
            this.enumColor = ColorMsgTable.GetColor (colorName);
        }

        public Color enumColor;
    }

    public static class ColorMsgTable 
    {
        static Dictionary<string, Color> proxyTable;

        public static Color GetColor (string msg) 
        {
            return proxyTable[msg];
        }

        static ColorMsgTable () 
        {
            proxyTable = new Dictionary<string, Color> ();
            proxyTable.Add (nameof (Color.yellow), Color.yellow);
            proxyTable.Add (nameof (Color.clear), Color.clear);
            proxyTable.Add (nameof (Color.grey), Color.grey);
            proxyTable.Add (nameof (Color.gray), Color.gray);
            proxyTable.Add (nameof (Color.magenta), Color.magenta);
            proxyTable.Add (nameof (Color.cyan), Color.cyan);
            proxyTable.Add (nameof (Color.red), Color.red);
            proxyTable.Add (nameof (Color.black), Color.black);
            proxyTable.Add (nameof (Color.white), Color.white);
            proxyTable.Add (nameof (Color.blue), Color.blue);
            proxyTable.Add (nameof (Color.green), Color.green);
        }
    }
}
