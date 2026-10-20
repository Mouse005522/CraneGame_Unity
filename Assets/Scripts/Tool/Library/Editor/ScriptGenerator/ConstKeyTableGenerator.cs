using System.Collections.Generic;
using Kun.Tool;

public class ConstKeyTableGenerator : ScriptGenerator
{
	public ConstKeyTableGenerator(List<string> drawKeys, List<ScriptGenerator> nodes) : base(nodes)
	{
		this.drawKeys = drawKeys;

		prohibitKeys = new string[] { " ", "-", "(", ")" };
	}

	//禁止的變數名稱
	string[] prohibitKeys;

	protected override void GeneratorContent()
	{
		for (int i = 0; i < drawKeys.Count; i++)
		{
			string key = drawKeys[i];

			string processKey = key;

			for (int j = 0; j < prohibitKeys.Length; j++)
			{
				processKey = processKey.Replace(prohibitKeys[j], "_");
			}

			string keyLine = $"public const string {processKey} = \"{key}\";";

			ProcessAddLine(keyLine);

			if (i < drawKeys.Count - 1)
			{
				ProcessAddLine("");
			}
		}
	}

	protected override bool ExitHook
	{
		get
		{
			return false;
		}
	}

	List<string> drawKeys;
}
