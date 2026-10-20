using UnityEngine;
using UnityEditor;
using System;

namespace Kun.Tool
{
    public class SearchScripts : EditorWindow
    {
		[MenuItem ("Editor/SearchScripts/Check Line")]
		static void ShowSelectWindow ()
		{
            string[] guids = AssetDatabase.FindAssets ("t:Script", new string[] { "Assets/Script" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath (guid);
                TextAsset script = AssetDatabase.LoadAssetAtPath<TextAsset> (path);

                var lineCount = script.text.Split (new[] { '\n' }, StringSplitOptions.None).Length;

                if (lineCount > 450) 
                {
                    Debug.LogError ($"{script.name} -> {lineCount}");
                }
            }
        }
	}
}
