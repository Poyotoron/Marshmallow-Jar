using System;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class MarshmallowJarToggleGUI
    {
        internal enum OverrideState
        {
            None = 0,
            Same = 1,
            Different = 2,
        }

#if MARSHMALLOW_JAR_HAS_VRCSDK
        internal const bool HasVrcSdk = true;
#else
        internal const bool HasVrcSdk = false;
#endif

        internal static readonly GUIContent HierarchyTitle =
            new GUIContent("Hierarchy に表示 / EditorOnly のチェックボックスを出す");
        internal static readonly GUIContent HierarchyDescription =
            new GUIContent("行の右端で、オブジェクトの表示（Active）と EditorOnly タグを切り替えられます。");
        internal static readonly GUIContent VariantTitle =
            new GUIContent("Prefab Variant の名前に「Variant」を付けない");
        internal static readonly GUIContent VariantDescription =
            new GUIContent("Prefab Variant を作ったとき、元と同じ名前で保存します。同じフォルダに同じ名前の Prefab があるときは「Variant」付きのままにします。");
        internal static readonly GUIContent SceneTitle = new GUIContent("シーンのライティングとカメラを整える");
        internal static readonly GUIContent SceneDescription =
            new GUIContent("新しいシーンを、アバターの色味を確かめやすいライティングで作ります。既存のシーンは、開いたときに確認してから変換します。");
        internal static readonly GUIContent SdkTitle = new GUIContent("VRChat SDK に名前とサムネイルを自動入力する");
        internal static readonly GUIContent SdkDescription =
            new GUIContent("初めてアップロードするアバターを SDK で選んだとき、名前とサムネイルが空なら、アバターの名前と Scene ビューの画像を入力します。");
        internal static readonly GUIContent SdkUnavailableDescription =
            new GUIContent("VRChat SDK（Avatars）3.10.0 以上が導入されているときに使えます。");
        internal static readonly GUIContent InspectorTitle =
            new GUIContent("Inspector に「別ウィンドウで開く」ボタンを出す");
        internal static readonly GUIContent InspectorDescription =
            new GUIContent("選んでいるオブジェクトを固定した Inspector を別ウィンドウで開きます。ほかを選んでも表示は切り替わりません。");
        internal static readonly GUIContent ColorizerDescription =
            new GUIContent("Project / Hierarchy の行に色付きラベルを付けます。右クリックメニューの「Marshmallow Jar」から使えます。");
        internal static readonly GUIContent CopyToTitle = new GUIContent("Inspector に「コピーを保存…」ボタンを出す");
        internal static readonly GUIContent CopyToDescription = new GUIContent(
            "アセットの Inspector に、保存先と名前を選んでコピーを作るボタンを出します。マテリアルなどを別のフォルダに複製するときに使います。");
        internal static readonly GUIContent ColorizerToggleTitle = new GUIContent("Irodori Colorizer を使う");
        internal static readonly GUIContent FollowGlobalContent = new GUIContent("全体設定のまま");
        internal static readonly GUIContent RevertToOnContent = new GUIContent("全体設定（ON）に戻す");
        internal static readonly GUIContent RevertToOffContent = new GUIContent("全体設定（OFF）に戻す");
        internal static readonly GUIContent RevertContent = new GUIContent("全体設定に戻す");
        internal static readonly Color OverrideBarColor = new Color32(1, 153, 235, 255);
        internal static readonly Color OverrideFillColor = new Color(0.004f, 0.6f, 0.92f, 0.12f);
        internal static readonly Color MismatchBarColor = new Color32(255, 152, 0, 255);
        internal static readonly Color MismatchFillColor = new Color(1f, 0.6f, 0f, 0.12f);
        internal static readonly GUIContent ProjectOverriddenContent =
            new GUIContent("このプロジェクトで上書き中", "Project Settings で、このプロジェクトだけ上書きされています（値は全体設定と同じ）。");
        internal static readonly GUIContent ProjectOnContent =
            new GUIContent("このプロジェクトでは ON", "Project Settings で、このプロジェクトだけ上書きされています。");
        internal static readonly GUIContent ProjectOffContent =
            new GUIContent("このプロジェクトでは OFF", "Project Settings で、このプロジェクトだけ上書きされています。");
        internal static readonly GUIContent ProjectDiffersContent =
            new GUIContent("このプロジェクトでは別の値", "Project Settings で、このプロジェクトだけ上書きされています。");
        internal const string GlobalOverrideNotice =
            "青い行は、このプロジェクト（Project Settings）で上書きしている項目です。そのうちオレンジの行は、全体設定と値が違います。";
        internal const string ProjectOverrideNotice =
            "青い行は、このプロジェクトで上書きしている項目です。そのうちオレンジの行は、全体設定（Preferences）と値が違います。";
        internal const float StateWidth = 140f;
        internal static GUIStyle DescriptionStyle { get; private set; }
        private static GUIStyle StateLabelStyle { get; set; }
        private static GUIStyle OverrideLabelStyle { get; set; }
        private static GUIStyle MismatchLabelStyle { get; set; }

        internal static void EnsureStyles()
        {
            if (DescriptionStyle != null)
            {
                return;
            }
            DescriptionStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            StateLabelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
            StateLabelStyle.normal.textColor = Color.gray;
            OverrideLabelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
            OverrideLabelStyle.normal.textColor = OverrideBarColor;
            MismatchLabelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };
            MismatchLabelStyle.normal.textColor = MismatchBarColor;
        }

        internal static void DrawGlobalFeature(MarshmallowJarToggle toggle, GUIContent title, GUIContent description, bool enabled)
        {
            EnsureStyles();
            Rect row = EditorGUILayout.GetControlRect();
            Rect stateRect = new Rect(row.xMax - StateWidth, row.y, StateWidth, row.height);
            Rect toggleRect = new Rect(row.x, row.y, row.width - StateWidth - 4f, row.height);
            OverrideState state = GetOverrideState(toggle);
            DrawOverrideHighlight(row, state);
            EditorGUI.BeginDisabledGroup(!enabled);
            bool current = MarshmallowJarToggles.GetGlobal(toggle);
            bool value = EditorGUI.ToggleLeft(toggleRect, title, current, EditorStyles.label);
            if (value != current)
            {
                MarshmallowJarToggles.SetGlobal(toggle, value);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            DrawProjectStateLabel(stateRect, state, MarshmallowJarToggles.IsOn(toggle) ? ProjectOnContent : ProjectOffContent);
            DrawDescription(description);
            EditorGUI.EndDisabledGroup();
        }

        internal static void DrawProjectFeature(MarshmallowJarToggle toggle, GUIContent title, GUIContent description, bool enabled)
        {
            EnsureStyles();
            Rect row = EditorGUILayout.GetControlRect();
            Rect stateRect = new Rect(row.xMax - StateWidth, row.y, StateWidth, row.height);
            Rect toggleRect = new Rect(row.x, row.y, row.width - StateWidth - 4f, row.height);
            OverrideState state = GetOverrideState(toggle);
            bool overridden = state != OverrideState.None;
            DrawOverrideHighlight(row, state);
            EditorGUI.BeginDisabledGroup(!enabled);
            bool current = MarshmallowJarToggles.IsOn(toggle);
            bool value = EditorGUI.ToggleLeft(toggleRect, title, current,
                overridden ? EditorStyles.boldLabel : EditorStyles.label);
            if (value != current)
            {
                MarshmallowJarToggles.SetOverride(toggle, value);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            GUIContent revert = MarshmallowJarToggles.GetGlobal(toggle) ? RevertToOnContent : RevertToOffContent;
            if (DrawStateCell(stateRect, overridden, revert))
            {
                MarshmallowJarToggles.ClearOverride(toggle);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            DrawDescription(description);
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawDescription(GUIContent description)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(description, DescriptionStyle);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4f);
        }

        internal static OverrideState GetOverrideState(MarshmallowJarToggle toggle)
        {
            if (!MarshmallowJarToggles.HasOverride(toggle))
            {
                return OverrideState.None;
            }
            return MarshmallowJarToggles.IsOn(toggle) == MarshmallowJarToggles.GetGlobal(toggle)
                ? OverrideState.Same : OverrideState.Different;
        }

        internal static OverrideState Combine(OverrideState a, OverrideState b)
        {
            return (OverrideState)Math.Max((int)a, (int)b);
        }

        internal static bool HasAnyOverride()
        {
            foreach (MarshmallowJarToggle toggle in MarshmallowJarToggles.All)
            {
                if (MarshmallowJarToggles.HasOverride(toggle))
                {
                    return true;
                }
            }
            return MarshmallowJarNotificationSettings.HasAnyOverride();
        }

        internal static void DrawOverrideHighlight(Rect row, OverrideState state)
        {
            if (state == OverrideState.None)
            {
                return;
            }
            Color fill = state == OverrideState.Same ? OverrideFillColor : MismatchFillColor;
            Color bar = state == OverrideState.Same ? OverrideBarColor : MismatchBarColor;
            EditorGUI.DrawRect(new Rect(row.x - 6f, row.y, row.width + 6f, row.height), fill);
            EditorGUI.DrawRect(new Rect(row.x - 6f, row.y, 2f, row.height), bar);
        }

        internal static void DrawProjectStateLabel(Rect rect, OverrideState state, GUIContent differentContent)
        {
            EnsureStyles();
            if (state == OverrideState.Same)
            {
                GUI.Label(rect, ProjectOverriddenContent, OverrideLabelStyle);
            }
            else if (state == OverrideState.Different)
            {
                GUI.Label(rect, differentContent, MismatchLabelStyle);
            }
        }

        internal static bool DrawStateCell(Rect rect, bool overridden, GUIContent revertContent)
        {
            EnsureStyles();
            if (overridden)
            {
                return GUI.Button(rect, revertContent, EditorStyles.miniButton);
            }
            GUI.Label(rect, FollowGlobalContent, StateLabelStyle);
            return false;
        }
    }
}
