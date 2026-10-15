using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Poyo.MarshmallowJar.Editor
{
    [InitializeOnLoad]
    internal static class SceneSetupWatcher
    {
        private const string StartupCheckedKey = "MarshmallowJar.SceneSetup.StartupChecked";
        private const string DeclinedKey = "MarshmallowJar.SceneSetup.Declined";
        private const string PendingKey = "MarshmallowJar.SceneSetup.Pending";
        private static bool _pendingCheck;

        internal static bool HasPending
        {
            get
            {
                return !string.IsNullOrEmpty(SessionState.GetString(PendingKey, string.Empty));
            }
        }

        static SceneSetupWatcher()
        {
            EditorSceneManager.newSceneCreated += OnNewSceneCreated;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            // NOTE: エディタ起動時に復元されるシーンでは sceneOpened が来ないため、セッションの最初に 1 回だけ確かめる。
            if (!SessionState.GetBool(StartupCheckedKey, false))
            {
                SessionState.SetBool(StartupCheckedKey, true);
                _pendingCheck = true;
                EditorApplication.delayCall += CheckActiveScene;
            }
        }

        private static void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup) || mode != NewSceneMode.Single ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            ClearPending();
            SceneSetupApplier.Apply(scene);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (mode != OpenSceneMode.Single || _pendingCheck)
            {
                return;
            }

            _pendingCheck = true;
            EditorApplication.delayCall += CheckActiveScene;
        }

        private static void CheckActiveScene()
        {
            _pendingCheck = false;
            Scene scene = SceneManager.GetActiveScene();
            string guid = scene.IsValid() && !string.IsNullOrEmpty(scene.path)
                ? AssetDatabase.AssetPathToGUID(scene.path)
                : string.Empty;
            string pending = SessionState.GetString(PendingKey, string.Empty);
            if (!string.IsNullOrEmpty(pending) && pending != guid)
            {
                ClearPending();
            }

            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup))
            {
                ClearPending();
            }

            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup) || Application.isBatchMode ||
                EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path) ||
                !scene.path.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return;
            }

            MarshmallowJarSettings settings = MarshmallowJarSettings.instance;
            string declined = SessionState.GetString(DeclinedKey, string.Empty);
            if (settings.GetIgnoredSceneGuids().Contains(guid) ||
                Array.IndexOf(declined.Split('|'), guid) >= 0 || SceneSetupRules.IsCompliant(scene))
            {
                return;
            }

            SessionState.SetString(PendingKey, guid);
            SceneSetupPromptWindow.Open(scene.name);
            Debug.Log(MarshmallowJarInfo.LogPrefix + "シーン「" + scene.name +
                "」のライティングが推奨設定ではありません。確認ウィンドウ、またはメニュー「Tools/Marshmallow Jar/Scene Lighting」から選んでください。");
        }

        internal static void ConvertActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup) || EditorApplication.isPlayingOrWillChangePlaymode ||
                !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            SceneSetupApplier.Apply(scene);
            Debug.Log(MarshmallowJarInfo.LogPrefix + "シーン「" + scene.name +
                "」を推奨ライティングに変換しました。保存すると確定します。");
            if (SessionState.GetString(PendingKey, string.Empty) == AssetDatabase.AssetPathToGUID(scene.path))
            {
                ClearPending();
            }
        }

        internal static void SkipPending()
        {
            string pending = SessionState.GetString(PendingKey, string.Empty);
            if (string.IsNullOrEmpty(pending))
            {
                return;
            }

            if (MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup))
            {
                string declined = SessionState.GetString(DeclinedKey, string.Empty);
                if (Array.IndexOf(declined.Split('|'), pending) < 0)
                {
                    SessionState.SetString(DeclinedKey,
                        string.IsNullOrEmpty(declined) ? pending : declined + "|" + pending);
                }
            }

            ClearPending();
        }

        internal static void IgnorePending()
        {
            string pending = SessionState.GetString(PendingKey, string.Empty);
            if (string.IsNullOrEmpty(pending))
            {
                return;
            }

            MarshmallowJarSettings settings = MarshmallowJarSettings.instance;
            if (MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup) && !settings.GetIgnoredSceneGuids().Contains(pending))
            {
                settings.GetIgnoredSceneGuids().Add(pending);
                settings.SaveChanges();
            }

            ClearPending();
        }

        internal static void ClearPending()
        {
            SessionState.EraseString(PendingKey);
            SceneSetupPromptWindow.CloseIfOpen();
        }
    }
}
