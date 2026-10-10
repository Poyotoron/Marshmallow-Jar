using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class MarshmallowJarSettingsProvider
    {
        private const string ColorizerSettingsPath = "Project/Marshmallow Jar/Irodori Colorizer";
#if MARSHMALLOW_JAR_HAS_VRCSDK
        private const bool HasVrcSdk = true;
#else
        private const bool HasVrcSdk = false;
#endif

        private static readonly GUIContent HeaderContent =
            new GUIContent("バージョン " + MarshmallowJarInfo.Version);
        private static readonly GUIContent FeaturesContent = new GUIContent("エディタ機能");
        private static readonly GUIContent HierarchyTitle =
            new GUIContent("Hierarchy に表示 / EditorOnly のチェックボックスを出す");
        private static readonly GUIContent HierarchyDescription =
            new GUIContent("行の右端で、オブジェクトの表示（Active）と EditorOnly タグを切り替えられます。");
        private static readonly GUIContent HierarchyOffsetContent = new GUIContent(
            "右端からの距離",
            "行の右端からチェックボックスまでの距離（px）。ほかの拡張のボタンと重なるときは大きくします。");
        private static readonly GUIContent VariantTitle =
            new GUIContent("Prefab Variant の名前に「Variant」を付けない");
        private static readonly GUIContent VariantDescription =
            new GUIContent("Prefab Variant を作ったとき、元と同じ名前で保存します。同じフォルダに同じ名前の Prefab があるときは「Variant」付きのままにします。");
        private static readonly GUIContent SceneTitle = new GUIContent("シーンのライティングとカメラを整える");
        private static readonly GUIContent SceneDescription =
            new GUIContent("新しいシーンを、アバターの色味を確かめやすいライティングで作ります。既存のシーンは、開いたときに確認してから変換します。");
        private static readonly GUIContent ApplySceneContent = new GUIContent("今のシーンに適用");
        private static readonly GUIContent ResetIgnoredScenesContent = new GUIContent("確認しないシーンをリセット");
        private static readonly GUIContent SdkTitle = new GUIContent("VRChat SDK に名前とサムネイルを自動入力する");
        private static readonly GUIContent SdkDescription =
            new GUIContent("初めてアップロードするアバターを SDK で選んだとき、名前とサムネイルが空なら、アバターの名前と Scene ビューの画像を入力します。");
        private static readonly GUIContent SdkUnavailableDescription =
            new GUIContent("VRChat SDK（Avatars）3.10.0 以上が導入されているときに使えます。");
        private static readonly GUIContent InspectorTitle =
            new GUIContent("Inspector に「別ウィンドウで開く」ボタンを出す");
        private static readonly GUIContent InspectorDescription =
            new GUIContent("選んでいるオブジェクトを固定した Inspector を別ウィンドウで開きます。ほかを選んでも表示は切り替わりません。");
        private static readonly GUIContent BlendShapeTitle = new GUIContent("ブレンドシェイプのスライダー");
        private static readonly GUIContent BlendShapeDescription = new GUIContent(
            "SkinnedMeshRenderer のブレンドシェイプを、下限と上限を決めたスライダーで動かすウィンドウです。範囲の外の値は入力できません。");
        private static readonly GUIContent OpenBlendShapeContent = new GUIContent("ウィンドウを開く");
        private static readonly GUIContent ColorizerTitle = new GUIContent("Irodori Colorizer");
        private static readonly GUIContent ColorizerDescription =
            new GUIContent("Project / Hierarchy の行に色付きラベルを付けます。右クリックメニューの「Marshmallow Jar」から使えます。");
        private static readonly GUIContent ColorizerSettingsContent = new GUIContent("Irodori Colorizer の設定を開く");

        private static GUIStyle _descriptionStyle;

        [SettingsProvider]
        private static SettingsProvider Create()
        {
            return new SettingsProvider(MarshmallowJarInfo.SettingsPath, SettingsScope.Project)
            {
                label = MarshmallowJarInfo.DisplayName,
                guiHandler = OnGUI,
                keywords = new HashSet<string>(new[]
                {
                    "marshmallow", "hierarchy", "editoronly", "variant", "prefab",
                    "lighting", "scene", "vrchat", "thumbnail", "inspector", "blendshape"
                }),
            };
        }

        [MenuItem(MarshmallowJarInfo.MenuPath, false, MarshmallowJarInfo.MenuPriority)]
        private static void OpenSettings()
        {
            SettingsService.OpenProjectSettings(MarshmallowJarInfo.SettingsPath);
        }

        // NOTE: 色付けの項目と離して区切り線を入れる。GameObject 側は 49 以下で Hierarchy の右クリックにも表示される。
        [MenuItem("Assets/" + MarshmallowJarInfo.DisplayName + "/Settings…", false, 1130)]
        private static void OpenSettingsFromAssets()
        {
            OpenSettings();
        }

        [MenuItem("GameObject/" + MarshmallowJarInfo.DisplayName + "/Settings…", false, 42)]
        private static void OpenSettingsFromGameObject()
        {
            OpenSettings();
        }

        private static void OnGUI(string searchContext)
        {
            if (_descriptionStyle == null)
            {
                _descriptionStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.LabelField(HeaderContent, EditorStyles.miniLabel);
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(FeaturesContent, EditorStyles.boldLabel);

            MarshmallowJarSettings settings = MarshmallowJarSettings.instance;
            EditorGUI.BeginChangeCheck();
            settings.hierarchyToggles = DrawFeature(HierarchyTitle, HierarchyDescription, settings.hierarchyToggles, true);
            EditorGUI.BeginDisabledGroup(!settings.hierarchyToggles);
            EditorGUI.indentLevel++;
            float offset = EditorGUILayout.Slider(HierarchyOffsetContent, settings.hierarchyToggleRightOffset,
                0f, MarshmallowJarSettings.MaxHierarchyToggleRightOffset);
            settings.hierarchyToggleRightOffset = Mathf.Round(offset);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(4f);
            settings.stripVariantSuffix = DrawFeature(VariantTitle, VariantDescription, settings.stripVariantSuffix, true);
            settings.sceneSetup = DrawFeature(SceneTitle, SceneDescription, settings.sceneSetup, true);
            DrawSceneButtons(settings);
            settings.vrcSdkAutoFill = DrawFeature(SdkTitle, HasVrcSdk ? SdkDescription : SdkUnavailableDescription,
                settings.vrcSdkAutoFill, HasVrcSdk);
            settings.inspectorNewWindowButton = DrawFeature(InspectorTitle, InspectorDescription,
                settings.inspectorNewWindowButton, true);
            if (EditorGUI.EndChangeCheck())
            {
                settings.SaveChanges();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(BlendShapeTitle);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(BlendShapeDescription, _descriptionStyle);
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
            if (installed)
            {
                EditorGUILayout.HelpBox(StandaloneColorizerNotice.WindowWarning, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField(ColorizerDescription, _descriptionStyle);
                if (GUILayout.Button(ColorizerSettingsContent))
                {
                    SettingsService.OpenProjectSettings(ColorizerSettingsPath);
                }
            }

        }

        private static bool DrawFeature(GUIContent title, GUIContent description, bool value, bool enabled)
        {
            EditorGUI.BeginDisabledGroup(!enabled);
            bool result = EditorGUILayout.ToggleLeft(title, value);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(description, _descriptionStyle);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(4f);
            return result;
        }

        private static void DrawSceneButtons(MarshmallowJarSettings settings)
        {
            EditorGUI.BeginDisabledGroup(!settings.sceneSetup || EditorApplication.isPlayingOrWillChangePlaymode);
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
