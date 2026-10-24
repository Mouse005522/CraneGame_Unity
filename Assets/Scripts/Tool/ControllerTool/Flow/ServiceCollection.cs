using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// 服務的集合群組 
    /// </summary>
    public class ServiceCollection : IServiceCollection, IServiceRoot
    {
        public ServiceCollection (float physicsRate = 0.1f, EasyInject defaultInject = null)
        {
            this.physicsRate = physicsRate;

            if (defaultInject != null)
            {
                ServiceInject = defaultInject.CreateClone ();
                ServiceInject.RemoveService<IServiceCollection> ();
                ServiceInject.RemoveService<IServiceRoot> ();
                ServiceInject.RemoveService<ServiceCollection> ();
            }
            else
            {
                ServiceInject = new EasyInject ();
            }

            ServiceInject.AddService (this)
                .With<IServiceCollection> ()
                .With<IServiceRoot> ();
        }

        /// <summary>
        /// 繼承目前的服務註冊表
        /// 並產生新的 ServiceCollection 副本
        /// </summary>
        public ServiceCollection CreateClone (float physicsRate = 0.1f)
        {
            return new ServiceCollection (physicsRate, this.ServiceInject);
        }

        /// <summary>
        /// 設定根節點 Transform
        /// </summary>
        public void SetRoot (Transform root)
        {
            Root = root;
        }

        public Transform Root { get; private set; }

        /// <summary>
        /// 管理器是否準備完成
        /// </summary>
        bool prepareFinish = false;

        public void BindingService ()
        {
            foreach (var mgr in CacheServices)
            {
                try
                {
                    BindingService (mgr);
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }
        }

        public async UniTask SetupServices ()
        {
            foreach (var mgr in flowables)
            {
                try
                {
                    await mgr.OnSetup ();
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }

            prepareFinish = true;
        }

        public async UniTask InitServices ()
        {
            foreach (var mgr in flowables)
            {
                try
                {
                    await mgr.OnInit ();
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }
        }

        void IServiceCollection.BindingService (object obj)
        {
            BindingService (obj);
        }

        /// <summary>
        /// 檢查是否需要透過attribute綁訂DI
        /// </summary>
        /// <param name="obj"></param>
        public void BindingService (object obj)
        {
            ServiceInject.BindingService<EasyInjectBindingAttribute> (obj);
        }

        /// <summary>
        /// 透過DI產生物件
        /// 自動觸發無參數建構式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateRequest<T> () where T : class, new()
        {
            T req = new T ();
            BindingService (req);
            return req;
        }

        T IServiceCollection.CreateRequest<T> ()
        {
            return this.CreateRequest<T> ();
        }

        void IServiceCollection.RemoveService<T> ()
        {
            RemoveService<T> ();
        }

        public EasyInject ServiceInject { get; private set; } = new EasyInject ();

        public List<object> CacheServices { get; private set; } = new List<object> ();

        /// <summary>
        /// 加入需要更新的物件
        /// </summary>
        /// <param name="flowable"></param>
        void FetchInterface (object obj)
        {
            if (obj is IFlowable flowable)
            {
                if (flowables.Contains (flowable) == false)
                {
                    flowables.Add (flowable);
                }
            }
        }

        List<IFlowable> flowables = new List<IFlowable> ();

        List<Action<float>> updateHooks = new List<Action<float>> ();

        List<Action> disposeHooks = new List<Action> ();

        /// <summary>
        /// 讓套件註冊更新回呼，保留解耦又能被 ServiceCollection 統一更新
        /// </summary>
        public void AddUpdateHook (Action<float> hook)
        {
            updateHooks.Add (hook);
        }

        /// <summary>
        /// 讓套件註冊釋放回呼，在 OnGameQuit 時統一執行
        /// </summary>
        public void AddDisposeHook (Action hook)
        {
            disposeHooks.Add (hook);
        }

        #region UnityLifeFlow

        const float PhysicsRate = 0.1f;

        public void OnUpdate (float deltaTime, float unscaledDeltaTime)
        {
            if (prepareFinish == false)
            {
                return;
            }

            foreach (var flowable in flowables)
            {
                try
                {
                    flowable.OnPreUpdate (deltaTime, unscaledDeltaTime);
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }

            foreach (var flowable in flowables)
            {
                try
                {
                    flowable.OnUpdate (deltaTime, unscaledDeltaTime);
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }

            UpdatePhysics (deltaTime);

            foreach (var hook in updateHooks)
            {
                try
                {
                    hook.Invoke (deltaTime);
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }
        }

        public void OnLateUpdate (float deltaTime)
        {
            if (prepareFinish == false)
            {
                return;
            }

            foreach (var flowable in flowables)
            {
                try
                {
                    flowable.OnLateUpdate (deltaTime);
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }
        }

        float physicsRate = 0.1f;
        float physicsTimer = 0f;

        void UpdatePhysics (float deltaTime) 
        {
            physicsTimer += deltaTime;
            if (physicsTimer >= physicsRate)
            {
                physicsTimer = 0;

                foreach (IFlowable flowable in flowables)
                {
                    try
                    {
                        flowable.OnPhysicsRateUpdate (physicsRate);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException (ex);
                    }
                }
            }
        }


        public void OnDrawGizmos ()
        {
            foreach (var flowable in flowables)
            {
                try
                {
                    flowable.OnDrawGizmos ();
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }
        }

        public void OnFixedUpdate (float deltaTime)
        {
            if (prepareFinish == false)
            {
                return;
            }

            foreach (var flowable in flowables)
            {
                try
                {
                    flowable.OnFixedUpdate (deltaTime);
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }
        }

        public void OnBeforeRender ()
        {
            if (prepareFinish == false)
            {
                return;
            }

            foreach (var flowable in flowables)
            {
                try
                {
                    flowable.OnBeforeRender ();
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }
        }

        /// <summary>
        /// 遊戲結束時或者進到新遊戲卸載舊遊戲時
        /// </summary>
        public void OnGameQuit ()
        {
            prepareFinish = false;
            foreach (var flowable in flowables)
            {
                try
                {
                    flowable.OnGameQuit ();
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }

            flowables.Clear ();
            updateHooks.Clear ();

            foreach (var hook in disposeHooks)
            {
                try
                {
                    hook.Invoke ();
                }
                catch (Exception ex)
                {
                    Debug.LogException (ex);
                }
            }

            disposeHooks.Clear ();
        }

        #endregion

        #region service

        /// <summary>
        /// 如果希望被以基底型別譬如interface之類的
        /// T要傳入想被記住的Type
        /// 透過無參數建構式觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<ServiceType> AddService<RequestT, ServiceType> (RequestT requestT) where RequestT : ServiceType
        {
            var token = ServiceInject.AddService<ServiceType> (requestT);

            FetchInterface (requestT);

            if (CacheServices.Contains (requestT) == false)
            {
                CacheServices.Add (requestT);
            }

            return token;
        }

        /// <summary>
        /// 如果希望被以基底型別譬如interface之類的
        /// T要傳入想被記住的Type
        /// 透過無參數建構式觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<RequestT> AddService<RequestT, ServiceType> () where RequestT : ServiceType, new()
        {
            var token = ServiceInject.AddService<RequestT, ServiceType> (out RequestT service);

            FetchInterface (service);

            if (CacheServices.Contains (service) == false)
            {
                CacheServices.Add (service);
            }

            return token;
        }

        /// <summary>
        /// 如果希望被以基底型別譬如interface之類的
        /// T要傳入想被記住的Type
        /// 透過無參數建構式觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<RequestT> AddService<RequestT> () where RequestT : new()
        {
            var token = ServiceInject.AddService (out RequestT service);

            FetchInterface (service);

            if (CacheServices.Contains (service) == false)
            {
                CacheServices.Add (service);
            }

            return token;
        }

        /// <summary>
        /// 如果希望被以基底型別譬如interface之類的
        /// T要傳入想被記住的Type
        /// 某些Service在多個地方被註冊needCache傳false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public EasyInjectAddToken<T> AddService<T> (T service, bool needCache = true)
        {
            var token = ServiceInject.AddService (service);

            if (needCache)
            {
                FetchInterface (service);

                if (CacheServices.Contains (service) == false)
                {
                    CacheServices.Add (service);
                }
            }

            return token;
        }

        public void RemoveService<T> ()
        {
            if (ServiceInject.TryGetService<T> (out T service))
            {
                CacheServices.Remove (service);

                if (service is IFlowable flowable)
                {
                    flowables.Remove (flowable);
                }
            }

            ServiceInject.RemoveService<T> ();
        }

        /// <summary>
        /// 會以實體型別被記錄
        /// 用以讓底層替衍生者進行注入的場合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public void AddServiceWithRealType (object service)
        {
            ServiceInject.AddServiceWithRealType (service);
        }

        #endregion

        /// <summary>
        /// 加入服務的外部套件對於Service工廠的處理
        /// </summary>
        /// <param name="serviceHandle"></param>
        public void AddServiceHandle (InjectServiceHandle serviceHandle)
        {
            ServiceInject.AddInjectServiceHandle (serviceHandle);
        }
    }

    /// <summary>
    /// ServiceCollection的根節點Transform
    /// </summary>
    public interface IServiceRoot
    {
        Transform Root { get; }
    }

    public interface IServiceCollection
    {
        /// <summary>
        /// 透過依賴注入產生
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateRequest<T> () where T : class, new();

        /// <summary>
        /// 檢查是否需要透過attribute綁訂DI
        /// </summary>
        /// <param name="obj"></param>
        void BindingService (object obj);

        /// <summary>
        /// 移除服務
        /// </summary>
        void RemoveService<T> ();
    }
}
