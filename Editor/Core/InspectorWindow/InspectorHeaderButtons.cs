using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    [InitializeOnLoad]
    internal static class InspectorHeaderButtons
    {
        private static readonly GUIContent ButtonContent =
            new GUIContent("別ウィンドウで開く", "このオブジェクトを固定したウィンドウを開きます。ほかを選んでも表示は切り替わりません。");
        private static GUILayoutOption[] _buttonOptions;
        private static readonly GUIContent CopyContent =
            new GUIContent("コピーを保存…", "保存先と名前を選んで、このアセットのコピーを作ります。");
        private static GUILayoutOption[] _copyOptions;

        static InspectorHeaderButtons()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
        }

        private static void OnHeaderGUI(UnityEditor.Editor editor)
        {
            bool newWindow = MarshmallowJarToggles.IsOn(MarshmallowJarToggle.InspectorNewWindowButton);
            bool copyTo = MarshmallowJarToggles.IsOn(MarshmallowJarToggle.InspectorCopyToButton);
            if (!newWindow && !copyTo)
            {
                return;
            }

            if (editor.targets.Length != 1)
            {
                return;
            }

            Object target = ResolveTarget(editor.target);
            if (target == null)
            {
                return;
            }

            bool showCopy = copyTo && AssetCopier.CanCopy(target);
            if (!newWindow && !showCopy)
            {
                return;
            }
            if (newWindow && _buttonOptions == null)
            {
                _buttonOptions = new[] { GUILayout.Width(EditorStyles.miniButton.CalcSize(ButtonContent).x + 8f) };
            }
            if (showCopy && _copyOptions == null)
            {
                _copyOptions = new[] { GUILayout.Width(EditorStyles.miniButton.CalcSize(CopyContent).x + 8f) };
            }

            // NOTE: 2 つのボタンを同じ行に並べるため、ヘッダーの描画は 1 か所で行う。
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (showCopy && GUILayout.Button(CopyContent, EditorStyles.miniButton, _copyOptions))
            {
                AssetCopier.CopyWithDialog(target);
                // NOTE: モーダルダイアログのあとはレイアウトの状態が変わるため、その描画を続けない。
                GUIUtility.ExitGUI();
            }
            if (newWindow && GUILayout.Button(ButtonContent, EditorStyles.miniButton, _buttonOptions))
            {
                EditorUtility.OpenPropertyEditor(target);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static Object ResolveTarget(Object target)
        {
            if (target is AssetImporter importer)
            {
                return AssetDatabase.LoadMainAssetAtPath(importer.assetPath);
            }

            return target;
        }
    }
}
