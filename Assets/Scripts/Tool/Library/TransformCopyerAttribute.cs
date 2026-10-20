using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kun.Tool
{
    public class TransformCopyerAttribute : PropertyAttribute
	{
		public TransformCopyerAttribute (TramsformCopyerType copyType)
		{
			this.copyType = copyType;
		}
		
		public TramsformCopyerType copyType;

	}

	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(TransformCopyerAttribute))]
	public class TransformCopyerAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight (property, label) + lineHeight + LineSpace + BtnHeight;
		}

		const float LineSpace = 10f;

		const float lineHeight = 16f;

		const float BtnWidth = 60f;

		const float BtnHeight = 20;

		TransformCopyerAttribute myAttribute
		{
			get
			{
				return (TransformCopyerAttribute)attribute;
			}
		}

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			EditorTool.DrawInProperty (position, property, label, ()=>
				{
					property.vector3Value = EditorGUI.Vector3Field (position, label, property.vector3Value);

					Rect btnRect = position;

					Vector3 btnPos = position.position;

					//靠右對齊
					btnPos.x += position.width - BtnWidth;
					btnPos.y += lineHeight + LineSpace;

					btnRect.position = btnPos;
					btnRect.width = BtnWidth;
					btnRect.height = BtnHeight;

					bool clickPaste =  GUI.Button (btnRect, "Paste");

					if(clickPaste)
					{
						property.vector3Value = myAttribute.copyType == TramsformCopyerType.CopyPos ? TransformCopier.position : TransformCopier.rotation.eulerAngles;
						EditorUtility.SetDirty (property.serializedObject.targetObject);
					}
				});
		}

	}
	#endif

	public enum TramsformCopyerType
	{
		CopyPos,
		CopyRot
	}
}
