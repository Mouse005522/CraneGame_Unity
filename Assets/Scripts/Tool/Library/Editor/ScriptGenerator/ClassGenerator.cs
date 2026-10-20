using System.Collections.Generic;
using Kun.Tool;

public class ClassGenerator : ScriptGenerator
{
	GeneratorClassSetting generatorClassSetting;

	public ClassGenerator(GeneratorClassSetting drawClassSetting, List<ScriptGenerator> nodes) : base(nodes)
	{
		this.generatorClassSetting = drawClassSetting;
	}

	protected override void GeneratorContent()
	{
		string drawPartial = generatorClassSetting.isPartial ? " partial " : " ";

		string drawStatic = generatorClassSetting.isStaticClass ? " static " : " ";
		string drawClass = generatorClassSetting.className;
		string drawInherit = generatorClassSetting.HasInherit ? (" : " + generatorClassSetting.InheritName) : "";

		string formatClassName = $"public{drawPartial}class {drawClass}{drawInherit}";

		if (generatorClassSetting.drawSerializableAttribute)
		{
			ProcessAddLine("[System.Serializable]");
		}

		ProcessAddLine(formatClassName);
		ProcessAddLine("{");
	}

}