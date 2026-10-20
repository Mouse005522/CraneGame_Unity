using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class FloatPredicter : Predicter<float>
    {
        protected override float Addition (float origin, float target)
        {
            return origin + target;
        }
        protected override float Multiplication (float origin, float value)
        {
            return origin * value;
        }
    }

    /// <summary>
    /// 推算底層
    /// </summary>
    /// <typeparam name="ValueT"></typeparam>
    public abstract class Predicter<ValueT>
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


        ValueT lastInput;
        ValueT inputValue;
        ValueT estimatedVelocity;

        float lastInputTimer;
        float timer; // 新增的 timer 變數，使用 deltaTime 累加

        public ValueT CurValue { get; private set; }

        public bool HasValue { get; private set; } = false;

        /// <summary>
        /// 數值更新後強制觸發
        /// </summary>
        /// <param name="forceValue"></param>
        public void ForceSetValue (ValueT forceValue)
        {
            HasValue = true;

            this.CurValue = this.lastInput = this.inputValue = forceValue;
            this.estimatedVelocity = GetInitValue ();
            timer = interpolationDuration;
            Flush ();
        }

        /// <summary>
        /// 數值更新後透過lerp觸發
        /// </summary>
        /// <param name="newValue"></param>
        public void InputNewValue (ValueT newValue)
        {
            if (HasValue == false)
            {
                lastInput = newValue;
                inputValue = newValue;
                CurValue = newValue;
                timer = 0f;
                lastInputTimer = 0f;
            }
            else
            {
                ValueT deltaValue = Subtraction (newValue, lastInput);
                float deltaTime = timer - lastInputTimer;

                if (deltaTime > 0)
                {
                    estimatedVelocity = Division (deltaValue, deltaTime);
                }

                lastInput = inputValue;
                inputValue = newValue;
                lastInputTimer = timer;
            }


            HasValue = true;
        }

        public void Update (float deltaTime)
        {
            if (HasValue == false)
            {
                return;
            }

            // 累加 timer
            timer += deltaTime;
            ValueT plus = Multiplication (estimatedVelocity, deltaTime);

            // 預測：在封包之間，用估算速度往前推
            CurValue = Addition (CurValue, plus);

            // 當伺服器新位置來時再修正（誤差回補）
            ValueT error = Subtraction (inputValue, CurValue);

            //平滑修正 - 使用正確的插值係數
            float lerpFactor = Mathf.Clamp01(deltaTime / interpolationDuration);
            ValueT fix = Multiplication (error, lerpFactor);
            CurValue = Addition (CurValue, fix);

            Flush ();
        }

        void Flush ()
        {
            valueChangeRouter.Invoke (CurValue);
        }

        public void Init ()
        {
            lastInput = GetInitValue ();
            CurValue = GetInitValue ();
            estimatedVelocity = GetInitValue ();
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

        /// <summary>
        /// 減法
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected ValueT Subtraction (ValueT origin, ValueT target) 
        {
            return Addition (origin, Multiplication (target, -1f));
        }

        /// <summary>
        /// 實作加法
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected abstract ValueT Addition (ValueT origin, ValueT target);

        /// <summary>
        /// 除法
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected ValueT Division (ValueT origin, float value) 
        {
            return Multiplication (origin, 1f / value);
        }

        /// <summary>
        /// 實作乘法
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract ValueT Multiplication (ValueT origin, float value);
    }
}