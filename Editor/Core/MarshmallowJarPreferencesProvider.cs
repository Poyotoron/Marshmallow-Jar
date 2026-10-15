using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Poyo.MarshmallowJar.Editor.MarshmallowJarToggleGUI;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class MarshmallowJarPreferencesProvider
    {
        private static readonly GUIContent HeaderContent = new GUIContent("バージョン " + MarshmallowJarInfo.Version);
        private static readonly GUIContent DescriptionContent = new GUIContent("全プロジェクトに共通の設定です。プロジェクトごとに変えたいときは Project Settings > Marshmallow Jar で上書きします。");
        private static readonly GUIContent OpenProjectContent = new GUIContent("このプロジェクトの設定を開く");
        private static readonly GUIContent FeaturesContent = new GUIContent("エディタ機能");

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            return new SettingsProvider(MarshmallowJarInfo.PreferencesPath, SettingsScope.User)
            {
                label = MarshmallowJarInfo.DisplayName,
                guiHandler = OnGUI,
                activateHandler = OnActivate,
                keywords = new HashSet<string>(new[]
                {
                    "marshmallow", "hierarchy", "editoronly", "variant", "prefab", "lighting",
                    "scene", "vrchat", "thumbnail", "inspector", "irodori", "notification", "webhook", "discord", "sound"
                }),
            };
        }

        [MenuItem(MarshmallowJarInfo.PreferencesMenuPath, false, MarshmallowJarInfo.PreferencesMenuPriority)]
        private static void OpenPreferences()
        {
            SettingsService.OpenUserPreferences(MarshmallowJarInfo.PreferencesPath);
        }

        [MenuItem("Assets/" + MarshmallowJarInfo.DisplayName + "/Preferences…", false, 1131)]
        private static void OpenPreferencesFromAssets()
        {
            OpenPreferences();
        }

        [MenuItem("GameObject/" + MarshmallowJarInfo.DisplayName + "/Preferences…", false, 43)]
        private static void OpenPreferencesFromGameObject()
        {
            OpenPreferences();
        }

        private static void OnActivate(string searchContext, VisualElement rootElement)
        {
            MarshmallowJarGlobalSettings.Reload();
        }

        private static void OnGUI(string searchContext)
        {
            EnsureStyles();
            EditorGUILayout.LabelField(HeaderContent, EditorStyles.miniLabel);
            EditorGUILayout.LabelField(DescriptionContent, DescriptionStyle);
            if (GUILayout.Button(OpenProjectContent))
            {
                SettingsService.OpenProjectSettings(MarshmallowJarInfo.SettingsPath);
            }
            if (HasAnyOverride())
            {
                EditorGUILayout.HelpBox(GlobalOverrideNotice, MessageType.Info);
            }
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(FeaturesContent, EditorStyles.boldLabel);
            DrawGlobalFeature(MarshmallowJarToggle.HierarchyToggles, HierarchyTitle, HierarchyDescription, true);
            DrawGlobalFeature(MarshmallowJarToggle.StripVariantSuffix, VariantTitle, VariantDescription, true);
            DrawGlobalFeature(MarshmallowJarToggle.SceneSetup, SceneTitle, SceneDescription, true);
            DrawGlobalFeature(MarshmallowJarToggle.VrcSdkAutoFill, SdkTitle,
                HasVrcSdk ? SdkDescription : SdkUnavailableDescription, HasVrcSdk);
            DrawGlobalFeature(MarshmallowJarToggle.InspectorNewWindowButton, InspectorTitle, InspectorDescription, true);
            DrawGlobalFeature(MarshmallowJarToggle.InspectorCopyToButton, CopyToTitle, CopyToDescription, true);
            DrawGlobalFeature(MarshmallowJarToggle.IrodoriColorizer, ColorizerToggleTitle, ColorizerDescription,
                !StandaloneColorizerNotice.IsStandaloneInstalled);
            NotificationSettingsGUI.DrawGlobal();
        }
    }
}
