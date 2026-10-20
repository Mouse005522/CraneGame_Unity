using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Kun.Tool
{
    //for GUILayout還有GUI用的
    //不會有Editor依賴
    public static class GUITool
    {
        public static Vector3 DrawVector3 (Vector3 value, string fieldName)
        {
            if (string.IsNullOrEmpty (fieldName) == false) 
            {
                GUILayout.Label (fieldName);
            }

            GUITool.DrawInHorizontal (() =>
            {
                GUILayout.Label ("X", GUILayout.Width (12f));
                value.x = FloatField ("", value.x, GUILayout.Width (50f));
                GUILayout.Space (10f);

                GUILayout.Label ("Y", GUILayout.Width (12f));
                value.y = FloatField ("", value.y, GUILayout.Width (50f));
                GUILayout.Space (10f);

                GUILayout.Label ("Z", GUILayout.Width (12f));
                value.z = FloatField ("", value.z, GUILayout.Width (50f));
                GUILayout.Space (10f);
            });

            return value;
        }

        public static bool FolderOut (bool isFoldoutOpen, string title) 
        {
            var msg = isFoldoutOpen ? $"▼ {title}" : $"▶ {title}";
            isFoldoutOpen = GUILayout.Toggle (isFoldoutOpen, msg);

            return isFoldoutOpen;
        }

        public static void DrawInHorizontal (Action body, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style == null)
            {
                GUILayout.BeginHorizontal (options);
            }
            else
            {
                GUILayout.BeginHorizontal (style, options);
            }

            body.Invoke ();
            GUILayout.EndHorizontal ();
        }

        public static int IntField (string title, int intValue, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal (); // 水平布局，將標題和輸入欄位放在一行
            GUILayout.Label (title);      // 繪製標題
            string input = intValue.ToString ();
            string newInput = GUILayout.TextField (input, options); // 繪製文本輸入框
            GUILayout.EndHorizontal ();

            // 嘗試將輸入轉換為整數
            if (int.TryParse (newInput, out int parsedValue))
            {
                return parsedValue;
            }

            // 如果轉換失敗，返回原始值
            return intValue;
        }

        public static float FloatField (string title, float floatValue, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal (); // 水平布局，將標題和輸入欄位放在一行
            if (string.IsNullOrEmpty (title) == false) 
            {
                GUILayout.Label (title);      // 繪製標題
            }
            string input = floatValue.ToString ();
            string newInput = GUILayout.TextField (input, options); // 繪製文本輸入框
            GUILayout.EndHorizontal ();

            // 嘗試將輸入轉換為整數
            if (float.TryParse (newInput, out float parsedValue))
            {
                return parsedValue;
            }

            // 如果轉換失敗，返回原始值
            return floatValue;
        }

        public static void DrawInVertical (Action body, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style == null)
            {
                GUILayout.BeginVertical (options);
            }
            else
            {
                GUILayout.BeginVertical (style, options);
            }

            body.Invoke ();
            GUILayout.EndVertical ();
        }

        public static void DrawInReadOnly (Action body)
        {
            GUI.enabled = false;
            body.Invoke ();
            GUI.enabled = true;
        }

        public static void DrawInIndent (Action body)
        {
            GUITool.DrawInHorizontal (() => 
            {
                GUILayout.Space (10);

                GUITool.DrawInVertical (() => 
                {
                    body.Invoke ();
                });
            });
        }

        public static Vector2 DrawInScrollView (Vector2 scorllPosition, float scrollBarHeight, float scrollBarWith, Action body)
        {
            Vector2 newPosition;

            using (var scrollView = new GUILayout.ScrollViewScope (scorllPosition, new GUIStyle (), GUILayout.Height (scrollBarHeight), GUILayout.Width (scrollBarWith)))
            {
                newPosition = scrollView.scrollPosition;

                body.Invoke ();
            }

            return newPosition;
        }

        public static Vector2 DrawInScrollView (Vector2 scorllPosition, float scrollBarHeight, Action body)
        {
            Vector2 newPosition;

            using (var scrollView = new GUILayout.ScrollViewScope (scorllPosition, GUILayout.Height (scrollBarHeight)))
            {
                newPosition = scrollView.scrollPosition;

                body.Invoke ();
            }

            return newPosition;
        }

        public static Vector2 DrawInScrollView (Vector2 scorllPosition, Action body, GUIStyle guiStyle, params GUILayoutOption[] options)
        {
            Vector2 newPosition;

            using (var scrollView = new GUILayout.ScrollViewScope (scorllPosition, guiStyle, options))
            {
                newPosition = scrollView.scrollPosition;

                body.Invoke ();
            }

            return newPosition;
        }

        public static Vector2 DrawInScrollView (Vector2 scorllPosition, Action body, params GUILayoutOption[] options)
        {
            Vector2 newPosition;

            using (var scrollView = new GUILayout.ScrollViewScope (scorllPosition, options))
            {
                newPosition = scrollView.scrollPosition;

                body.Invoke ();
            }

            return newPosition;
        }
    }
}

