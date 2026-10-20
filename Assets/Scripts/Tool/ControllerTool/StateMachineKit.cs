using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    [Serializable]
    public class StateMachineKit<T> where T : struct, Enum
    {
        public StateMachineKit () 
        {
            onEnter = (state) => { };
            onExit = (state) => { };
            onBeforeEnter = (state) => { };
        }

        private Dictionary<T, StateData> stateTable = new Dictionary<T, StateData> (EqualityComparer<T>.Default);

        [ReadOnly]
        [SerializeField]
        private T curState;

        public T CurrentState => curState;

        StateData curStateData = null;

        public void RegisterState (T state, Action onEnter = null, Func<float, T?> onStay = null, Action onExit = null)
        {
            if (stateTable.ContainsKey (state) == false)
            {
                StateData stateData = new StateData ();

                stateData.OnEnter = onEnter != null ? onEnter : () => { };
                stateData.OnStay = onStay != null ? onStay : (float deltaTime) => null;
                stateData.OnExit = onExit != null ? onExit : () => { };

                stateTable[state] = stateData;
            }
        }

        public void Update (float deltaTime)
        {
            if (curStateData != null)
            {
                T? nextState = curStateData.OnStay.Invoke (deltaTime);
                //EqualityComparer處理value type可以避免裝箱
                if (nextState.HasValue && EqualityComparer<T>.Default.Equals (nextState.Value, curState) == false)
                {
                    Transfer (nextState.Value);
                }
            }
        }

        public void Transfer (T targetState)
        {
            T? prevState = default;

            if (curStateData != null)
            {
                curStateData.OnExit.Invoke ();
                onExit.Invoke (curState);
                prevState = curState;
            }
            else 
            {
                prevState = null;
            }

            curStateData = null;

            curState = targetState;

            if (stateTable.ContainsKey (targetState) == false)
            {
                LoggerRouter.Error ($"State {targetState} is not registered.");
                return;
            }

            var pair = (prevState, targetState);

            onBeforeEnter.Invoke (pair);

            curStateData = stateTable[targetState];
            curStateData.OnEnter.Invoke ();
            onEnter.Invoke (pair);
        }

        /// <summary>
        /// 全域的狀態變更事件綁定
        /// </summary>
        /// <param name="onEnter"></param>
        public void BindEnterEvent (Action<(T? prev, T cur)> onEnter)
        {
            this.onEnter += onEnter;
        }

        /// <summary>
        /// 全域的狀態變更事件綁定
        /// </summary>
        /// <param name="onEnter"></param>
        public void BindEnterEvent (Action<T> onEnter)
        {
            this.onEnter += ((T? prev, T cur) pair) => onEnter.Invoke (pair.cur);
        }

        Action<(T? prev, T cur)> onEnter;

        /// <summary>
        /// 全域的狀態變更事件綁定
        /// </summary>
        /// <param name="onEnter"></param>
        public void BindBeforeEnterEvent (Action<T> onEnter)
        {
            this.onBeforeEnter += ((T? prev, T cur) pair) => onEnter.Invoke (pair.cur);
        }

        /// <summary>
        /// 全域的狀態變更事件綁定
        /// </summary>
        /// <param name="onEnter"></param>
        public void BindBeforeEnterEvent (Action<(T? prev, T cur)> onEnter)
        {
            this.onBeforeEnter += onEnter;
        }

        Action<(T? prev, T cur)> onBeforeEnter;

        /// <summary>
        /// 全域的狀態變更事件綁定
        /// </summary>
        /// <param name="onExit"></param>
        public void BindEnterExit (Action<T> onExit)
        {
            this.onExit += onExit;
        }

        Action<T> onExit;

        private class StateData
        {
            public Action OnEnter;
            public Func<float, T?> OnStay;
            public Action OnExit;
        }
    }
}
