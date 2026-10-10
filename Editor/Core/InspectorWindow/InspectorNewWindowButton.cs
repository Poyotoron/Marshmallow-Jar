using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    [InitializeOnLoad]
    internal static class InspectorNewWindowButton
    {
        private static readonly GUIContent ButtonContent =
            new GUIContent("別ウィンドウで開く", "このオブジェクトを固定したウィンドウを開きます。ほかを選んでも表示は切り替わりません。");
        private static GUILayoutOption[] _buttonOptions;

        static InspectorNewWindowButton()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
        }

        private static void OnHeaderGUI(UnityEditor.Editor editor)
        {
            if (!MarshmallowJarSettings.instance.inspectorNewWindowButton)
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

            if (_buttonOptions == null)
            {
                _buttonOptions = new[] { GUILayout.Width(EditorStyles.miniButton.CalcSize(ButtonContent).x + 8f) };
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(ButtonContent, EditorStyles.miniButton, _buttonOptions))
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
