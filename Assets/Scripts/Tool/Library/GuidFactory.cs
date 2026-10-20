using System;
using System.Collections.Generic;

namespace Kun.Tool
{
    public static class GuidFactory
	{
		public static string CreateGUID (List<string> usedGUIDs, string prefix)
		{
			string gValue;

			while (true)
			{
				var guid = GetGUID ();
				gValue = $"{prefix}    {guid}";

				if (usedGUIDs.Contains (gValue) == false)
				{
					break;
				}
			}

			return gValue;
		}

		public static string CreateGUID (List<string> usedGUIDs)
		{	
			string gValue;

			while(true)
			{
				gValue = GetGUID ();

				if (usedGUIDs.Contains (gValue) == false)
				{
					break;
				}
			}

			return gValue;
		}

		/// <summary>
		/// 為了省效能 只取前30碼 所以可能會重覆 重覆了就再生
		/// </summary>
		/// <returns>The GUI.</returns>
		public static string GetGUID ()
		{
			Guid g = Guid.NewGuid ();
			string gValue = g.ToString ().Substring (0, 30);
			return gValue;
		}
	}
}
