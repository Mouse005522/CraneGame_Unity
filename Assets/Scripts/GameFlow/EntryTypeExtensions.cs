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

        public static bool IsPcPlayerBuild (this EntryType entryType)
        {
            return entryType == EntryType.Host;
        }

        static bool IsStandaloneWindowsBuild (this EntryType entryType)
        {
            return entryType.IsServerBuild () || entryType.IsPcPlayerBuild ();
        }

        public static BuildTarget GetBuildTarget (this EntryType entryType)
        {
            return entryType.IsStandaloneWindowsBuild ()
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
            return entryType switch
            {
                EntryType.Server => "PC ServerBuild",
                EntryType.Host => "PC PlayerBuild",
                _ => "WebBuild",
            };
        }

        public static string GetBuildOutputName (this EntryType entryType, string productName)
        {
            return entryType switch
            {
                EntryType.Server => $"{productName}_Server_PC",
                EntryType.Host => $"{productName}_Host_PC",
                _ => $"{productName}_Client_WebGL",
            };
        }

        public static string GetBuildLocationPath (this EntryType entryType, string buildFolder, string outputName)
        {
            return entryType.IsStandaloneWindowsBuild ()
                ? Path.Combine (buildFolder, $"{outputName}.exe")
                : Path.Combine (buildFolder, outputName);
        }
    }
}
