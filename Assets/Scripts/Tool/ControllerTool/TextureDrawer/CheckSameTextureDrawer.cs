#define testMode
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class CheckSameTextureDrawer
	{
		/// <summary>
		/// 比較兩個值的相似度,
		/// 回傳0~1
		/// </summary>
		/// <param name="tex">Tex.</param>
		/// <param name="trackRectScale">Track rect scale.</param>
		/// <param name="drawColor">Draw color.</param>
		/// <param name="onUpdateTex">On update tex.</param>
		/// <param name="onCheckSameValueModify">On check same value modify.</param>
		public void Setup (Texture2D tex, int trackRectScale, Color drawColor, Action<Color[]> onUpdateTex, List<int> trackIndexs, Action<float> onCheckSameValueModify)
		{
			internalDrawer = new TextureDrawer ();
			internalDrawer.Setup (tex, trackRectScale, drawColor, (newPixels, indexs)=>
				{
					onUpdateTex.Invoke (newPixels);

					OnReceiveIndexsModify (indexs);
				});
					
			this.trackIndexs = trackIndexs.ToList ();
			this.onCheckSameValueModify = onCheckSameValueModify;

			trackPoints = trackIndexs.ConvertAll (trackIndex => TextureDrawer.GetPos (trackIndex, tex.width));
		}

		List<Vector2Int> trackPoints = new List<Vector2Int> ();

		public int Width
		{
			get
			{
				return internalDrawer.Width;
			}
		}

		public int Height
		{
			get
			{
				return internalDrawer.Height;
			}
		}

		List<int> trackIndexs;

		Action<float> onCheckSameValueModify;

		public void StartThread ()
		{
			receiveIndexsModifyLocker = new ReaderWriterLockSlim ();
			receiveIndexs = new List<int> ();
			
			checkSameLocker = new ReaderWriterLockSlim ();
			checkSameValue = null;

			internalDrawer.StartThread ();
		}

		ReaderWriterLockSlim receiveIndexsModifyLocker;
		List<int> receiveIndexs = new List<int> ();

		ReaderWriterLockSlim checkSameLocker;
		float? checkSameValue;

		Thread checkSameThread;

		void OnReceiveIndexsModify (List<int> cacheIndexs)
		{
			var cachePoints = cacheIndexs.ConvertAll (cacheIndex => TextureDrawer.GetPos (cacheIndex, internalDrawer.Width));

			List<Vector2Int> fullSamePoints = new List<Vector2Int> ();

			//稍微偏移的點
			List<Vector2Int> deviatePoints = new List<Vector2Int> ();

			var trackerPairs = trackPoints.ConvertAll (trackPoint => new RefKeyValuePair<Vector2Int, bool> (trackPoint, false));

			cachePoints.ToList ().ForEach (cachePoint=>
				{
					var findTrackIndex = trackerPairs.FindIndex (track => track.key == cachePoint);

					if (findTrackIndex > -1)
					{
						var pair = trackerPairs[findTrackIndex];
						cachePoints.Remove (cachePoint);
						pair.value = true;
						fullSamePoints.Add (cachePoint);
					}
				});

			//是否為相近點的依據
			Func<Vector2Int, Vector2Int, bool> CheckSameFunc = (Vector2Int point1, Vector2Int point2) => 
			{
				if (Mathf.Abs (point1.x - point2.x) <= 1)
				{
					if (Mathf.Abs (point1.y - point2.y) <= 1)
					{
						return true;
					}
				}

				return false;
			};

			cachePoints.ToList ().ForEach (cachePoint=>
				{
					var findTrackIndex = trackerPairs.FindIndex (track => CheckSameFunc(track.key, cachePoint));

					if (findTrackIndex > -1)
					{
						var pair = trackerPairs[findTrackIndex];
						cachePoints.Remove (cachePoint);

						// 點被占用不扣分也不加分
						// 點沒被占用當作加半分
						if (pair.value == false)
						{
							pair.value = true;
							deviatePoints.Add (cachePoint);
						}
					}
				});

			//剩下的就是完全偏移的點
			float value = fullSamePoints.Count + deviatePoints.Count * 0.7f - cachePoints.Count;

			float processValue = value / trackPoints.Count;

			#if testMode && UNITY_EDITOR
			UnityEngine.Debug.Log ("---------------------->");

			UnityEngine.Debug.Log ("完全相近 -> " + fullSamePoints.Count.ToString ());

			UnityEngine.Debug.Log ("部分偏移 -> " + deviatePoints.Count.ToString ());

			UnityEngine.Debug.Log ("完全偏移 -> " + cachePoints.Count.ToString ());

			UnityEngine.Debug.Log ("總數 -> " + trackPoints.Count.ToString ());

			UnityEngine.Debug.Log (processValue);
			#endif

			try
			{
				checkSameLocker.EnterReadLock ();

				checkSameValue = processValue;
			}
			catch (Exception e)
			{
				Debug.LogError (e.Message);
				Debug.LogError (e.StackTrace);
			}
			finally
			{
				checkSameLocker.ExitReadLock ();
			}
		}

		public void AddDrawTask (List<Vector2Int> drawTask)
		{
			internalDrawer.AddDrawTask (drawTask);
		}

		float timer = 0f;

		public void Update (float deltaTime)
		{
			internalDrawer.Update (deltaTime);

			try
			{
				checkSameLocker.EnterReadLock ();

				//不用刷新的這麼即時
				if(checkSameValue != null)
				{	
					onCheckSameValueModify.Invoke (checkSameValue.Value);
					
					checkSameValue = null;
				}
			}
			catch (Exception e)
			{
				Debug.LogError (e.Message);
				Debug.LogError (e.StackTrace);
			}
			finally
			{
				checkSameLocker.ExitReadLock ();
			}
		}

		TextureDrawer internalDrawer;

		public void ResetToOriginTex ()
		{
			StopThread ();
			onCheckSameValueModify.Invoke (0f);

			internalDrawer.ResetToOriginTex ();
		}

		public void Exit ()
		{
			internalDrawer.Exit ();
			this.StopThread ();
		}

		/// <summary>
		/// 外部不會主動呼叫離開,
		/// 大都是做了某事後要一起停掉
		/// </summary>
		protected void StopThread ()
		{
			if (checkSameThread != null) 
			{
				checkSameThread.Abort ();

				checkSameThread = null;
			}
		}
	}
}	