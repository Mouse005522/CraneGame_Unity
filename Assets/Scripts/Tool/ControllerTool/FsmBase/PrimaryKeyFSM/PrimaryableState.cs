using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// 透過回傳PK讓底層可以不用特別回傳泛型提高維護性
    /// </summary>
    /// <typeparam name="StateT"></typeparam>
    /// <typeparam name="PKT"></typeparam>
    public abstract class PrimaryableState<StateT, PKT> : FlowState<StateT>, Primaryable<PKT> where StateT : FlowState<StateT>, Primaryable<PKT> where PKT : struct
    {
        public abstract PKT Status { get; }

        PKT Primaryable<PKT>.PrimaryKey => Status;

        public override string StateMsg => ((Primaryable<PKT>)this).PrimaryKey.ToString ();

        public override void Setup (FlowStateController<StateT> controller)
        {
            base.Setup (controller);

            pkRepo = (PrimaryKeyStateRepo<StateT, PKT>)controller.Repo;
        }

        PrimaryKeyStateRepo<StateT, PKT> pkRepo;

        public sealed override FlowState<StateT> Stay (float deltaTime)
        {
            base.Stay (deltaTime);

            var result = OnStay (deltaTime);

            if (result != null)
            {
                if (pkRepo.TryGetState (result.Value, out StateT state))
                {
                    return state;
                }
            }

            return null;
        }

        protected virtual PKT? OnStay (float deltaTime)
        {
            return null;
        }
    }
}
