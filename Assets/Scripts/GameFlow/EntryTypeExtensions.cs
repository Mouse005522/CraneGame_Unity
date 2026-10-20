using System.IO;
using UnityEditor;

namespace Kun.Tool
{
    public static class EntryTypeExtensions
    {
        public static bool IsServerBuild (this EntryType entryType)
        {
            return entryType == EntryType.Server;
        }

        public static BuildTarget GetBuildTarget (this EntryType entryType)
        {
            return entryType.IsServerBuild ()
                ? BuildTarget.StandaloneWindows64
                : BuildTarget.WebGL;
        }

        public static int GetBuildSubtarget (this EntryType entryType)
        {
            return entryType.IsServerBuild ()
                ? (int)StandaloneBuildSubtarget.Server
                : 0;
        }

        public static string GetBuildProfileName (this EntryType entryType)
        {
            return entryType.IsServerBuild ()
                ? "PC ServerBuild"
                : "WebBuild";
        }

        public static string GetBuildOutputName (this EntryType entryType, string productName)
        {
            return entryType.IsServerBuild ()
                ? $"{productName}_Server_PC"
                : $"{productName}_Client_WebGL";
        }

        public static string GetBuildLocationPath (this EntryType entryType, string buildFolder, string outputName)
        {
            return entryType.IsServerBuild ()
                ? Path.Combine (buildFolder, $"{outputName}.exe")
                : Path.Combine (buildFolder, outputName);
        }
    }
}
