using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kun.Tool
{
    public static class EasyMediat
    {
        static EasyMediat ()
        {
            PrepareTypes ();

            PrepareTypeTable ();
        }

        static List<Type> handlerTypes = null;

        static List<Type> reqTypes = null;

        static void PrepareTypes ()
        {

            //考量到這個套件大多在Editor中被呼叫
            //不過也會有Runtime的情形所以並行
#if UNITY_EDITOR
            handlerTypes = TypeCache.GetTypesDerivedFrom (typeof (EasyMediatNotificationHandler<>)).ToList ();
            handlerTypes.RemoveAll (t => t.IsAbstract);

            reqTypes = TypeCache.GetTypesDerivedFrom (typeof (EasyMediatNotificationHandler<,>)).ToList ();
            reqTypes.RemoveAll (t => t.IsAbstract);
            return;
#endif
            handlerTypes = AssemblyUtility.GetSubclasses (typeof (EasyMediatNotificationHandler<>)).ToList ();
            reqTypes = AssemblyUtility.GetSubclasses (typeof (EasyMediatNotificationHandler<,>)).ToList ();
        }

        static void PrepareTypeTable ()
        {
            handlerTable = new HandlerCacher<Type> ();
            handlerTable.CreateTable(handlerTypes, typeof (EasyMediatNotificationHandler<>), 1, (argTypes) => argTypes[0]);

            reqHandlerTable = new HandlerCacher<(Type argType, Type resType)> ();

            reqHandlerTable.CreateTable (reqTypes, typeof (EasyMediatNotificationHandler<,>), 2, (argTypes) => (argTypes[0], argTypes[1]));
        }

        /// <summary>
        /// 只收不回傳
        /// </summary>
        static HandlerCacher<Type> handlerTable = new HandlerCacher<Type> ();

        /// <summary>
        /// 收且回傳
        /// </summary>
        static HandlerCacher<(Type argType, Type resType)> reqHandlerTable = new HandlerCacher<(Type argType, Type resType)> ();

        static object[] tempPar = new object [0];

        /// <summary>
        /// 配合unity domain
        /// editor reload不明確的問題
        /// 給予主動清空的手段
        /// </summary>
        public static void ClearDynamics () 
        {
            dynamicHandlerTable.Clear ();
            dynamicReqHandlerTable.Clear ();
        }

        /// <summary>
        /// 也可以透過動態設定靜態方法的方式綁定
        /// </summary>
        public static void AddDynamicHandler<T> (Action<T> handler) 
        {
            var type = typeof (T);

            if (dynamicHandlerTable.TryGetValue (type, out List<Delegate> callbacks) == false)
            {
                callbacks = new List<Delegate> ();
                dynamicHandlerTable.Add (type, callbacks);
            }

            callbacks.Add (handler);
        }

        public static void RemoveDynamicHandler<T> (Action<T> handler)
        {
            var type = typeof (T);

            if (dynamicHandlerTable.TryGetValue (type, out List<Delegate> callbacks) == false)
            {
                callbacks = new List<Delegate> ();
                dynamicHandlerTable.Add (type, callbacks);
            }

            callbacks.Remove (handler);
        }

        static Dictionary<Type, List<Delegate>> dynamicHandlerTable = new Dictionary<Type, List<Delegate>> ();

        /// <summary>
        /// 也可以透過動態設定靜態方法的方式綁定
        /// </summary>
        public static void AddDynamicReqHandler<T,TRes> (Func<T,TRes> handler)
        {
            (Type argType, Type resType) pairKey = (typeof (T), typeof(TRes));

            if (dynamicReqHandlerTable.TryGetValue (pairKey, out List<Delegate> callbacks) == false)
            {
                callbacks = new List<Delegate> ();
                dynamicReqHandlerTable.Add (pairKey, callbacks);
            }

            callbacks.Add (handler);
        }

        /// <summary>
        /// 也可以透過動態設定靜態方法的方式綁定
        /// </summary>
        public static void RemoveDynamicReqHandler<T, TRes> (Func<T, TRes> handler)
        {
            (Type argType, Type resType) pairKey = (typeof (T), typeof (TRes));

            if (dynamicReqHandlerTable.TryGetValue (pairKey, out List<Delegate> callbacks) == false)
            {
                callbacks = new List<Delegate> ();
                dynamicReqHandlerTable.Add (pairKey, callbacks);
            }

            callbacks.Remove (handler);
        }

        static Dictionary<(Type argType, Type resType), List<Delegate>> dynamicReqHandlerTable = new Dictionary<(Type argType, Type resType), List<Delegate>> ();

        /// <summary>
        /// 推送資料並收集回傳值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TRes"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<TRes> Push<T, TRes> (T data)
        {
            var pairKey = (typeof (T), typeof (TRes));

            List<TRes> resCollection = new List<TRes> ();

            if (reqHandlerTable.TryGetValue (pairKey, out List<ConstructorInfo> ctors))
            {
                foreach (var ctor in ctors) 
                {
                    try
                    {
                        var obj = ctor.Invoke (tempPar);

                        var handler = (obj as EasyMediatNotificationHandler<T, TRes>);
                        var res = handler.OnReceive (data);
                        resCollection.Add (res);
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Error (e);
                    }
                }
            }

            if (dynamicReqHandlerTable.TryGetValue (pairKey, out List<Delegate> funcs)) 
            {
                foreach (var func in funcs) 
                {
                    try
                    {
                        var obj = func.DynamicInvoke (data);
                        resCollection.Add ((TRes)obj);
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Error (e);
                    }
                }
            }

            return resCollection;
        }

        /// <summary>
        /// 推送資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public static void Push<T> (T data)
        {
            var key = typeof (T);
            if (handlerTable.TryGetValue (key, out List<ConstructorInfo> ctors))
            {
                foreach (var ctor in ctors) 
                {
                    try
                    {
                        var obj = ctor.Invoke (tempPar);

                        var handler = (obj as EasyMediatNotificationHandler<T>);
                        handler.OnReceive (data);
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Error (e);
                    }
                }
            }

            if (dynamicHandlerTable.TryGetValue (key, out List<Delegate> callbacks))
            {
                foreach (var callback in callbacks) 
                {
                    try
                    {
                        callback.DynamicInvoke (data);
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.Error (e);
                    }
                }
            }
        }

        class HandlerCacher<TKey> 
        {
            Dictionary<TKey, List<ConstructorInfo>> table = new Dictionary<TKey, List<ConstructorInfo>> ();

            public bool TryGetValue (TKey key, out List<ConstructorInfo> ctors) 
            {
                return table.TryGetValue (key, out ctors);
            }

            public void CreateTable (List<Type> srcTypes, Type genType, int argCount, Func<List<Type>, TKey> converter)
            {
                table = new Dictionary<TKey, List<ConstructorInfo>> ();

                var typeGroups = srcTypes.GroupBy (type =>
                {
                    List< Type> argTypes = null;

                    while (type != null && type != typeof (object))
                    {
                        if (type.IsGenericType && type.GetGenericTypeDefinition () == genType)
                        {
                            var args = type.GetGenericArguments ();

                            //長度1是只收不回傳
                            if (args.Length == argCount)
                            {
                                argTypes = type.GetGenericArguments ().ToList ();
                            }
                            break;
                        }
                        type = type.BaseType;
                    }

                    return converter.Invoke (argTypes);

                }).ToList ().ConvertAll (g =>
                {
                    TKey key = g.Key;
                    List<Type> handlerTypes = g.ToList ();

                    return (key, handlerTypes);
                });

                typeGroups.ForEach (g =>
                {
                    List<ConstructorInfo> ctors = new List<ConstructorInfo> ();

                    g.handlerTypes.ForEach (t =>
                    {
                        var typeCtors = t.GetConstructors ();
                        var findNoPar = typeCtors.FirstOrDefault (c => c.GetParameters ().Length == 0);

                        if (findNoPar == null)
                        {
                            LoggerRouter.Error ($"{t.Name}為handler但是不具有無參數建構式");
                        }
                        else
                        {
                            ctors.Add (findNoPar);
                        }
                    });

                    if (ctors.Count > 0)
                    {
                        table.Add (g.key, ctors);
                    }
                });
            }
        }
    }
}
