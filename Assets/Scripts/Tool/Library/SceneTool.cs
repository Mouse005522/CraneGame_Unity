using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public static class SceneTool
    {
        const string MainCamera = "MainCamera";

        public static async UniTask<Camera> FindMainCameraAsync (this Scene scene, FindPattern findPattern = FindPattern.DFS)
        {
            var cam = await FindObjectOfTypeAsync<Camera> (scene, findPattern, (cam) =>
            {
                return cam.tag == MainCamera;
            });

            return cam;
        }

        /// <summary>
        /// 尋找主攝影機的統一異步入口
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="findCam"></param>
        public static void AddFindMainCameraTask (this BatchSchedule<GameObject> schedule, Action<Camera> findCam)
        {
            schedule.AddFindComTask (findCam, true, (cam) =>
            {
                return cam.tag == MainCamera;
            });
        }

        /// <summary>
        /// 沒有拘束是為了配合interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static async UniTask<List<T>> FindObjectOfTypesAsync<T> (this Scene scene, FindPattern findPattern = FindPattern.DFS, Predicate<T> filter = null)
        {
            BatchSchedule<GameObject> sceneGoSchedule = CreateSceneSchedule (scene, findPattern);

            GetComponentTask<T> getComponentTask = new GetComponentTask<T> ();
            getComponentTask.Setup (true, filter);

            sceneGoSchedule.AddTask (getComponentTask);

            await sceneGoSchedule.RunTask ();

            var results = getComponentTask.results;

            return results;
        }

        /// <summary>
        /// 沒有拘束是為了配合interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static async UniTask<T> FindObjectOfTypeAsync<T> (this Scene scene, FindPattern findPattern = FindPattern.DFS, Predicate<T> filter = null)
        {
            BatchSchedule<GameObject> sceneGoSchedule = CreateSceneSchedule (scene, findPattern);

            GetComponentTask<T> getComponentTask = new GetComponentTask<T> ();
            getComponentTask.Setup (true, filter);

            sceneGoSchedule.AddTask (getComponentTask);

            await sceneGoSchedule.RunTask ();

            var results = getComponentTask.results;

            if (results.Count > 0)
            {
                if (results.Count > 1)
                {
                    LoggerRouter.Error ($"搜尋 {typeof (T)} 發現多個, 取用第一個");
                }

                return results[0];
            }
            else
            {
                LoggerRouter.Error ($"搜尋 {typeof (T)} 失敗");
                return default;
            }
        }

        public static void AddFindComTask<T> (this BatchSchedule<GameObject> schedule, Action<T> findCom, bool needError = true, Predicate<T> filter = null)
        {
            GetComponentTask<T> getComponentTask = new GetComponentTask<T> ();
            getComponentTask.Setup (true, filter);

            schedule.AddTask (getComponentTask, () =>
            {
                bool hasResult = false;

                var results = getComponentTask.results;

                T result = default;

                if (results.Count > 0)
                {
                    if (results.Count > 1)
                    {
                        if (needError) 
                        {
                            LoggerRouter.Error ($"搜尋 {typeof (T)} 發現多個, 取用第一個");
                        }
                    }

                    result = results[0];
                    hasResult = true;
                }
                else
                {
                    if (needError)
                    {
                        LoggerRouter.Error ($"搜尋 {typeof (T)} 失敗");
                    }

                    result = default;
                    hasResult = false;
                }

                if (hasResult)
                {
                    try
                    {
                        findCom.Invoke (result);
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Exception (e);
                    }
                }
            });
        }

        public static void AddFindComsTask<T> (this BatchSchedule<GameObject> schedule, Action<List<T>> findCom, Predicate<T> filter = null)
        {
            GetComponentTask<T> getComponentTask = new GetComponentTask<T> ();
            getComponentTask.Setup (false, filter);

            schedule.AddTask (getComponentTask, () =>
            {
                var results = getComponentTask.results.ToList ();

                try
                {
                    findCom.Invoke (results);
                }
                catch (Exception e)
                {
                    LoggerRouter.Exception (e);
                }
            });
        }

        /// <summary>
        /// 沒有拘束是為了配合interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static async UniTask<List<T>> FindObjectsOfTypeAsync<T> (this Scene scene, FindPattern findPattern = FindPattern.DFS)
        {
            BatchSchedule<GameObject> sceneGoSchedule = CreateSceneSchedule (scene, findPattern);

            GetComponentTask<T> getComponentTask = new GetComponentTask<T> ();
            sceneGoSchedule.AddTask (getComponentTask);

            await sceneGoSchedule.RunTask ();

            return getComponentTask.results;
        }

        /// <summary>
        /// 需要一次遞迴中尋找複數型別的Com的時候
        /// 需要建立排程再註冊完後再一口氣執行
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="findPattern"></param>
        /// <returns></returns>
        public static BatchSchedule<GameObject> CreateSceneSchedule (this Scene scene, FindPattern findPattern = FindPattern.BFS)
        {
            BatchSchedule<GameObject> sceneGoSchedule = BatchSchedule<GameObject>.CreateScehedule (AsyncCycleSceneGoGetter (scene, findPattern));

            return sceneGoSchedule;
        }

        /// <summary>
        /// 透過遞迴的方式異步取得所有場上所有Go
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        static async IAsyncEnumerable<GameObject> AsyncCycleSceneGoGetter (Scene scene, FindPattern findPattern)
        {
            int count = 0;

            var rootGos = scene.GetRootGameObjects ().ToList ();

            if (findPattern == FindPattern.OnlyRoot)
            {
                for (int i = 0; i < rootGos.Count; i++)
                {
                    yield return rootGos[i];
                    count++;
                    if (count > AsyncDelayCount)
                    {
                        await UniTask.Delay ((int)(AsyncDelayTime * 1000));
                        count = 0;
                    }
                }
                yield break;
            }
            else
            {
                List<IEnumerator<GameObject>> rootIters = rootGos.ConvertAll (go => AsyncCycleGoGetter (go, findPattern).GetEnumerator ());

                if (findPattern == FindPattern.DFS)
                {
                    for (int i = 0; i < rootIters.Count; i++)
                    {
                        var iter = rootIters[i];

                        while (iter.MoveNext ())
                        {
                            yield return iter.Current;

                            count++;

                            if (count > AsyncDelayCount)
                            {
                                await UniTask.Delay ((int)(AsyncDelayTime * 1000));
                                count = 0;
                            }
                        }
                    }
                }
                else if (findPattern == FindPattern.BFS)
                {
                    while (true)
                    {
                        var copyChildIters = rootIters.ToList ();

                        //廣度優先, 先不斷從外層跑
                        for (int i = 0; i < copyChildIters.Count; i++)
                        {
                            var copyIter = copyChildIters[i];

                            if (copyIter.MoveNext ())
                            {
                                yield return copyIter.Current;

                                count++;

                                if (count > AsyncDelayCount)
                                {
                                    await UniTask.Delay ((int)(AsyncDelayTime * 1000));
                                    count = 0;
                                }
                            }
                            else
                            {
                                rootIters.Remove (copyIter);
                            }
                        }

                        if (rootIters.Count == 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    LoggerRouter.Error ($"不支援的搜尋模式 {findPattern}");
                }
            }
        }

        /// <summary>
        /// 透過遞迴的方式異步取得所有子物件
        /// </summary>
        /// <param name="curGo"></param>
        /// <returns></returns>
        static IEnumerable<GameObject> AsyncCycleGoGetter (GameObject curGo, FindPattern findPattern)
        {
            yield return curGo;

            List<IEnumerator<GameObject>> childIters = new List<IEnumerator<GameObject>> ();

            for (int i = 0; i < curGo.transform.childCount; i++)
            {
                var child = curGo.transform.GetChild (i).gameObject;

                var iter = AsyncCycleGoGetter (child, findPattern).GetEnumerator ();

                childIters.Add (iter);
            }

            if (findPattern == FindPattern.DFS)
            {
                for (int i = 0; i < childIters.Count; i++)
                {
                    var iter = childIters[i];

                    while (iter.MoveNext ())
                    {
                        yield return iter.Current;
                    }
                }
            }
            else
            {
                while (true)
                {
                    var copyChildIters = childIters.ToList ();

                    //廣度優先, 先不斷從外層跑
                    for (int i = 0; i < copyChildIters.Count; i++)
                    {
                        var copyIter = copyChildIters[i];

                        if (copyIter.MoveNext ())
                        {
                            yield return copyIter.Current;
                        }
                        else
                        {
                            childIters.Remove (copyIter);
                        }
                    }

                    if (childIters.Count == 0)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 傳入Gameobject回傳Components
        /// </summary>
        /// <typeparam name="TCom"></typeparam>
        public class GetComponentTask<TCom> : BatchTask<GameObject>
        {
            /// <summary>
            /// unique代表是否找到第一個物件後就結束
            /// </summary>
            /// <param name="findUnique"></param>
            /// <param name="filter"></param>
            public void Setup (bool findUnique, Predicate<TCom> filter)
            {
                this.filter = filter;
                this.findUnique = findUnique;
            }

            public override void Invoke (GameObject scheduleObj, out bool isDone)
            {
                var coms = scheduleObj.GetComponents<TCom> ().ToList ();

                if (filter != null)
                {
                    coms.RemoveAll (com => filter.Invoke (com) == false);
                }

                if (findUnique)
                {
                    isDone = coms.Count > 0;
                }
                else
                {
                    isDone = false;
                }

                results.AddRange (coms);
            }

            Predicate<TCom> filter = null;

            bool findUnique = false;

            public List<TCom> results = new List<TCom> ();
        }

        /// <summary>
        /// 當經過這個數量
        /// </summary>
        const int AsyncDelayCount = 50;

        /// <summary>
        /// 就要等待這個時間
        /// </summary>
        const float AsyncDelayTime = 0.005f;

        public enum FindPattern
        {
            /// <summary>
            /// 深度優先
            /// </summary>
            DFS,
            /// <summary>
            /// 廣度優先
            /// </summary>
            BFS,
            /// <summary>
            /// 只搜索最外層
            /// </summary>
            OnlyRoot,
        }
    }
}