using System.Collections.Generic;

namespace Kun.Tool
{
    public class ClassGeneratorData : GeneratorData
	{
		GeneratorClassSetting generatorClassSetting;

		public ClassGeneratorData (GeneratorClassSetting drawClassSetting, List<GeneratorData> nodes) : base (nodes)
		{
			this.generatorClassSetting = drawClassSetting;
		}

		protected override void GeneratorContent ()
		{
			string drawPartial = generatorClassSetting.isPartial ? " partial " : " ";

			string drawStatic = generatorClassSetting.isStaticClass ? " static " : " ";
			string drawClass = generatorClassSetting.className;

			drawClass = GetProhibitKeyReplace (drawClass);

            string drawInherit = generatorClassSetting.HasInherit ? (" : " + generatorClassSetting.InheritName) : "";

			string formatClassName = $"public{drawPartial}class {drawClass}{drawInherit}";

			if (generatorClassSetting.drawSerializableAttribute)
			{
				ProcessAddLine ("[System.Serializable]");
			}

			ProcessAddLine (formatClassName);
			ProcessAddLine ("{");
		}
	}

	[System.Serializable]
	public class GeneratorClassSetting
	{
		public bool isPartial = false;

		public bool drawSerializableAttribute;

		public string className;

		public bool isStaticClass;

		bool hasInherit = false;
		public bool HasInherit
		{
			get
			{
				return hasInherit;
			}
		}

		string inheritName;
		public string InheritName
		{
			get
			{
				return inheritName;
			}

			set
			{
				inheritName = value;

				hasInherit = true;
			}
		}

	}
}