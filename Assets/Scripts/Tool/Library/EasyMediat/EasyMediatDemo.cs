
#if UNITY_EDITOR
using UnityEngine;

namespace Kun.Tool
{
    public class EasyMediatDemo : MonoBehaviour
    {
        [ContextMenu ("DoTest")]
        void DoTest ()
        {
            EasyMediat.ClearDynamics ();

            EasyMediatDemoData pushData = new EasyMediatDemoData () { msg = "testPush" };

            EasyMediat.AddDynamicHandler ((EasyMediatDemoData data) => 
            {
                Debug.LogError ($"d -> {data.msg}");
            });

            EasyMediat.Push (pushData);

            EasyMediatDemoData reqData = new EasyMediatDemoData () { msg = "testReq" };

            EasyMediat.AddDynamicReqHandler ((EasyMediatDemoData data) => 
            {
                return 87f;
            });

            var results = EasyMediat.Push<EasyMediatDemoData, float> (reqData);

            Debug.Log ($"results -> {string.Join (", ", results)}");
        }
    }

    class EasyMediatDemoData 
    {
        public string msg;
    }

    class PushTest : EasyMediatNotificationHandler<EasyMediatDemoData>
    {
        public override void OnReceive (EasyMediatDemoData data)
        {
            Debug.Log ($"1 -> {data.msg}");
        }
    }

    class PushTest2 : EasyMediatNotificationHandler<EasyMediatDemoData>
    {
        public override void OnReceive (EasyMediatDemoData data)
        {
            Debug.Log ($"2 -> {data.msg}");
        }
    }

    class ReqTest : EasyMediatNotificationHandler<EasyMediatDemoData, float>
    {
        public override float OnReceive (EasyMediatDemoData data)
        {
            Debug.Log ($"1 -> {data.msg}");

            return 1;
        }
    }

    class ReqTest2 : EasyMediatNotificationHandler<EasyMediatDemoData, float>
    {
        public override float OnReceive (EasyMediatDemoData data)
        {
            Debug.Log ($"2 -> {data.msg}");

            return 2;
        }
    }
}

#endif
