using System;

namespace Kun.Tool
{
    public abstract class FlowStateController<T> where T : FlowState<T>
    {
        protected virtual void Setup (StateRepo<T> repo)
        {
            Repo = repo;
            repo.Setup (this);
        }

        public void BindOnChange (Action<T> onNewState = null)
        {
            if (onNewState != null) 
            {
                newStateRouter.Bind (onNewState);
            }
        }

        ActionRouter<T> newStateRouter = new ActionRouter<T> ();

        public void OnUpdate (float deltaTime)
        {
            hasForceChange = false;

            if (CurState != null)
            {
                var nextState = CurState.Stay (deltaTime);

                //如果已經觸發轉換了, 那就不要管stay的結果
                if (nextState != null && hasForceChange == false)
                {
                    CurState.Exit ();

                    EnterNewState (nextState as T);
                }
            }
        }

        bool hasForceChange = false;

        public void ForceToChangeState<State> () where State : T
        {
            var nexState = Repo.GetState<State> ();

            ForceToChangeState (nexState);
        }

        public void ForceToChangeState (T newState)
        {
            hasForceChange = true;

            FlowState oldState = null;

            if (CurState != null)
            {
                oldState = CurState;
                CurState.Exit ();
            }

            EnterNewState (newState);
        }

        protected virtual void EnterNewState (T newState) 
        {
            CurState = newState;

            CurState.Enter ();

            newStateRouter.Invoke (newState);
        }

        protected virtual void OnForceExit () 
        {

        }

        public T CurState { get; private set; }

        public void ForceExitCurState () 
        {
            if (CurState != null) 
            {
                CurState.Exit ();
                CurState = null;

                OnForceExit ();
            }
        }

        public StateRepo<T> Repo { get; private set; }
    }
    
}