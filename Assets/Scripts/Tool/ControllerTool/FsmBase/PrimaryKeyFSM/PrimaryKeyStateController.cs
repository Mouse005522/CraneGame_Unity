using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class PrimaryKeyStateFlowStateController<StateT,PKT> : FlowStateController<StateT> where StateT : PrimaryableState<StateT,PKT>, Primaryable<PKT> where PKT:struct
    {
        protected override void Setup (StateRepo<StateT> repo)
        {
            base.Setup (repo);

            pkRepo = (PrimaryKeyStateRepo<StateT, PKT>)repo;
        }

        public void ForceTransferByPK (PKT pk) 
        {
            if (pkRepo.TryGetState (pk, out StateT state)) 
            {
                ForceToChangeState (state);
            }
        }

        PrimaryKeyStateRepo<StateT, PKT> pkRepo;

        public bool TryGetCurState (out StateT state)
        {
            if (CurState != null)
            {
                state = CurState;
                return true;
            }
            else
            {
                state = default;
                return false;
            }
        }

        public bool TryGetCurStatus (out PKT status)
        {
            if (CurState != null)
            {
                status = CurState.Status;
                return true;
            }
            else
            {
                status = default;
                return false;
            }
        }
    }
}