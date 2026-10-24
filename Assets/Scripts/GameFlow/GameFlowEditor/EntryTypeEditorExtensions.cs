using UnityEditor;

namespace Kun.Tool
{
    public static class EntryTypeEditorExtensions
    {
        public static BuildTarget GetBuildTarget (this EntryType entryType)
        {
            bool isStandaloneWindows = entryType.IsServerBuild () || entryType.IsPcPlayerBuild ();
            return isStandaloneWindows
                ? BuildTarget.StandaloneWindows64
                : BuildTarget.WebGL;
        }

        public static int GetBuildSubtarget (this EntryType entryType)
        {
            return entryType.IsServerBuild ()
                ? (int)StandaloneBuildSubtarget.Server
                : 0;
        }
    }
}
