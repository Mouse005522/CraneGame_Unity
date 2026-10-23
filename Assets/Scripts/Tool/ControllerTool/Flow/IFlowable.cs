using Cysharp.Threading.Tasks;

namespace Kun.Tool
{
    public interface IFlowable
    {
        /// <summary>
        /// 兩階段更新的Setup先再來是Init
        /// </summary>
        UniTask OnSetup ();

        /// <summary>
        /// 兩階段更新的Setup先再來是Init
        /// </summary>
        UniTask OnInit ();

        void OnPreUpdate (float deltaTime, float unscaledDeltaTime);

        void OnUpdate (float deltaTime, float unscaledDeltaTime);
        void OnFixedUpdate (float fixedDeltatTime);
        void OnPhysicsRateUpdate (float physicsRate);

        void OnLateUpdate (float deltaTime);

        void OnBeforeRender ();

        void OnDrawGizmos ();

        void OnGameQuit ();
    }
}
