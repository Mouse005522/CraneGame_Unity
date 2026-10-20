using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public abstract class StateRepo<T> where T : FlowState<T> 
	{
		public virtual void Setup (FlowStateController<T> controller)
		{
			this.Controller = controller;

			StateTable = new Dictionary<Type, T> ();
		}

		public FlowStateController<T> Controller { get; private set; }


		public void AddState<State> () where State:T, new ()
		{
			State state = new State ();

			AddState (state);
        }

		public void AddState (T state) 
		{
            state.Setup (Controller);

            StateTable.Add (state.GetType (), state);
            States.Add (state);
        }

		protected List<T> States { get; private set; } = new List<T> ();

		protected Dictionary<Type, T> StateTable { get; private set; } = new Dictionary<Type, T> ();

		public TState GetState<TState> () where TState : FlowState
		{
			return GetState (typeof (TState)) as TState;
		}

		public FlowState GetState (Type type)
		{
			try
			{
				return StateTable[type];
			}
			catch
			{
				LoggerRouter.Error ($"找不到狀態, 請確認是否有初始化到這個狀態 -> {type.Name}");
				return null;
			}
		}
	}
}