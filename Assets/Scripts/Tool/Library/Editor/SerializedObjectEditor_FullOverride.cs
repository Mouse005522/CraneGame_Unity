using UnityEngine;
using UnityEditor;

namespace Kun.Tool
{
    public class SerializedObjectEditor_FullOverride<T> : SerializedObjectEditor<T>  where T:MonoBehaviour
	{
		const float beginIntervalSpace = 10f;
		const float scriptFieldKeyWidth = 50f;
		protected const float classIntervalSpace = 5f;

		public sealed override void OnInspectorGUI ()
		{
			GUILayout.Space (beginIntervalSpace);

			DrawScriptField ();

			GUILayout.Space (classIntervalSpace);

			serializedObject.Update ();

			DrawInspectorGUI ();

			serializedObject.ApplyModifiedProperties ();
        }

		protected virtual void DrawInspectorGUI () 
		{

		}

		void DrawScriptField()
		{
			DrawVariableField ("Script : ", () => {
					
				MonoScript monoScript = MonoScript.FromMonoBehaviour (runtimeScript);
				EditorTool.DrawInReadOnly(()=>
					{
						EditorGUILayout.ObjectField (monoScript, typeof(MonoScript), false);
					});
			}, scriptFieldKeyWidth);
			GUI.enabled = true;
		}
	}	
}