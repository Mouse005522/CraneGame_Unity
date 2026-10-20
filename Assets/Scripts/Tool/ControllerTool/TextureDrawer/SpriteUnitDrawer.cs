using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class SpriteUnitDrawer : MonoBehaviour 
	{
		[SerializeField]
		SpriteRenderer processRender;

		[SerializeField]
		Color drawColor = Color.red;

		TextureDrawer drawer;

		Texture2D newTex;

		void Awake ()
		{
			var oldTex = processRender.sprite.texture;

			trackRectScale = 540 / oldTex.width;

			if (trackRectScale <= 0)
			{
				trackRectScale = 1;
			}

			drawer = new TextureDrawer ();
			drawer.Setup (oldTex, trackRectScale, drawColor, OnRefreshTex);
			drawer.StartThread ();

			cachePixelsPerUnit = processRender.sprite.pixelsPerUnit;
			newTex = new Texture2D (oldTex.width, oldTex.height, TextureFormat.ARGB32, false);
			newTex.filterMode = oldTex.filterMode;
		}

		Vector2Int? lastDeltaPoint = null;

		void Update()
		{
			Vector2Int? curDeltaPoint = null;

			if (Input.GetMouseButton (0))
			{
				var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast (ray, out hit) && hit.collider.gameObject == this.gameObject)
				{
					var hitDeltaPoint = GetHitDeltaPoint (hit);

					List<Vector2Int> drawTask = new List<Vector2Int> ();

					drawTask.Add (hitDeltaPoint);

					if (lastDeltaPoint != null) 
					{
						drawTask.Add (lastDeltaPoint.Value);
					}

					drawer.AddDrawTask (drawTask);

					curDeltaPoint = hitDeltaPoint;
				}
			}

			drawer.Update (Time.deltaTime);

			lastDeltaPoint = curDeltaPoint;

			if (Input.GetKeyDown (KeyCode.Escape)) 
			{
				drawer.ResetToOriginTex ();
				drawer.StartThread ();
			}
		}

		int trackRectScale;

		Vector2Int GetHitDeltaPoint (RaycastHit hit)
		{
			var scale = processRender.transform.lossyScale;
			var deltaPixel = (hit.collider.transform.InverseTransformPoint (hit.point)) * cachePixelsPerUnit;

			//pixel的最左是0, 所以要把-1補到0為止
			var halfX = drawer.Width / 2;
			float xDelta = (((deltaPixel.x / halfX) + 1) * halfX);

			var halfY = drawer.Height / 2;
			float yDelta = (((deltaPixel.y / halfY) + 1) * halfY);

			return new Vector2Int ((int)xDelta, (int)yDelta);
		}

		float cachePixelsPerUnit;

		void OnRefreshTex (Color[] refreshPixels, List<int> cacheIndexs)
		{	
			newTex.SetPixels (refreshPixels);
			newTex.Apply ();

			var newRect = processRender.sprite.rect;

			processRender.sprite = Sprite.Create (newTex, newRect, Vector2.one * 0.5f, cachePixelsPerUnit);
		}

		void OnApplicationQuit ()
		{
			if (drawer != null) 
			{
				drawer.Exit ();
			}
		}

		[ContextMenu ("GetMsg")]
		void GetMsg ()
		{
			
			Debug.LogFormat ("size -> {0}", processRender.sprite.pixelsPerUnit);
			Debug.LogFormat ("size -> {0}", JsonUtility.ToJson (processRender.sprite.rect));

			Debug.LogFormat ("texture Size -> {0}", JsonUtility.ToJson (new Vector2 (processRender.sprite.texture.width, processRender.sprite.texture.height)));

			Debug.LogFormat ("rect size -> {0}", JsonUtility.ToJson (processRender.sprite.rect.size));
			Debug.LogFormat ("rect center -> {0}", JsonUtility.ToJson (processRender.sprite.rect.center));
			Debug.LogFormat ("pivot -> {0}", JsonUtility.ToJson (processRender.sprite.pivot));
			Debug.LogFormat ("color size -> {0}", processRender.sprite.texture.GetPixels ().Length);
		}
	}
}