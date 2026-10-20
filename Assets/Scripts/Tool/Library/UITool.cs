using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public static class UITool
	{
		/// <summary>
		/// 讓UI的座標與世界座標相同
		/// </summary>
		/// <param name="uiTransform">User interface transform.</param>
		/// <param name="headCam">Head cam.</param>
		/// <param name="canvasTransfrom">Canvas transfrom.</param>
		/// <param name="worldPos">World position.</param>
		public static void SetToWorldPos (this RectTransform uiTransform, Camera headCam, RectTransform canvasTransfrom, Vector3 worldPos)
		{
			var offset = new Vector2 (canvasTransfrom.sizeDelta.x / 2f, canvasTransfrom.sizeDelta.y / 2f);

			Vector2 viewPos = headCam.WorldToViewportPoint (worldPos);
			Vector2 inCanvasPos = new Vector2(viewPos.x * canvasTransfrom.sizeDelta.x, viewPos.y * canvasTransfrom.sizeDelta.y);

			//一個0,0在左上, 一個0,0在中間 所以要轉換
			var localPos = (inCanvasPos - offset);

			//UI不一定直接是Canvas子物件, 還是改到世界比較保險
			var pos = canvasTransfrom.TransformPoint (localPos);

			uiTransform.position = pos;
		}

		public static List<Vector3> GetBoundPoints (this Renderer render)
		{
			if (render is MeshRenderer)
			{
				return (render as MeshRenderer).GetBoundPoints ();
			}
			else if (render is SkinnedMeshRenderer)
			{
				return (render as SkinnedMeshRenderer).GetBoundPoints ();
			}
			else
			{
				Debug.LogError ($"not support -> {render.GetType ()}");
				return new List<Vector3> ();
			}
		}

		public static List<Vector3> GetBoundPoints (this MeshRenderer render)
		{
			Bounds bounds = render.bounds;

			Quaternion boundsRot = render.transform.rotation;

			return MathTool.GetBoundPoints (bounds, boundsRot);
		}

		public static List<Vector3> GetBoundPoints (this SkinnedMeshRenderer skin)
		{
			Bounds bounds = skin.bounds;

			Quaternion boundsRot;

			//rootBone的本地旋轉由骨架控制 所以整個skin的旋轉要看 rootBone的parent
			Transform rootBoundsParent = skin.rootBone.parent;

			if (rootBoundsParent != null)
			{
				boundsRot = rootBoundsParent.rotation;
			}
			else
			{
				boundsRot = Quaternion.identity;
			}

			return MathTool.GetBoundPoints (bounds, boundsRot);
		}


		public static CanvasBoundsData GetCanvasBoundsData (this Renderer render, Camera refCamera)
		{
			List<Vector3> boundPoints = render.GetBoundPoints ();

			float minX = float.MaxValue;
			float maxX = float.MinValue;

			float minY = float.MaxValue;
			float maxY = float.MinValue;

			boundPoints.Map ((i, boundPoint) =>
			{
				Vector3 screenPoint = refCamera.WorldToScreenPoint (boundPoint);

				if (screenPoint.x < minX)
				{
					minX = screenPoint.x;
				}

				if (screenPoint.x > maxX)
				{
					maxX = screenPoint.x;
				}

				if (screenPoint.y < minY)
				{
					minY = screenPoint.y;
				}

				if (screenPoint.y > maxY)
				{
					maxY = screenPoint.y;
				}
			});

			List<Vector3> screenPoints = new List<Vector3> ();

			//畫出一個矩形包住整個bounds在
			screenPoints.Add (new Vector3 (minX, minY));
			screenPoints.Add (new Vector3 (minX, maxY));
			screenPoints.Add (new Vector3 (maxX, minY));
			screenPoints.Add (new Vector3 (maxX, maxY));

			CanvasBoundsData data = new CanvasBoundsData ();
			data.centerPos = new Vector3 (((minX + maxX) / 2), ((minY + maxY) / 2));

			float xSize = Mathf.Abs (maxX - minX);
			float ySize = Mathf.Abs (maxY - minY);

			data.sizeDelta = new Vector2 (xSize, ySize);

			return data;
		}
	}
}
