using System;

namespace Kun.Tool
{
    public abstract class FlowState<T> : FlowState where T:FlowState<T>
    {
        public virtual void Setup (FlowStateController<T> controller)
        {
            this.Controller = controller;
        }

        protected FlowStateController<T> Controller { get; private set; }

        protected StateRepo<T> Repo => Controller.Repo;

        public virtual void Enter ()
        {

        }

        public virtual FlowState<T> Stay (float deltaTime)
        {
            return null;
        }

        public virtual void Exit ()
        {

        }

        protected Action CreateTransferEvent<State> () where State : T
        {
            return () =>
            {
                Controller.ForceToChangeState<State> ();
            };
        }

        public virtual void DrawEditor ()
        {

        }
    }

    public abstract class FlowState
    {
        public abstract string StateMsg { get; }
    }


}