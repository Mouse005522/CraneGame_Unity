using System;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Kun.Tool
{
    public static class SettingParser
    {
        const string GameSettingFolderName = "GameSettings";

        public static readonly string FolderPath = $"{Application.streamingAssetsPath}/{GameSettingFolderName}";

        /// <summary>
        /// 若檔名與泛型名一樣 則直接套用
        /// subFolderName: 子資料夾名稱
        /// </summary>
        /// <returns>The loader.</returns>
        /// <param name="dataName">Data name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T LoadJson<T> (params string[] subFolderNames) where T : class, new()
        {
            return LoadJson<T> (out _, subFolderNames);
        }

        /// <summary>
        /// 若檔名與泛型名一樣 則直接套用
        /// subFolderName: 子資料夾名稱
        /// </summary>
        /// <returns>The loader.</returns>
        /// <param name="dataName">Data name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T LoadJson<T> (out string rawJson ,params string[] subFolderNames) where T : class, new()
        {
            rawJson = "";
            Type type = typeof (T);
            string dataName = $"{type.Name}.json";

            var folderPath = FolderPath;

            foreach (var subFolderName in subFolderNames)
            {
                folderPath = Path.Combine (folderPath, subFolderName);
            }

            var path = Path.Combine (folderPath, dataName);

            var allLines = "";

            using (UnityWebRequest wwwFile = UnityWebRequest.Get (path))
            {
                var asyncOperation = wwwFile.SendWebRequest ();

                while (asyncOperation.isDone == false)
                {
                    
                }

                if (wwwFile.result != UnityWebRequest.Result.Success)
                {
                    LoggerRouter.Error (wwwFile.error);
                }
                else
                {
                    allLines = wwwFile.downloadHandler.text;
                    rawJson = allLines;
                }
            }

            if (string.IsNullOrEmpty (allLines))
            {
                LoggerRouter.Error ($"無法讀取設定! 請檢查{dataName}是否存在!");
                LoggerRouter.Error (path);
                allLines = "{}";

                return new T ();
            }

            T process = JsonUtility.FromJson<T> (allLines);

            return process;
        }

        /// <summary>
        /// 若檔名與泛型名一樣 則直接套用
        /// </summary>
        /// <returns>The loader.</returns>
        /// <param name="dataName">Data name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static async UniTask<T> LoadJsonAsync<T> (params string[] subFolderNames) where T : class, new()
        {
            Type type = typeof (T);
            string dataName = $"{type.Name}.json";

            var folderPath = FolderPath;

            foreach (var subFolderName in subFolderNames)
            {
                folderPath = Path.Combine (folderPath, subFolderName);
            }

            var path = Path.Combine (folderPath, dataName);

            var allLines = "";

            using (UnityWebRequest wwwFile = UnityWebRequest.Get (path))
            {
                var asyncOperation = wwwFile.SendWebRequest ();

                await UniTask.WaitUntil (() => asyncOperation.isDone);

                if (wwwFile.result != UnityWebRequest.Result.Success)
                {
                    LoggerRouter.Error (wwwFile.error);
                }
                else
                {
                    allLines = wwwFile.downloadHandler.text;
                }
            }

            if (string.IsNullOrEmpty (allLines))
            {
                LoggerRouter.Error ($"無法讀取設定! 請檢查{dataName}是否存在!");
                LoggerRouter.Error (path);
                allLines = "{}";

                return new T ();
            }

            T process = null;

            await UniTask.SwitchToTaskPool ();

            process = JsonUtility.FromJson<T> (allLines);

            await UniTask.SwitchToMainThread ();

            return process;
        }

        public static void SaveJson (object obj, params string[] subFolderNames)
        {
            Type type = obj.GetType ();
            string dataName = $"{type.Name}.json";

            var folderPath = FolderPath;

            foreach (var subFolderName in subFolderNames)
            {
                folderPath = Path.Combine (folderPath, subFolderName);
            }

            var path = Path.Combine (folderPath, dataName);

            string json = JsonUtility.ToJson (obj, true);

            using (StreamWriter writer = new StreamWriter (path, false))
            {
                writer.Write (json);
            }
        }
    }
}
