using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public static class RaycastTool
	{
		/// <summary>
		/// 測試起點到Rect的點上是否有任意點可以被看到
		/// 其中一個點可以被看到就當作物體可以被看到
		/// </summary>
		/// <returns><c>true</c>, if rect transform was tested, <c>false</c> otherwise.</returns>
		/// <param name="rectTransform">Rect transform.</param>
		/// <param name="testOrigin">Test origin.</param>
		public static bool TestRectTransform (RectTransform rectTransform, Camera cam, int layerMask = ~0)
		{
			var originPos = cam.transform.position;
			
			var points = GetPoints (rectTransform);

			bool hasPass = points.Any (point=>
				{
					if (CheckInView (cam, point) == false)
					{
						return false;
					}
					
					float dist = Vector3.Distance (point, originPos);

					var dir = point - originPos;

					Debug.DrawRay (originPos,dir, Color.red, 0.1f);

					return Physics.Raycast (originPos,dir,dist, layerMask) == false;
				});

			//一個可以被看到就是看的到
			return hasPass;
		}

		/// <summary>
		/// 檢查這個點是否在攝影機的範圍內
		/// </summary>
		/// <returns><c>true</c>, if in view was checked, <c>false</c> otherwise.</returns>
		/// <param name="cam">Cam.</param>
		/// <param name="checkPoint">Check point.</param>
		public static bool CheckInView (Camera cam, Vector3 checkPoint)
		{
			var viewPoint = cam.WorldToViewportPoint (checkPoint);

			if (viewPoint.z > 0) 
			{
				//這些點都需要在0~1之間不能就是超出螢幕
				List<float> rangePoint = new List<float>{ viewPoint.x, viewPoint.y };

				return rangePoint.TrueForAll (p=>
					{
						return p<=1f && p>=0f;
					});
			}

			return false;
		}
		
		/// <summary>
		/// 包括中點在內, 以及外圍的4個點
		/// 只適合錨點置中的類型
		/// </summary>
		/// <returns>The points.</returns>
		/// <param name="rectTransform">Rect transform.</param>
		public static List<Vector3> GetPoints (RectTransform rectTransform)
		{
			List<Vector3> points = new List<Vector3> ();

			var size = rectTransform.sizeDelta;

			points = GetPoints (rectTransform, size.x, size.y);

			return points;
		}

		/// <summary>
		/// 透過size與中點反推五個點
		/// </summary>
		/// <returns>The points.</returns>
		/// <param name="rectTransform">Rect transform.</param>
		public static List<Vector3> GetPoints (Transform transform, float width, float height)
		{
			List<Vector3> points = new List<Vector3> ();

			points.Add (transform.position);

			float extentX = width * 0.5f;
			float extentY = height * 0.5f;

			List<Vector3> localPositions = new List<Vector3> ();

			for (int i = 0; i < 2; i++) 
			{
				//抓一正一負
				int xPositivValue = (i % 2 == 0) ? 1 : -1;

				for (int j = 0; j < 2; j++) 
				{
					int yPositivValue = (j % 2 == 0) ? 1 : -1;

					localPositions.Add (new Vector3 (extentX * xPositivValue, extentY * yPositivValue, 0));
				}
			}

			//已知size跟position坐標系是1比1
			//把圖片當作Rect的子物件想, 所以圖片size映射回去就能求得圖片的4個點的世界座標
			var boards = localPositions.ConvertAll (localPos => transform.TransformPoint (localPos));

			points.AddRange (boards);

			return points;
		}
	}
}
