using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Kun.Tool
{
    public static class GenericTool
	{
		public static T DeepClone<T>(this T source)
		{
			MemoryStream memoryStream = new MemoryStream ();
			BinaryFormatter binaryFormatter = new BinaryFormatter ();

			binaryFormatter.Serialize (memoryStream, source);
			memoryStream.Flush ();
			memoryStream.Position = 0;
			T result = (T)binaryFormatter.Deserialize (memoryStream);
			memoryStream.Dispose ();
			return result;
		}

		public static void ProcessLayerMask (ref LayerMask layerMask, string modifyLayerName, bool isAdd)
		{
			int layerMaskValue = layerMask.value;

			ProcessLayerMask (ref layerMaskValue, modifyLayerName, isAdd);

			layerMask.value = layerMaskValue;
		}

		public static void ProcessLayerMask (Camera camera, string modifyLayerName, bool isAdd)
		{
			int layerMaskValue = camera.cullingMask;

			ProcessLayerMask (ref layerMaskValue, modifyLayerName, isAdd);

			camera.cullingMask = layerMaskValue;
		}

		public static void ProcessLayerMask (ref int layerMask, string modifyLayerName, bool isAdd)
		{
			int modifyLayer = LayerMask.NameToLayer (modifyLayerName);

			if (modifyLayer == -1) 
			{
				//everyThing的layer是-1 同時 NameToLayer 找不到東西也會回傳-1
				UnityEngine.Debug.LogError ("this layer not exist -> " + modifyLayerName);
			}
			else
			{
				int modifyLayerValue = 1 << modifyLayer;

				//確定指定的layer有沒有被包含在layerMask裡
				bool isContain = (layerMask & modifyLayerValue) == modifyLayerValue;

				// 新增 且 不包含			//移除 且 包含	
				if ((isAdd && (isContain == false))||((isAdd == false) && isContain))
				{
					//反轉指定開關
					layerMask = layerMask ^ modifyLayerValue;
				}
			}
		}

		/// <summary>
		/// 預設子物件的localPosition 為 Vector3.Zero
		/// </summary>
		/// <returns>The child.</returns>
		/// <param name="parent">Parent.</param>
		/// <param name="childName">Child name.</param>
		/// <param name="setLocalZero">If set to <c>true</c> set local zero.</param>
		public static GameObject InstantiateChild(Transform parent, string childName, bool setLocalZero = true)
		{
			GameObject childGO = new GameObject(childName);
			Transform childTransform = childGO.transform;

			childTransform.SetParent(parent);

			if (setLocalZero)
			{
				childTransform.localPosition = Vector3.zero;
				childTransform.localRotation = Quaternion.identity;
			}

			return childGO;
		}

		public static bool IsFileLocked (string path)
		{
			try
			{
				using (FileStream stream = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					
				}
			}
			catch (IOException)
			{
				return true;
			}

			return false;
		}
	}
}