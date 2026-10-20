using Cysharp.Threading.Tasks;
using Kun.Tool;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Kun.Tool
{

    /// <summary>
    /// 獨立不雅詞檢查器
    /// - 詞庫載入時統一正規化（Leet + 分隔符號移除），查找時同步正規化輸入
    /// - HashSet 查找 O(1)，初始化後唯讀，支援多執行緒並行查找
    /// </summary>
    public class ProfanityChecker
    {
        static readonly Dictionary<char, char> leetMap = new Dictionary<char, char>
        {
            { '0', 'o' },
            { '1', 'i' },
            { '2', 'z' },
            { '3', 'e' },
            { '4', 'a' },
            { '5', 's' },
            { '7', 't' },
            { '@', 'a' },
            { '!', 'i' },
            { '$', 's' },
            { '+', 't' },
        };

        static readonly char[] separators = new char[] { '-', '_', ' ' };

        // 初始化完成後唯讀，HashSet 唯讀操作本身執行緒安全
        HashSet<string> profanitySet = new HashSet<string> ();

        int minWordLen = 1;

        public bool IsReady { get; private set; } = false;

        public int Count => profanitySet.Count;

        /// <summary>
        /// 非同步從多個路徑載入詞庫，完成後才可呼叫 IsProfanity / ContainsProfanityAsync
        /// 載入時統一對詞庫詞做正規化，與查找時的處理保持一致
        /// </summary>
        public async UniTask InitAsync (params string[] paths)
        {
            var words = new HashSet<string> ();

            await UniTask.SwitchToTaskPool ();

            foreach (var path in paths)
            {
                await LoadFromFileAsync (path, words);
            }

            await UniTask.SwitchToMainThread ();

            profanitySet = words;

            minWordLen = int.MaxValue;

            foreach (var w in profanitySet)
            {
                if (w.Length < minWordLen)
                {
                    minWordLen = w.Length;
                }
            }

            if (minWordLen == int.MaxValue)
            {
                minWordLen = 1;
            }

            IsReady = true;
        }

        async UniTask LoadFromFileAsync (string path, HashSet<string> target)
        {
            if (File.Exists (path) == false)
            {
                LoggerRouter.Error ($"[ProfanityChecker] 找不到詞庫檔案，略過 -> {path}");
                return;
            }

            int added = 0;

            using (var reader = new StreamReader (path, Encoding.UTF8))
            {
                string line;

                while ((line = await reader.ReadLineAsync ()) != null)
                {
                    var raw = line.Trim ().ToLowerInvariant ();

                    if (string.IsNullOrEmpty (raw) || raw.StartsWith ("#"))
                    {
                        continue;
                    }

                    // 載入時統一正規化：Leet + 移除分隔符號
                    // 與 Normalize() 邏輯保持一致，確保查找時能對齊
                    var normalized = Normalize (raw);

                    if (target.Add (normalized))
                    {
                        added++;
                    }
                }
            }

            LoggerRouter.Log ($"[ProfanityChecker] 載入詞庫完成，共 {added} 筆 -> {path}");
        }

        /// <summary>
        /// 統一正規化：ToLower + Leet Speak 替換 + 移除分隔符號
        /// 詞庫載入與查找時共用同一套邏輯，確保對齊
        /// </summary>
        public static string Normalize (string input)
        {
            if (string.IsNullOrEmpty (input))
            {
                return input;
            }

            var sb = new StringBuilder (input.Length);

            foreach (char c in input.ToLowerInvariant ())
            {
                if (IsSeparator (c))
                {
                    continue;
                }

                sb.Append (leetMap.TryGetValue (c, out char mapped) ? mapped : c);
            }

            return sb.ToString ();
        }

        static bool IsSeparator (char c)
        {
            foreach (char sep in separators)
            {
                if (c == sep)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 精確比對：輸入本身是否為不雅詞
        /// HashSet O(1)，同步即可，不需切執行緒
        /// </summary>
        public bool IsProfanity (string word)
        {
            if (string.IsNullOrEmpty (word))
            {
                return false;
            }

            return profanitySet.Contains (Normalize (word));
        }

        /// <summary>
        /// 包含比對：滑動視窗切出所有子字串，用 HashSet O(1) 查找
        /// 切到 ThreadPool 執行避免阻塞主執行緒，完成後切回 MainThread
        /// </summary>
        public async UniTask<bool> ContainsProfanityAsync (string term)
        {
            if (string.IsNullOrWhiteSpace (term))
            {
                return false;
            }

            await UniTask.SwitchToThreadPool ();

            bool result = ContainsProfanityInternal (term);

            await UniTask.SwitchToMainThread ();

            return result;
        }

        bool ContainsProfanityInternal (string term)
        {
            // 輸入與詞庫都經過相同的 Normalize，直接滑窗比對即可
            string normalized = Normalize (term);

            int inputLen = normalized.Length;

            for (int windowSize = minWordLen; windowSize <= inputLen; windowSize++)
            {
                for (int i = 0; i <= inputLen - windowSize; i++)
                {
                    string sub = normalized.Substring (i, windowSize);

                    if (profanitySet.Contains (sub))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
