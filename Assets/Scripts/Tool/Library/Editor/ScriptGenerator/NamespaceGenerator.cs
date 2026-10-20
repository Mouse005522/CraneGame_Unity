using System.Collections.Generic;
using Kun.Tool;

public class NamespaceGenerator : ScriptGenerator
{
	List<string> usingNamespaces = new List<string>();
	string belongNamespace = "";

	public NamespaceGenerator(List<string> usingNamespaces, string belongNamespace, List<ScriptGenerator> nodes) : base(nodes)
	{
		this.usingNamespaces = usingNamespaces;
		this.belongNamespace = belongNamespace;
	}

	protected override void GeneratorContent()
	{
		//加上引用的空間
		usingNamespaces.ForEach(_namespace =>
		{
			string processUsingLine = GetMixNamespace(_namespace);
			ProcessAddLine(processUsingLine);
		});
		AddTempLine();
		string belongNamespaceStr = "namespace" + GetSpace() + belongNamespace;

		ProcessAddLine(belongNamespaceStr);
		ProcessAddLine("{");
	}


	string GetMixNamespace(string usingNamespace)
	{
		return string.Format("using{0}{1};", GetSpace(), usingNamespace);
	}
}
