using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kun.Tool
{
    public static class ComponentTool
	{
		public static T GetOrAddComponent<T> (this Component component) where T : Component
		{
			return component.gameObject.GetOrAddComponent<T> ();
		}

		public static T GetOrAddComponent<T> (this GameObject go) where T : Component
		{
			T component = go.GetComponent<T> ();

			if (component == null)
			{
				component = go.AddComponent<T> ();
			}

			return component;
		}

		/// <summary>
		/// 把特定的排除, 其他的留著
		/// Transform不用特別說因為也移除不掉
		/// </summary>
		/// <param name="go">Go.</param>
		/// <param name="types">Types.</param>
		public static void RemoveComsWithout (this GameObject go, params Type[] types)
		{
			var transformType = typeof(Transform);
			var rectTransformType = typeof(RectTransform);

			List<Type> processTypes = types.ToList ();

			var coms = go.GetComponents<Component> ().ToList ();

			coms.ForEach (com=>
				{
					var comType = com.GetType ();

					if(comType == transformType)
					{
						return;
					}

					if(comType == rectTransformType)
					{
						return;
					}

					if(processTypes.Exists (processType => processType == comType) == false)
					{
						MonoBehaviour.Destroy (com);
					}
				});
		}

		/// <summary>
		/// 把特定的留著, 其他的排除
		/// Transform不用特別說因為也移除不掉
		/// </summary>
		/// <param name="go">Go.</param>
		/// <param name="types">Types.</param>
		public static void RemoveComs (this GameObject go, params Type[] types)
		{
			var transformType = typeof(Transform);
			var rectTransformType = typeof(RectTransform);

			List<Type> processTypes = types.ToList ();

			if (processTypes.Contains (transformType)) 
			{
				processTypes.Remove (transformType);
				Debug.LogError ("Transform 無法被移除");
			}

			if (processTypes.Contains (rectTransformType)) 
			{
				processTypes.Remove (rectTransformType);
				Debug.LogError ("RectTransform 無法被移除");
			}

			//要符合移除順序, 特別是Canvas相關的東西
			processTypes.ForEach (type=>
				{
					var findCom = go.GetComponent (type);

					if (findCom != null)
					{
						MonoBehaviour.DestroyImmediate (findCom);
					}
				});
		}

		/// <summary>
		/// 設定為父物件並完整跟隨Pose
		/// </summary>
		public static void FetchTarget (this Transform selfTarget, Transform target)
		{
			selfTarget.SetParent (target);
			selfTarget.localPosition = Vector3.zero;
			selfTarget.localEulerAngles = Vector3.zero;
		}

		public static void FetchTarget (this RectTransform selfTransform, RectTransform source)
		{
			selfTransform.SetParent (source.parent);

			selfTransform.anchorMin = source.anchorMin;
			selfTransform.anchorMax = source.anchorMax;
			selfTransform.anchoredPosition = source.anchoredPosition;
			selfTransform.pivot = source.pivot;
		}

		/// <summary>
		/// 設定為父物件
		/// 並長在子物件Y為0的位置
		/// 旋轉只跟隨Y軸
		/// </summary>
		public static void FetchTargetFloor (this Transform selfTarget, Transform target)
		{
			selfTarget.SetParent (target);
			selfTarget.localPosition = Vector3.zero;
			selfTarget.localEulerAngles = Vector3.zero;
		}

		public static List<Material> GetOwnerMats (this GameObject go)
		{
			List<Material> mats = new List<Material> ();

			go.GetComponentsInChildren<MeshRenderer> ().ToList ().ForEach (render => mats.AddRange (render.materials));
			go.GetComponentsInChildren<SkinnedMeshRenderer> ().ToList ().ForEach (render => mats.AddRange (render.materials));

			return mats;
		}

		/// <summary>
		/// 要產出與本來長度等長的數量避免UV問題
		/// </summary>
		/// <param name="matSrc"></param>
		/// <returns></returns>
		public static List<Material> ReplaceMat (this MeshRenderer render, Material matSrc)
		{
			var oldLength = render.materials.Length;
			List<Material> mats = new List<Material> ();

			for (int i = 0; i < oldLength; i++)
			{
				var mat = MonoBehaviour.Instantiate (matSrc);
				mats.Add (mat);
			}

			render.materials = mats.ToArray ();

			return mats;
		}

		/// <summary>
		/// 要產出與本來長度等長的數量避免UV問題
		/// </summary>
		/// <param name="matSrc"></param>
		/// <returns></returns>
		public static List<Material> ReplaceMat (this SkinnedMeshRenderer render, Material matSrc)
		{
			var oldLength = render.materials.Length;
			List<Material> mats = new List<Material> ();

			for (int i = 0; i < oldLength; i++)
			{
				var mat = MonoBehaviour.Instantiate (matSrc);
				mats.Add (mat);
			}

			render.materials = mats.ToArray ();

			return mats;
		}

		public static bool CheckContains (this BoxCollider boxCollider, Vector3 point, bool ignoreY = false)
		{
			var localPoint = boxCollider.transform.InverseTransformPoint (point);

			var delta = localPoint - boxCollider.center;

			for (int i = 0; i < 3; i++)
			{
				if (ignoreY == false || i != 1)
				{
					if (CheckInHalf (delta[i], boxCollider.size[i]) == false)
					{
						return false;
					}
				}
			}

			return true;
		}

        public static bool CheckContains (Vector3 sensorCenter, Quaternion sensorRot, Vector3 sensorSize, Vector3 point, bool ignoreY = false)
        {
            var pos = point;

			pos -= sensorCenter;

            //把這個座標逆推回原來的方形內
            pos = Quaternion.Inverse (sensorRot) * pos;

            for (int i = 0; i < 3; i++)
            {
				//無視Y軸
				if (i != 1 || ignoreY == false)
                {
                    var contain = CheckInHalf (pos[i], sensorSize[i]);

                    if (contain == false)
                    {
						return false;
                    }
                }
            }

			return true;
        }

        static bool CheckInHalf (float checkValue, float checkSize)
        {
            var half = checkSize / 2;
            var abs = Mathf.Abs (checkValue);

            return abs <= half;
        }

        /// <summary>
        /// 取得包括頂點物件在內 所有子物件實體
        /// </summary>
        /// <returns>The gos.</returns>
        /// <param name="go">Go.</param>
        public static List<GameObject> GetGos (this GameObject go)
		{
			List<GameObject> gos = new List<GameObject> ();

			gos.Add (go);

			int childCount = go.transform.childCount;

			for (int i = 0; i < childCount; i++)
			{
				gos.AddRange (GetGos (go.transform.GetChild (i).gameObject));
			}

			return gos;
		}

		/// <summary>
		/// 重設動畫變數與State
		/// </summary>
		/// <param name="animator"></param>
		public static void ResetRuntime (this Animator animator) 
		{
			if (animator != null)
			{
				var controller = animator.runtimeAnimatorController;
				animator.runtimeAnimatorController = null;
				animator.runtimeAnimatorController = controller;
			}
			else
			{
				LoggerRouter.Error ($"animator為null");
			}
        }

#if UNITY_EDITOR
		public static void SetGameObjectAndChildrenDirty (GameObject gameObject)
        {
            // 標記當前 GameObject 為 Dirty
            EditorUtility.SetDirty (gameObject);

			if (gameObject.transform is RectTransform rect) 
			{
				EditorUtility.SetDirty (rect);
			}

            // 遞迴處理所有子物件
            foreach (Transform child in gameObject.transform)
            {
                SetGameObjectAndChildrenDirty (child.gameObject);
            }
        }
#endif
	}

	/// <summary>
	/// 可以取得Gameobject實體的
	/// </summary>
	public interface Componentable
	{
		GameObject GetEntity ();
	}
}