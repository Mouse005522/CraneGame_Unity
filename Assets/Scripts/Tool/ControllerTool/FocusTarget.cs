using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
	public class FocusTarget : MonoBehaviour
	{
		[Header("當前座標超過目標這個距離就會開始移動")]
		public float leastRange = 5f;

        [Header ("當前Y座標超過目標這個距離就會開始移動")]
        public float leastYRange = 0.3f;

		public float speed = 1.5f;

        [Header("與頭顯的偏移量")]
		public Vector3 plus;

		[Space]
		[Space]

		[SerializeField]
		[ReadOnly]
		Transform focusTarget;

		[SerializeField]
		[ReadOnly]
		Transform lookTarget;

		[SerializeField]
		[ReadOnly]
		TraceState traceState;

        /// <summary>
        /// focusTarget 為跟隨者
        /// lookTarget 為UI要看相的目標
        /// </summary>
        /// <param name="focusPosTarget"></param>
        /// <param name="focusRotTarget"></param>
        /// <param name="plus"></param>
        /// <param name="lookTarget"></param>
        /// <param name="mirror"></param>
        /// <param name="speed"></param>
        /// <param name="leastRange"></param>
        public void Init (Transform focusTarget, Transform lookTarget)
		{
			this.focusTarget = focusTarget;

			this.lookTarget = lookTarget;

			Trace (true);
		}

		[Header("直接刷新, 不靠lerp")]
		[SerializeField]
		bool alwaysImmediately = false;

        // Update is called once per frame
        void LateUpdate ()
		{
			if (focusTarget == null)
			{
				traceState = TraceState.sleep;
				return;
			}
			else if (alwaysImmediately)
			{
				ForceMoveToTracePose ();
				return;
            }

            switch (traceState)
			{
				case TraceState.trace:
					{
						Trace ();
						break;
					}

				case TraceState.sleep:
					{
						OverLook ();
						break;
					}
			}
        }

		public void ForceMoveToTracePose ()
		{
			Trace (true);
		}

		Vector3 GetNextPoint ()
		{
			if (focusTarget != null)
			{
				Quaternion dirRot;

				if (followElevationAngle == false)
				{
					dirRot = Quaternion.Euler (0, focusTarget.eulerAngles.y, 0);
				}
				else 
				{
					dirRot = focusTarget.rotation;
                }

				Vector3 nextPoint = focusTarget.position + dirRot * plus;

				return nextPoint;
			}
			else
			{
				LoggerRouter.Error ($"尚未指定focusTarget");
				return Vector3.zero;
			}
		}

		void Trace (bool immediately = false)
		{
			var nextPoint = GetNextPoint ();

			if (immediately)
			{
				this.transform.position = nextPoint;
			}
			else
			{
				var dir = nextPoint - this.transform.position;
				var dist = Vector3.Magnitude (dir);

				var processSpeed = speed * Time.deltaTime;

				if (dist < processSpeed)
				{
					this.transform.position += dir;
				}
				else
				{
					this.transform.position += dir.normalized * processSpeed;
				}
			}

			DoLook ();

			if (Vector3.Distance (nextPoint, this.transform.position) <= 0.001f)
			{
				traceState = TraceState.sleep;
			}
		}

		[SerializeField]
		bool followElevationAngle = false;

        void DoLook ()
		{
			if (lookTarget != null)
			{
				Vector3 lookPoint = lookTarget.position;
				
				if (followElevationAngle == false) 
				{
					lookPoint.y = this.transform.position.y;
				}

				this.transform.LookAt (lookPoint);
				this.transform.rotation *= Quaternion.Euler (new Vector3 (0, 180, 0));
			}
		}

		void OverLook ()
		{
			DoLook ();

			var lookPoint = GetNextPoint ();

			var dist = Vector3.Distance (lookPoint, this.transform.position);

			var yDist = Mathf.Abs (lookPoint.y - this.transform.position.y);


			if (dist > leastRange || yDist > leastYRange)
			{
				traceState = TraceState.trace;
			}
		}

		public enum TraceState
		{
			trace, sleep
		}


		float GetProcessVector3Abs (Vector3 a, Vector3 b)
		{
			var range = 0f;

			range += Mathf.Abs (a.x - b.x);
			range += Mathf.Abs (a.z - b.z);

			return range;
		}
	}
}