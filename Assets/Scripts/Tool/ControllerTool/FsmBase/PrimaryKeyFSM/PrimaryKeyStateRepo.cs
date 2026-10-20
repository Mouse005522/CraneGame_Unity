using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class PrimaryKeyStateRepo<StateT,PKT> : StateRepo<StateT> where StateT : FlowState<StateT>, Primaryable<PKT> where PKT:struct
    {
        public bool TryGetState (PKT pk, out StateT state)
        {
            if (States.TryFind (s => s.PrimaryKey.Equals (pk), out state))
            {
                return true;
            }
            else
            {
                LoggerRouter.Error ($"找不到對應的state -> {pk}");
                return false;
            }
        }
    }
}