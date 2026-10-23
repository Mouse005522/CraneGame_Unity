using System;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Kun.Tool
{
    public sealed class SettingParser
    {
        public string FolderPath { get; }

        public SettingParser (string folderPath)
        {
            if (string.IsNullOrWhiteSpace (folderPath))
            {
                throw new ArgumentException ("設定目錄不可為空。", nameof (folderPath));
            }

            FolderPath = folderPath;
        }

        /// <summary>
        /// 從目前目錄建立子 scope；不會修改目前 parser，也不會立即建立資料夾。
        /// 每個參數代表一層資料夾，可繼續從回傳的 parser 衍生。
        /// </summary>
        public SettingParser CreateChild (params string[] subFolderNames)
        {
            if (subFolderNames == null || subFolderNames.Length == 0)
            {
                throw new ArgumentException ("至少需要一個子目錄名稱。", nameof (subFolderNames));
            }

            var folderPath = FolderPath;

            foreach (var subFolderName in subFolderNames)
            {
                if (string.IsNullOrWhiteSpace (subFolderName) ||
                    subFolderName == "." || subFolderName == ".." ||
                    subFolderName.IndexOfAny (new[] { '/', '\\', ':' }) >= 0)
                {
                    throw new ArgumentException ("請傳入單層子目錄名稱。", nameof (subFolderNames));
                }

                folderPath = Path.Combine (folderPath, subFolderName);
            }

            return new SettingParser (folderPath);
        }

        /// <summary>
        /// 若檔名與泛型名一樣 則直接套用
        /// subFolderName: 子資料夾名稱
        /// </summary>
        /// <returns>The loader.</returns>
        /// <param name="dataName">Data name.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T LoadJson<T> (params string[] subFolderNames) where T : class, new()
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
        public T LoadJson<T> (out string rawJson ,params string[] subFolderNames) where T : class, new()
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
        public async UniTask<T> LoadJsonAsync<T> (params string[] subFolderNames) where T : class, new()
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

        public void SaveJson (object obj, params string[] subFolderNames)
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

            Directory.CreateDirectory (folderPath);

            using (StreamWriter writer = new StreamWriter (path, false))
            {
                writer.Write (json);
            }
        }
    }
}
