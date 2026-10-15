using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static Poyo.MarshmallowJar.Editor.MarshmallowJarToggleGUI;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class MarshmallowJarSettingsProvider
    {
        private const string ColorizerSettingsPath = "Project/Marshmallow Jar/Irodori Colorizer";

        private static readonly GUIContent HeaderContent =
            new GUIContent("バージョン " + MarshmallowJarInfo.Version);
        private static readonly GUIContent FeaturesContent = new GUIContent("エディタ機能");
        private static readonly GUIContent HierarchyOffsetContent = new GUIContent(
            "右端からの距離",
            "行の右端からチェックボックスまでの距離（px）。ほかの拡張のボタンと重なるときは大きくします。");
        private static readonly GUIContent ApplySceneContent = new GUIContent("今のシーンに適用");
        private static readonly GUIContent ResetIgnoredScenesContent = new GUIContent("確認しないシーンをリセット");
        private static readonly GUIContent BlendShapeTitle = new GUIContent("ブレンドシェイプのスライダー");
        private static readonly GUIContent BlendShapeDescription = new GUIContent(
            "SkinnedMeshRenderer のブレンドシェイプを、下限と上限を決めたスライダーで動かすウィンドウです。範囲の外の値は入力できません。");
        private static readonly GUIContent OpenBlendShapeContent = new GUIContent("ウィンドウを開く");
        private static readonly GUIContent ColorizerTitle = new GUIContent("Irodori Colorizer");
        private static readonly GUIContent ColorizerSettingsContent = new GUIContent("Irodori Colorizer の設定を開く");

        private static readonly GUIContent GlobalDescription = new GUIContent("ON / OFF は全体設定（Preferences > Marshmallow Jar）に従います。ここで切り替えると、このプロジェクトだけ上書きします。");
        private static readonly GUIContent OpenGlobalContent = new GUIContent("全体設定を開く");

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            return new SettingsProvider(MarshmallowJarInfo.SettingsPath, SettingsScope.Project)
            {
                label = MarshmallowJarInfo.DisplayName,
                guiHandler = OnGUI,
                activateHandler = OnActivate,
                keywords = new HashSet<string>(new[]
                {
                    "marshmallow", "hierarchy", "editoronly", "variant", "prefab",
                    "lighting", "scene", "vrchat", "thumbnail", "inspector", "blendshape", "irodori", "notification", "webhook", "discord", "sound"
                }),
            };
        }

        [MenuItem(MarshmallowJarInfo.MenuPath, false, MarshmallowJarInfo.MenuPriority)]
        private static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(MarshmallowJarInfo.SettingsPath);
        }

        // NOTE: 色付けの項目と離して区切り線を入れる。GameObject 側は 49 以下で Hierarchy の右クリックにも表示される。
        [MenuItem("Assets/" + MarshmallowJarInfo.DisplayName + "/Project Settings…", false, 1130)]
        private static void OpenSettingsFromAssets()
        {
            OpenSettings();
        }

        [MenuItem("GameObject/" + MarshmallowJarInfo.DisplayName + "/Project Settings…", false, 42)]
        private static void OpenSettingsFromGameObject()
        {
            OpenSettings();
        }

        private static void OnActivate(string searchContext, VisualElement rootElement)
        {
            MarshmallowJarGlobalSettings.Reload();
        }

        private static void OnGUI(string searchContext)
        {
            EnsureStyles();
            EditorGUILayout.LabelField(HeaderContent, EditorStyles.miniLabel);
            EditorGUILayout.LabelField(GlobalDescription, DescriptionStyle);
            if (GUILayout.Button(OpenGlobalContent))
            {
                SettingsService.OpenUserPreferences(MarshmallowJarInfo.PreferencesPath);
            }
            if (HasAnyOverride())
            {
                EditorGUILayout.HelpBox(ProjectOverrideNotice, MessageType.Info);
            }
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(FeaturesContent, EditorStyles.boldLabel);

            MarshmallowJarSettings settings = MarshmallowJarSettings.instance;
            DrawProjectFeature(MarshmallowJarToggle.HierarchyToggles, HierarchyTitle, HierarchyDescription, true);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.HierarchyToggles));
            EditorGUI.indentLevel++;
            float offset = EditorGUILayout.Slider(HierarchyOffsetContent, settings.hierarchyToggleRightOffset,
                0f, MarshmallowJarSettings.MaxHierarchyToggleRightOffset);
            settings.hierarchyToggleRightOffset = Mathf.Round(offset);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(4f);
            if (EditorGUI.EndChangeCheck())
            {
                settings.SaveChanges();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            DrawProjectFeature(MarshmallowJarToggle.StripVariantSuffix, VariantTitle, VariantDescription, true);
            DrawProjectFeature(MarshmallowJarToggle.SceneSetup, SceneTitle, SceneDescription, true);
            DrawSceneButtons(settings);
            DrawProjectFeature(MarshmallowJarToggle.VrcSdkAutoFill, SdkTitle,
                HasVrcSdk ? SdkDescription : SdkUnavailableDescription, HasVrcSdk);
            DrawProjectFeature(MarshmallowJarToggle.InspectorNewWindowButton, InspectorTitle, InspectorDescription, true);
            DrawProjectFeature(MarshmallowJarToggle.InspectorCopyToButton, CopyToTitle, CopyToDescription, true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(BlendShapeTitle);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(BlendShapeDescription, DescriptionStyle);
            BlendShapeSliderWindow.DrawRangeFields(settings);
            EditorGUI.indentLevel--;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            if (GUILayout.Button(OpenBlendShapeContent))
            {
                BlendShapeSliderWindow.Open(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(ColorizerTitle, EditorStyles.boldLabel);
            bool installed = StandaloneColorizerNotice.IsStandaloneInstalled;
            DrawProjectFeature(MarshmallowJarToggle.IrodoriColorizer, ColorizerToggleTitle, ColorizerDescription, !installed);
            if (installed)
            {
                EditorGUILayout.HelpBox(StandaloneColorizerNotice.WindowWarning, MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button(ColorizerSettingsContent))
                {
                    SettingsService.OpenProjectSettings(ColorizerSettingsPath);
                }
            }
            NotificationSettingsGUI.DrawProject();

        }

        private static void DrawSceneButtons(MarshmallowJarSettings settings)
        {
            EditorGUI.BeginDisabledGroup(!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup) || EditorApplication.isPlayingOrWillChangePlaymode);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            if (GUILayout.Button(ApplySceneContent))
            {
                SceneSetupWatcher.ConvertActiveScene();
            }

            EditorGUI.BeginDisabledGroup(settings.GetIgnoredSceneGuids().Count == 0);
            if (GUILayout.Button(ResetIgnoredScenesContent))
            {
                settings.GetIgnoredSceneGuids().Clear();
                settings.SaveChanges();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(4f);
        }
    }
}
