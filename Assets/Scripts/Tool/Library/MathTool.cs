using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public static class MathTool
	{
        /// <summary>
        /// 檢查兩個物件是否因為距離交錯
        /// 只比對水平視角
        /// -1代表完全相反
        /// 0代表垂直
        /// passDot為允許通過的數值
        /// staggeredDist為要交錯多少距離才算
        /// localPos為把背面當作前方的話的本地座標
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckDistStaggered (Transform src, Transform target, out Vector3 localPos, float passDot = -0.5f, float staggeredDist = 0.5f)
        {
			//不要比對仰角
			var headingA = src.transform.position - target.transform.position;
            headingA.y = 0f;
            headingA.Normalize ();

            var headingB = target.transform.forward;
            headingB.y = 0f;
            headingB.Normalize ();

            var dot = Vector3.Dot (headingA, headingB);

            if (dot >= passDot)
            {
                localPos = default;

                return false;
            }

			var delta = src.position - target.position;

            var headingRot = Quaternion.LookRotation (headingA);

            //算出a遠離b的座標
            localPos = Quaternion.Inverse (headingRot) * (delta);

			return (localPos.z) > staggeredDist;
        }

        /// <summary>
        /// 檢查兩個物件是否交錯
        /// 只比對水平視角
        /// -1代表完全相反
        /// 0代表垂直
        /// passDot為允許通過的數值
        /// staggeredDist為要交錯多少距離才算
        /// localPos為把背面當作前方的話的本地座標
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckStaggered (Transform src, Transform target, out Vector3 localPos, float passDot = -0.5f, float staggeredDist = 0.5f)
		{
			//不要比對仰角
			var headingA = src.transform.forward;
			headingA.y = 0f;
			headingA.Normalize ();

            var headingB = target.transform.forward;
            headingB.y = 0f;
            headingB.Normalize ();

			var dot = Vector3.Dot (headingA, headingB);

			if (dot >= passDot) 
			{
				localPos = default;

                return false;
			}

			var delta = target.position - src.position;

			var headingRot = Quaternion.LookRotation (headingA);

			//算出a遠離b的座標
			localPos = Quaternion.Euler (0, 180, 0) * (headingRot) * (delta);

            return localPos.z > staggeredDist;
        }


        public static Pose GetWorldPose (this Transform selfTransfrom, SerlizePose localPose)
		{
			return GetWorldPose (selfTransfrom, localPose.pos, localPose.GetRot ());
		}
		
		public static Pose GetWorldPose (this Transform selfTransfrom, Pose localPose)
		{
			return GetWorldPose (selfTransfrom, localPose.pos, localPose.rot);
		}

		public static Pose GetWorldPose (this Transform selfTransfrom, Vector3 localPos, Quaternion localRot)
		{
			Vector3 worldPoint = selfTransfrom.TransformPoint (localPos);
			Quaternion worldRot = selfTransfrom.rotation * localRot;

			return new Pose (worldPoint, worldRot);
		}

		/// <summary>
		/// 這個Axis在Vector3內的index
		/// </summary>
		/// <returns>The axis index.</returns>
		public static int GetAxisIndex (this Axis axis)
		{
			if (axis == Axis.X) 
			{
				return 0;
			}

			if (axis == Axis.Y)
			{
				return 1;
			}

			if (axis == Axis.Z) 
			{
				return 2;
			}

			return -1;
		}

		public static Vector3 GetAxisVector (this Axis axis) 
		{
			var axisIndex = axis.GetAxisIndex ();

			Vector3 axisVector = new Vector3 ();
			axisVector[axisIndex] = 1;

			return axisVector;
		}

		public static Vector3 Vector3Multiply (Vector3 a, Vector3 b)
		{
			return new Vector3 (a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public static Vector2 Vector2Multiply (Vector2 a, Vector2 b)
		{
			return new Vector2 (a.x * b.x, a.y * b.y);
		}

		public static Vector3 Vector3Division (Vector3 a, Vector3 b)
		{
			return new Vector3 (a.x / b.x, a.y / b.y, a.z / b.z);
		}

		public static Vector2 Vector2Division (Vector2 a, Vector2 b)
		{
			return new Vector2 (a.x / b.x, a.y / b.y);
		}

		public static List<Vector3> GetBoundPoints (this Bounds bounds, Quaternion boundsRot)
		{
			Vector3 center = bounds.center;
			Vector3 extents = bounds.extents;

			List<Vector3> boundPoints = new List<Vector3> ();

			boundPoints.Add (center + boundsRot * extents);

			boundPoints.Add (center + boundsRot * Vector3Multiply (extents, new Vector3 (1, -1, 1)));

			boundPoints.Add (center + boundsRot * Vector3Multiply (extents, new Vector3 (-1, 1, 1)));

			boundPoints.Add (center + boundsRot * Vector3Multiply (extents, new Vector3 (-1, -1, 1)));

			boundPoints.Add (center + boundsRot * Vector3Multiply (extents, new Vector3 (1, 1, -1)));

			boundPoints.Add (center + boundsRot * Vector3Multiply (extents, new Vector3 (1, -1, -1)));

			boundPoints.Add (center + boundsRot * Vector3Multiply (extents, new Vector3 (-1, 1, -1)));

			boundPoints.Add (center + boundsRot * Vector3Multiply (extents, new Vector3 (-1, -1, -1)));

			return boundPoints;
		}

		/// <summary>
		/// 轉Z軸
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="rotDegree"></param>
		/// <returns></returns>
		public static Vector2 RotateV2 (Vector2 dir, float rotDegree)
		{
			return Quaternion.Euler (0, 0, rotDegree) * dir;
		}

		public static float Delta (float f1, float f2)
		{
			return Mathf.Abs (f1 - f2);
		}

		/// <summary>
		/// 比較兩個 Quaternio的dot結果是否大於某個值
		/// </summary>
		/// <param name="q1">Q1.</param>
		/// <param name="q2">Q2.</param>
		/// <param name="deviationRange">Deviation range.</param>
		public static bool Approximately (this Quaternion q1, Quaternion q2, float deviationRange = 0.7f)
		{
			float successRange = 1 - deviationRange;

			//Quaternion.Dot與向量的dot概念不太相同 相乘後才會有接近的概念
			float dotValue = Vector3.Dot (q1 * Vector3.right, q2 * Vector3.right);

			return dotValue >= successRange;
		}

		/// <summary>
		/// 少於幾%之前都為0 將%數以後的 擴大補償
		/// </summary>
		/// <returns>The divide process.</returns>
		/// <param name="progress">Progress.</param>
		/// <param name="divide">Divide.</param>
		public static float GetDivideProcess (float progress, float divide)
		{
			float remaining = progress - divide;

			if (remaining <= 0)
				remaining = 0;

			return remaining / (1 - divide);
		}
	}

	public enum Axis
	{
		X,
		Y,
		Z
	}

	public class CanvasBoundsData
	{
		public Vector3 centerPos;

		public Vector2 sizeDelta;
	}

	[Serializable]
	public class Pose
	{
		public Pose ()
		{

		}

		public Pose (Vector3 pos, Vector3 euler) : this (pos, Quaternion.Euler (euler))
		{
		}

		public Pose (Transform poseTarget) : this (poseTarget.position, poseTarget.rotation)
		{

		}

		public Pose (Vector3 pos, Quaternion rot)
		{
			this.pos = pos;
			this.rot = rot;
		}

		public Vector3 pos;
		public Quaternion rot;

		public static Pose Lerp (Pose fromPose, Pose toPose, float lerp)
		{
			Pose lerpPose = new Pose ();
			lerpPose.pos = Vector3.Lerp (fromPose.pos, toPose.pos, lerp);
			lerpPose.rot = Quaternion.Lerp (fromPose.rot, toPose.rot, lerp);

			return lerpPose;
		}
	}

	/// <summary>
	/// 旋轉用V3記, 才有辦法手填
	/// </summary>
	[Serializable]
	public class SerlizePose
	{
		public SerlizePose ()
		{

		}

		public SerlizePose (Vector3 pos, Vector3 euler)
		{
			this.pos = pos;
			this.euler = euler;
		}

		public Vector3 pos;
		public Vector3 euler;

		public Quaternion GetRot ()
		{
			return Quaternion.Euler (euler);
		}
	}

	public enum CompareType 
	{
		[EnumMsg("大於")]
		More,
		[EnumMsg ("小於")]
		Less
	}

	public enum CompareType_WithEqual
	{
		[EnumMsg ("大於")]
		More,
		[EnumMsg ("大於等於")]
		MoreEqual,
		[EnumMsg ("等於")]
		Equal,
		[EnumMsg ("小於")]
		Less,
		[EnumMsg ("小於等於")]
		LessEqual

	}

	public class ValueRangeRouter<T> 
	{
		public ValueRangeRouter (Action<T> cacher, ValueRange<T> ranger)
		{
			this.cacher = cacher;
			this.ranger = ranger;
		}

		public void OnProgressUpdate (float progress) 
		{
			var value = ranger.GetValue (progress);

			cacher.Invoke (value);
		}

		Action<T> cacher;

		ValueRange<T> ranger;
	}

	[Serializable]
    public class FloatRange : ValueRange<float>
    {
        public override float GetValue (float progress)
        {
			return Mathf.Lerp (from, to, progress);
        }
    }

    [Serializable]
	public abstract class ValueRange <T>
	{
		[SerializeField]
		protected T from;

		[SerializeField]
		protected T to;

		public abstract T GetValue (float progress);
	}
}
