using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class FloatLerper : Lerper<float>
    {
        protected override float GetLerpValue (float from, float to, float progress)
        {
            return Mathf.Lerp (from, to, progress);
        }
    }

    public abstract class Lerper<ValueT>
    {
        public void BindOnModify (Action<ValueT> callback)
        {
            valueChangeRouter.Bind (callback);
        }

        public void UnBindOnModify (Action<ValueT> callback)
        {
            valueChangeRouter.UnBind (callback);
        }

        public void SetInterpolationDuration (float interpolationDuration)
        {
            this.interpolationDuration = interpolationDuration;
        }

        float interpolationDuration = DefaultInterpolationDuration;

        const float DefaultInterpolationDuration = 0.1f;

        ActionRouter<ValueT> valueChangeRouter = new ActionRouter<ValueT> ();

        protected abstract ValueT GetLerpValue (ValueT from, ValueT to, float progress);

        ValueT oldValue;
        ValueT curValue;
        ValueT newValue;

        float timer;

        public bool HasValue { get; private set; } = false;

        /// <summary>
        /// 數值更新後強制觸發
        /// </summary>
        /// <param name="forceValue"></param>
        public void ForceSetValue (ValueT forceValue)
        {
            HasValue = true;

            this.curValue = this.oldValue = this.newValue = forceValue;
            timer = interpolationDuration;
            Flush ();
        }

        /// <summary>
        /// 數值更新後透過lerp觸發
        /// </summary>
        /// <param name="newValue"></param>
        public void InputNewValue (ValueT newValue)
        {
            HasValue = true;

            this.newValue = newValue;
            this.timer = 0;
            this.oldValue = this.curValue;
        }

        public void Update (float deltaTime)
        {
            if (HasValue == false)
            {
                return;
            }

            if (timer < interpolationDuration)
            {
                float f = timer / interpolationDuration;

                curValue = GetLerpValue (this.oldValue, this.newValue, f);

                timer += deltaTime;

                Flush ();
            }
        }

        void Flush ()
        {
            valueChangeRouter.Invoke (curValue);
        }

        public void Init ()
        {
            oldValue = GetInitValue ();
            curValue = GetInitValue ();
            newValue = GetInitValue ();
        }

        /// <summary>
        /// Quaternion這種default是0000可是其實應該0001的才需要特別處理
        /// 或者是參考型別也要處理
        /// </summary>
        /// <returns></returns>
        protected virtual ValueT GetInitValue ()
        {
            return default (ValueT);
        }
    }
}
