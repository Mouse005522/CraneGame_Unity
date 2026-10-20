using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    [Serializable]
	public class TextureDrawer
	{
		Action<Color[], List<int>> onUpdateTex;
		Color drawColor;

		/// <summary>
		/// HashSet以多長為一個範圍
		/// </summary>
		const int HashSetRange = 1000;

		#region cache

		Color[] originPixels = null;

		public int Width
		{
			get
			{
				return width;
			}
		}

		[SerializeField][ReadOnly]
		int width;

		public int Height
		{
			get
			{
				return height;
			}
		}

		[SerializeField][ReadOnly]
		int height;

		List<Vector2Int> pointGroup = new List<Vector2Int> ();
		#endregion

		#region const setting

		const int half = 10;

		int Half 
		{
			get
			{
				var processHalf = (int)(half / trackRectScale);

				if (processHalf < 1) 
				{
					processHalf = 1;
				}

				return processHalf;
			}
		}

		#endregion

		#region modify
		Color[] cachePixels = null;

		//分兩個 減少lock的時間
		Color[] refreshPixels = null;

		List<HashSet<int>> cacheSets = new List<HashSet<int>> ();

		List<int> modifyIndexs = new List<int> ();
		List<int> refreshIndexs = new List<int> ();
		#endregion

		#region locker
		Thread calculatePixelThread;
		ReaderWriterLockSlim calculatePixelTaskLocker;

		bool refreshMark = false;

		ReaderWriterLockSlim refreshMarkLocker;

		List<List<Vector2Int>> drawTasks = new List<List<Vector2Int>> ();
		#endregion

		int trackRectScale;

		public void Setup (Texture2D tex, int trackRectScale, Color drawColor, Action<Color[], List<int>> onUpdateTex, List<int> modifyIndexs = null)
		{
			this.drawColor = drawColor;
			this.onUpdateTex = onUpdateTex;

			this.trackRectScale = trackRectScale;

			width = tex.width;
			height = tex.height;

			cachePixels = tex.GetPixels ();
			originPixels = cachePixels.ToArray ();
			refreshPixels = cachePixels.ToArray ();

			CreatePointGroup ();
			CreateCacheSet ();

			ProcessOldHistory (modifyIndexs);
		}

		void ProcessOldHistory (List<int> modifyIndexs)
		{
			this.modifyIndexs = new List<int> ();
			
			if (modifyIndexs != null) 
			{
				bool hasModify;

				modifyIndexs.ForEach (index=>
					{
						//這個會去寫入 this.modifyIndexs
						TryMaskPointIndex (index , out hasModify);
					});

				onUpdateTex?.Invoke (cachePixels, modifyIndexs);
			}

			refreshIndexs = this.modifyIndexs.ToList ();
		}

		public void AddDrawTask (List<Vector2Int> drawTask)
		{
			try
			{	
				calculatePixelTaskLocker.EnterWriteLock ();
				drawTasks.Add(drawTask);
			}
			catch(Exception e)
			{
				Debug.LogError (e.StackTrace);
			}
			finally
			{
				calculatePixelTaskLocker.ExitWriteLock ();
			}
		}

		public void Update (float deltaTime)
		{
			try
			{
				refreshMarkLocker.EnterReadLock ();

				//不用刷新的這麼即時
				if(refreshMark)
				{	
					RefreshTex ();
					refreshMark = false;
				}
			}
			catch (Exception e)
			{
				Debug.LogError (e.Message);
				Debug.LogError (e.StackTrace);
			}
			finally
			{
				refreshMarkLocker.ExitReadLock ();
			}
		}

		public void StartThread ()
		{
			calculatePixelTaskLocker = new ReaderWriterLockSlim ();
			refreshMarkLocker = new ReaderWriterLockSlim ();

			calculatePixelThread = new Thread (UpdateDrawTask);
			calculatePixelThread.Start ();
		}

		/// <summary>
		/// 外部不會主動呼叫離開,
		/// 大都是做了某事後要一起停掉
		/// </summary>
		protected void StopThread ()
		{
			if (calculatePixelThread != null) 
			{
				calculatePixelThread.Abort ();

				calculatePixelThread = null;
			}
		}

		public void ResetToOriginTex () 
		{
			StopThread ();

			drawTasks.Clear ();
			modifyIndexs.Clear ();
			refreshIndexs.Clear ();

			cachePixels = originPixels.ToArray ();
			refreshPixels = originPixels.ToArray ();

			//把每個set都clear掉就好
			//總集合的長度取決於pixel的總數
			cacheSets.ForEach (cacheSet => cacheSet.Clear ());

			calculatePixelTaskLocker = new ReaderWriterLockSlim ();
			refreshMarkLocker = new ReaderWriterLockSlim ();
			refreshMark = false;

			RefreshTex ();
		}

		public void Exit ()
		{
			StopThread ();
		}

		void RefreshTex ()
		{	
			onUpdateTex?.Invoke (refreshPixels, refreshIndexs);
		}

		void UpdateDrawTask ()
		{
			while (true) 
			{
				List<List<Vector2Int>> copyTask = new List<List<Vector2Int>> ();

				try
				{
					calculatePixelTaskLocker.EnterReadLock ();
					copyTask = drawTasks.ToList ();
					drawTasks.Clear ();	
				}
				catch(Exception e)
				{
					Debug.LogError (e.StackTrace);
				}
				finally
				{
					calculatePixelTaskLocker.ExitReadLock ();
				}

				bool hasModify = false;

				try
				{
					copyTask.ForEach (drawTask=>
						{
							Vector2Int beginPoint = drawTask[0];
							Vector2Int? endPoint = null;

							//2就是有終點, 1就是只有起點
							if(drawTask.Count > 1)
							{
								endPoint = drawTask[1];
							}
							else
							{
								endPoint = null;
							}

							var track = GetTrack (beginPoint, endPoint);

							track.ForEach (point =>
								{
									bool anyPointModify = false;

									TryMaskPointGroup (point, out anyPointModify);

									if (anyPointModify)
									{
										hasModify = true;
									}
								});
						});	
				}
				catch (Exception e)
				{
					Debug.LogError (e.Message);
					Debug.LogError (e.StackTrace);
				}

				if (hasModify) 
				{
					try
					{
						refreshMarkLocker.EnterWriteLock ();

						if (hasModify) 
						{
							refreshMark = true;
							refreshPixels = cachePixels.ToArray ();
							refreshIndexs = modifyIndexs.ToList();
						}
					}
					catch (Exception e)
					{
						Debug.LogError (e.Message);
						Debug.LogError (e.StackTrace);
					}
					finally
					{
						refreshMarkLocker.ExitWriteLock ();
					}
				}

				Thread.Sleep (1);
			}
		}

		/// <summary>
		/// 依據上一次點擊的點, 產生中間補間的點
		/// </summary>
		/// <returns>The tracks.</returns>
		List<Vector2Int> GetTrack (Vector2Int beginPoint, Vector2Int? endPoint)
		{
			//沒有上一個點
			//重新當作起點
			if (endPoint == null || endPoint.Value == beginPoint)
			{
				return new List<Vector2Int>{ beginPoint };
			}
			else
			{
				List<int> xTrack = GetAxisTrack (beginPoint.x, endPoint.Value.x);
//
				List<int> yTrack = GetAxisTrack (beginPoint.y, endPoint.Value.y);

				//數量多得當主導
				List<int> mainTrack = null;

				List<int> subTrack = null;

				bool mainIsX;

				if (xTrack.Count > yTrack.Count)
				{
					mainTrack = xTrack;
					subTrack = yTrack;

					mainIsX = true;
				}
				else
				{
					mainTrack = yTrack;
					subTrack = xTrack;

					mainIsX = false;
				}

				List<Vector2Int> track = new List<Vector2Int> ();

				try
				{
					track = GetMappingPos (mainTrack, subTrack, mainIsX);
				}
				catch(Exception e)
				{
					Debug.LogError (e.Message);
					Debug.LogError (e.StackTrace);
				}

				return track;
			}
		}

		/// <summary>
		/// 傳入兩個list, 找出大概相對的值
		/// </summary>
		/// <returns>The mapping value.</returns>
		List<Vector2Int> GetMappingPos (List<int> srcValues, List<int> targetValues, bool mainIsX)
		{
			List<RefKeyValuePair<float, int>> srcTable = new List<RefKeyValuePair<float, int>> ();

			//不緩存是不希望緩存到無窮小數然後導致被誤差被放大
			srcValues.Map ((index, value)=>
				{
					float progressValue = (float)index / (srcValues.Count - 1);

					srcTable.Add (progressValue, value);
				});

			List<RefKeyValuePair<float, int>> targetTable = new List<RefKeyValuePair<float, int>> ();

			//不緩存是不希望緩存到無窮小數然後導致被誤差被放大
			targetValues.Map ((index, value)=>
				{
					float progressValue = (float)index / (targetValues.Count - 1);

					targetTable.Add (progressValue, value);
				});


			List<Vector2Int> points = new List<Vector2Int> ();

			srcTable.ForEach (srcPair=>
				{
					///找到第一個比我大的
					var findIndex = targetTable.FindIndex (targetPair => targetPair.key > srcPair.key);

					var targetPoint = 0;

					if (findIndex > 0)
					{
						var prevIndex = findIndex - 1;

						var findValue = targetTable[findIndex].key;
						var prevValue = targetTable[prevIndex].key;

						//看是靠這個近還是靠上一個近
						if (Mathf.Abs (findValue - srcPair.key) > Mathf.Abs (prevValue - srcPair.key))
						{
							targetPoint = targetTable[findIndex].value;
						}
						else
						{
							targetPoint = targetTable[prevIndex].value;
						}

						//表示需要1,2 就好
						if(findIndex >= 2)
						{
							targetTable.RemoveAt (0);
						}
					}
					else if (findIndex == 0)
					{
						targetPoint = targetTable[0].value;
					}
					else
					{
						targetPoint = targetTable [targetTable.Count - 1].value;
					}

					if (mainIsX)
					{
						points.Add (new Vector2Int (srcPair.value, targetPoint));
					}
					else
					{
						points.Add (new Vector2Int (targetPoint, srcPair.value));
					}
				});

			return points;
		}

		List<int> GetAxisTrack (int begin, int end)
		{
			List<int> track = new List<int> ();

			if (begin != end) 
			{
				if (begin < end) 
				{
					for (int i = begin; i <= end; i++) 
					{
						track.Add (i);
					}
				}
				else
				{
					for (int i = begin; i >= end; i--) 
					{
						track.Add (i);
					}
				}
			}
			else
			{
				track.Add (begin);
			}


			return track;
		}

		void TryMaskPointGroup (Vector2Int center, out bool hasModify)
		{
			hasModify = false;

			var drawGroup = GetDrawGroup (center);

			foreach (var drawPoint in drawGroup) 
			{
				bool singleModify = false;

				TryMaskPoint (drawPoint, out singleModify);

				if (singleModify) 
				{
					hasModify = true;
				}
			}
		}

		void TryMaskPoint (Vector2Int point, out bool hasModify)
		{
			var index = GetIndex (point);

			TryMaskPointIndex (index, out hasModify);
		}

		int GetIndex (Vector2Int point)
		{
			//index 從0開始
			int index = width * point.y + (point.x);

			// 舉例來說長度5的時候應該是4
			// 8的時候是7 因為最後一次加的時候多加了進去
			if (point.y > 0) 
			{
				index -= 1;
			}

			return index;
		}


		/// <summary>
		/// 反查座標
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="index">Index.</param>
		public static Vector2Int GetPos (int index, int width)
		{
			var remaining = (index + 1) % width;

			var y = (index + 1) / width;

			//index從0開始所以-1
			if(remaining > 0)
			{
				//偏移到下一行
				return new Vector2Int (remaining -1 , y);
			}
			else
			{
				return new Vector2Int (width - 1, y - 1);
			}
		}

		void TryMaskPointIndex (int index, out bool hasModify)
		{
			var hashSetIndex = index / HashSetRange;
			
			var cacheSet = cacheSets [hashSetIndex];

			if (cacheSet.Add (index))
			{
				cachePixels [index] = drawColor;
				hasModify = true;
				modifyIndexs.Add (index);
			}
			else
			{
				hasModify = false;
			}
		}

		/// <summary>
		/// 做出筆刷效果
		/// 藉由平移本來的筆刷
		/// </summary>
		/// <returns>The draw group.</returns>
		List<Vector2Int> GetDrawGroup (Vector2Int center)
		{
			List<Vector2Int> drawGroup = new List<Vector2Int> ();

			pointGroup.ForEach (point=>
				{
					var transferPoint = point + center;

					if (transferPoint.x >= 0 && transferPoint.x <= (width - 1))
					{
						if(transferPoint.y >= 0 && transferPoint.y <= (height - 1))
						{
							drawGroup.Add (transferPoint);
						}
					}
				});

			return drawGroup;
		}

		void CreatePointGroup ()
		{
			pointGroup = new List<Vector2Int> ();

			int beginX = -Half;

			int endX = Half;

			int beginY = -Half;

			int endY = Half;

			for (int posX = beginX; posX <= endX; posX++) 
			{
				for (int posY = beginY; posY <= endY; posY++)
				{
					Vector2Int point = new Vector2Int (posX, posY);
					var dist = Vector2.Distance (point, Vector2.zero);

					if (dist < Half)
					{
						pointGroup.Add (point);
					}
				}	
			}
		}

		void CreateCacheSet () 
		{
			//預先把容量撐開, 不然每次底層改size都會耗時
			var sizeBuffer = Enumerable.Range (0, HashSetRange).ToList ();

			var hashSetCount = refreshPixels.Length / HashSetRange;

			//有餘數
			if (refreshPixels.Length % HashSetRange > 0)
			{
				hashSetCount++;
			}

			cacheSets = new List<HashSet<int>> ();

			for (int i = 0; i < hashSetCount; i++)
			{
				var cacheSet = new HashSet<int> (sizeBuffer.ToList ());
				cacheSet.Clear ();

				cacheSets.Add (cacheSet);
			}
		}
	}
}