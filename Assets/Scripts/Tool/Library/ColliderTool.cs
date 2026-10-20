using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Kun.Tool
{
    public static class ColliderTool
    {
        public static Vector3 GetWorldCenter (this Collider coll)
        {
            if (coll is BoxCollider boxCollider)
            {
                return boxCollider.transform.TransformPoint (boxCollider.center);
            }

            if (coll is CapsuleCollider capsuleCollider)
            {
                return capsuleCollider.transform.TransformPoint (capsuleCollider.center);
            }

            if (coll is SphereCollider sphereCollider)
            {
                return sphereCollider.transform.TransformPoint (sphereCollider.center);
            }

            return coll.transform.position;
        }

        /// <summary>
        /// 用點打出極短的射線進行物理檢測
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="layerMask"></param>
        /// <param name="hitBuffer"></param>
        /// <returns></returns>
        public static int CastAllNoAlloc (Vector3 center, Quaternion rot, int layerMask, RaycastHit[] hitBuffer)
        {
            var forward = rot * Vector3.forward;

            var hitCount = Physics.BoxCastNonAlloc (center, Vector3.one * 0.001f, forward, hitBuffer, rot, 0.000001f, layerMask);
            return hitCount;
        }


        /// <summary>
        /// 用點打出極短的射線進行物理檢測
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="layerMask"></param>
        /// <param name="hitBuffer"></param>
        /// <returns></returns>
        public static int CastAllNoAlloc (this IPoseProxy poseProxy, int layerMask, RaycastHit[] hitBuffer, float size = 0.1f)
        {
            var center = poseProxy.Pos;
            var rot = Quaternion.Euler (poseProxy.Euler);
            var forward = rot * Vector3.forward;

            var hitCount = Physics.BoxCastNonAlloc (center, Vector3.one * size * 0.5f, forward, hitBuffer, rot, 0.000001f, layerMask);
            return hitCount;
        }
        /// <summary>
        /// 用點打出極短的射線進行物理檢測
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="layerMask"></param>
        /// <param name="hitBuffer"></param>
        /// <returns></returns>
        public static int CastAllNoAlloc (this Transform entity, int layerMask, RaycastHit[] hitBuffer)
        {
            return CastAllNoAlloc (entity.position, entity.rotation, layerMask, hitBuffer);
        }

        public static int CastAllNoAlloc (this BoxCollider boxColl, int layerMask, RaycastHit[] hitBuffer)
        {
            Vector3 halfExtents = MathTool.Vector3Multiply (boxColl.size, boxColl.transform.lossyScale) / 2;
            Vector3 center = boxColl.transform.TransformPoint (boxColl.center);
            var hitCount = Physics.BoxCastNonAlloc (center, halfExtents, boxColl.transform.forward, hitBuffer, boxColl.transform.rotation, 0.000001f, layerMask);
            return hitCount;
        }

        public static List<RaycastHit> CastAll (this BoxCollider boxColl, int layerMask = -1)
        {
            Vector3 halfExtents = MathTool.Vector3Multiply (boxColl.size, boxColl.transform.lossyScale) / 2;
            Vector3 center = boxColl.transform.TransformPoint (boxColl.center);
            var hits = Physics.BoxCastAll (center, halfExtents, boxColl.transform.forward, boxColl.transform.rotation, 0.000001f, layerMask);
            return hits.ToList ();
        }

        public static List<Collider> OverlapBox (this BoxCollider boxColl, int layerMask = -1)
        {
            Vector3 halfExtents = MathTool.Vector3Multiply (boxColl.size, boxColl.transform.lossyScale) / 2;
            Vector3 center = boxColl.transform.TransformPoint (boxColl.center);
            var colls = Physics.OverlapBox (center, halfExtents, boxColl.transform.rotation, layerMask);
            return colls.ToList ();
        }

        /// <summary>
        /// range為可以使用的範圍, 避免穿模不要填滿
        /// y自動為0
        /// </summary>
        /// <param name="ignoreY"></param>
        /// <returns></returns>
        public static Vector3 GetRandomInnerPoint (this BoxCollider boxCollider, float useRange = 0.7f, bool ignoreY = true)
        {
            Vector3 pos = Vector3.zero;

            Vector3 delta = Vector3.zero;

            for (int i = 0; i < 3; i++)
            {
                var range = UnityEngine.Random.Range (useRange * -1, useRange);
                delta[i] = range * boxCollider.size[i] / 2;
            }

            var innerPoint = boxCollider.center + delta;

            pos = boxCollider.transform.TransformPoint (innerPoint);

            if (ignoreY)
            {
                pos.y = 0f;
            }

            return pos;
        }
    }
}
