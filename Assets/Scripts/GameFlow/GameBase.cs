using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Kun.Tool;

namespace Kun.CraneGame
{
    public abstract class GameBase : MonoBehaviour
    {
        public ServiceCollection Services { get; private set; }

        [EasyInjectBinding]
        public SettingParser SettingParser { get; private set; }

        bool initialized;
        bool closed;

        protected abstract string SettingScopeName { get; }

        /// <summary>
        /// 建立端點專屬 scope，再統一執行註冊、注入與兩階段初始化。
        /// </summary>
        public async UniTask Setup (ServiceCollection rootServices)
        {
            if (closed || Services != null)
            {
                throw new InvalidOperationException ("端點已初始化或已關閉。");
            }

            if (rootServices == null)
            {
                throw new ArgumentNullException (nameof (rootServices));
            }

            if (!rootServices.ServiceInject.TryGetService<SettingParser> (out var rootParser))
            {
                throw new InvalidOperationException ("Root scope 未註冊 SettingParser。");
            }

            var parser = rootParser.CreateChild (SettingScopeName);
            Services = rootServices.CreateClone ();
            Services.SetRoot (transform);
            Services.RemoveService<SettingParser> ();
            Services.AddService (parser);

            try
            {
                Services.BindingService (this);
                await PrepareServices ();

                Services.BindingService ();
                await Services.SetupServices ();
                await Services.InitServices ();
                initialized = true;
            }
            catch
            {
                Close ();
                throw;
            }
        }

        /// <summary>
        /// 各端在此建立與註冊服務；注入與生命週期由基底統一處理。
        /// </summary>
        protected abstract UniTask PrepareServices ();
        protected virtual void OnEnable ()
        {
            if (!closed)
            {
                Application.onBeforeRender += BeforeRender;
            }
        }

        protected virtual void OnDisable ()
        {
            Application.onBeforeRender -= BeforeRender;
        }

        protected virtual void Update ()
        {
            if (initialized)
                Services.OnUpdate (Time.deltaTime, Time.unscaledDeltaTime);
        }

        protected virtual void FixedUpdate ()
        {
            if (initialized)
                Services.OnFixedUpdate (Time.fixedDeltaTime);
        }

        protected virtual void LateUpdate ()
        {
            if (initialized)
                Services.OnLateUpdate (Time.deltaTime);
        }

        void BeforeRender ()
        {
            if (initialized)
                Services.OnBeforeRender ();
        }

        protected virtual void OnDrawGizmos ()
        {
            if (initialized)
                Services.OnDrawGizmos ();
        }

        public void Close ()
        {
            if (closed)
                return;

            closed = true;
            initialized = false;
            Application.onBeforeRender -= BeforeRender;

            try
            {
                OnClose ();
            }
            finally
            {
                Services?.OnGameQuit ();
            }
        }

        protected virtual void OnClose ()
        {
        }

        protected virtual void OnApplicationQuit ()
        {
            Close ();
        }

        protected virtual void OnDestroy ()
        {
            Close ();
        }
    }
}
