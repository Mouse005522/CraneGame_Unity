using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kun.Tool
{
    public static class FolderWatcherManager
    {
        public static FolderWatcher Create (string folderPath, Action<Action> dispatcher)
        {
            if (string.IsNullOrEmpty (folderPath))
            {
                throw new ArgumentException ($"folderPath 不可為空 -> {folderPath}");
            }

            if (Directory.Exists (folderPath) == false) 
            {
                throw new ArgumentException ($"folderPath 不存在 -> {folderPath}");
            }

            return new FolderWatcher (folderPath, dispatcher);
        }
    }

    public class FolderWatcher
    {
        readonly FileSystemWatcher watcher;

        Action<Action> dispatcher;

        public FolderWatcher (string folderPath, Action<Action> dispatcher)
        {
            this.dispatcher = dispatcher;

            watcher = new FileSystemWatcher (folderPath)
            {
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite
            };

            watcher.Changed += OnFileEvent;
        }

        Dictionary<string, Action> callbackTable = new Dictionary<string, Action> ();

        public void BindWatcher<T> (Action callback)
        {
            var fileName = typeof (T).Name;

            callbackTable[fileName] = callback;
        }

        void OnFileEvent (object sender, FileSystemEventArgs e)
        {
            var fileName = Path.GetFileNameWithoutExtension (e.Name);

            if (callbackTable.TryGetValue (fileName, out var callback))
            {
                dispatcher.Invoke (callback);
            }
        }

        

        public void Dispose ()
        {

            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Changed -= OnFileEvent;
                watcher.Dispose ();
            }
        }
    }
}