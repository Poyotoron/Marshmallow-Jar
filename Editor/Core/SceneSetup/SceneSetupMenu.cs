using UnityEditor;
using UnityEngine.SceneManagement;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class SceneSetupMenu
    {
        private const string Root = "Tools/" + MarshmallowJarInfo.DisplayName + "/Scene Lighting/";

        [MenuItem(Root + "Convert Active Scene", false, 1100)]
        private static void ConvertActiveScene()
        {
            SceneSetupWatcher.ConvertActiveScene();
        }

        [MenuItem(Root + "Convert Active Scene", true)]
        private static bool ValidateConvertActiveScene()
        {
            return MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup) &&
                !EditorApplication.isPlayingOrWillChangePlaymode && SceneManager.GetActiveScene().IsValid();
        }

        [MenuItem(Root + "Skip This Time", false, 1101)]
        private static void SkipPending()
        {
            SceneSetupWatcher.SkipPending();
        }

        [MenuItem(Root + "Skip This Time", true)]
        private static bool ValidateSkipPending()
        {
            return SceneSetupWatcher.HasPending;
        }

        [MenuItem(Root + "Don't Ask for This Scene", false, 1102)]
        private static void IgnorePending()
        {
            SceneSetupWatcher.IgnorePending();
        }

        [MenuItem(Root + "Don't Ask for This Scene", true)]
        private static bool ValidateIgnorePending()
        {
            return SceneSetupWatcher.HasPending;
        }
    }
}
