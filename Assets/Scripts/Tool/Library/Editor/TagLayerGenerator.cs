using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Kun.Tool
{
	public class TagLayerGenerator : EditorWindow
	{
		[MenuItem ("Editor/Tags & Layers")]
		public static void Update_Tags_Layers ()
		{
			CreateConstSctipt ("Tags", UnityEditorInternal.InternalEditorUtility.tags.ToList ());

			CreateConstSctipt ("Layers", UnityEditorInternal.InternalEditorUtility.layers.ToList ());

			AssetDatabase.Refresh ();
		}

		const string NamespaceName = "Kun.Tool";

		static void CreateConstSctipt (string className, List<string> keys)
		{
			GeneratorClassSetting classSetting = new GeneratorClassSetting ();

			classSetting.drawSerializableAttribute = false;
			classSetting.isStaticClass = true;
			classSetting.className = className;

			TableKeyGeneratorData<string> tableKeyData = new TableKeyGeneratorData_String (keys, null);

			ClassGeneratorData classData = new ClassGeneratorData (classSetting, new List<GeneratorData> () { tableKeyData });

			NamespaceGeneratorData namespaceData = new NamespaceGeneratorData (new List<string> (), NamespaceName, new List<GeneratorData> () { classData });

			var lines = namespaceData.GetGeneratorLines ();


			string path = $"{Application.dataPath}/Scripts/ConstKeys/{className}.cs";
			//path.Replace (@"/", @"\");

			if (File.Exists (path) == false) 
			{
				var file = File.Create (path);
				file.Close ();
			}

			using (StreamWriter writer = new StreamWriter (path, false))
			{
				lines.ForEach (line => 
					{
						writer.WriteLine (line);
					});
			}
		}
	}
}
