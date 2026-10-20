using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class TextureMerger : MonoBehaviour 
	{
		[SerializeField]
		List<TexData> texDatas = new List<TexData> ();

		[SerializeField][ReadOnly]
		Rect totalRect;

		[SerializeField]
		float resize = 1f;

		public Texture2D CreateMergeTexture ()
		{	
			totalRect = new Rect ();

			texDatas.ForEach (texData=>
				{
					var tex2D = texData.tex as Texture2D;

					var size = new Vector2 (tex2D.width, tex2D.height);

					Rect texRect = new Rect (texData.delta, size);

					ExtenstionRect (ref totalRect, texRect);
				});


			var preTex = new Texture2D ((int)totalRect.width, (int)totalRect.height, TextureFormat.ARGB32, false);
			//產出跟圖片一樣大的 alpha0
			Color[] bgColors = new int[(int)totalRect.width * (int)totalRect.height].ToList ().ConvertAll (i => new Color (1, 1, 1, 0)).ToArray ();

			preTex.SetPixels (bgColors);

			foreach (var texData in texDatas) 
			{
				var tex2D = texData.tex as Texture2D;

				// rect的左上要拉到00
				int deltaX = texData.delta.x - (int)totalRect.xMin;
				int deltaY = texData.delta.y - (int)totalRect.yMax;

				for (int i = 0; i < tex2D.width; i++) 
				{
					for (int j = 0; j < tex2D.height; j++) 
					{
						var pixel = tex2D.GetPixel (i, j);

						if (pixel.a != 0) 
						{
							int xPos = i + deltaX;
							int yPos = j + deltaY;

							preTex.SetPixel (xPos, yPos, pixel);
						}

					}
				}
			}

			preTex.Apply ();
			//wrapMode要在apply之後改
			preTex.wrapMode = TextureWrapMode.Clamp;

			if (resize != 1) 
			{
				int newWidth = (int)(preTex.width * resize);

				int newHeight = (int)(preTex.height * resize);

				RenderTexture rt = new RenderTexture (newWidth, newHeight, 24);
				RenderTexture.active = rt;
				Graphics.Blit (preTex, rt);

				Texture2D resizeTex = new Texture2D (newWidth, newHeight, TextureFormat.ARGB32, false);
				resizeTex.ReadPixels (new Rect (0, 0, newWidth, newHeight), 0, 0);
				resizeTex.Apply ();
				resizeTex.wrapMode = TextureWrapMode.Clamp;
				return resizeTex;
			}
			else
			{
				return preTex;
			}

		}

		/// <summary>
		/// 用一個rect把對方撐開
		/// </summary>
		/// <param name="srcRect">Source rect.</param>
		/// <param name="extenstionRect">Extenstion rect.</param>
		void ExtenstionRect(ref Rect srcRect, Rect extenstionRect)
		{
			if (extenstionRect.xMin < totalRect.xMin)
			{
				totalRect.xMin = extenstionRect.xMin;
			}

			if (extenstionRect.xMax > totalRect.xMax)
			{
				totalRect.xMax = extenstionRect.xMax;
			}

			if (extenstionRect.yMin < totalRect.yMin)
			{
				totalRect.yMin = extenstionRect.yMin;
			}

			if (extenstionRect.yMax > totalRect.yMax)
			{
				totalRect.yMax = extenstionRect.yMax;
			}
		}
	}

	[Serializable]
	public class TexData
	{
		public Texture tex;
		public Vector2Int delta;
	}
}