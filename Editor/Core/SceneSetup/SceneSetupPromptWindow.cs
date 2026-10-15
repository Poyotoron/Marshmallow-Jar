using System;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    internal sealed class SceneSetupPromptWindow : EditorWindow
    {
        private const string MessageBody =
            "のライティングは、アバターの色味を確かめる設定になっていません。\n" +
            "推奨の設定に変換しますか？\n\n" +
            "・環境光: Gradient（空 #A0A0A0 / 地平 #808080 / 地面 #606060）\n" +
            "・Realtime Shadow Color: #A0A0A0\n" +
            "・Directional Light: 回転 (40, -145, -20) / 白 / 強さ 0.8\n" +
            "・Scene ビューの視野角: 40°\n\n" +
            "変換したあとシーンを保存すると確定します。閉じると「今回はしない」と同じになります。";

        private static readonly GUIContent ConvertContent = new GUIContent("変換する");
        private static readonly GUIContent SkipContent = new GUIContent("今回はしない");
        private static readonly GUIContent IgnoreContent = new GUIContent("このシーンでは確認しない");

        [SerializeField] private string _message;
        [NonSerialized] private bool _closingProgrammatically;

        internal static void Open(string sceneName)
        {
            SceneSetupPromptWindow window = GetWindow<SceneSetupPromptWindow>(true, MarshmallowJarInfo.DisplayName, true);
            window.minSize = window.maxSize = new Vector2(440f, 250f);
            window._message = "シーン「" + sceneName + "」" + MessageBody;
            window.Repaint();
        }

        private void OnGUI()
        {
            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.SceneSetup))
            {
                // NOTE: 確認中に機能を無効にした場合は、辞退や無視として記録せず確認だけを取り下げる。
                SceneSetupWatcher.ClearPending();
                return;
            }

            if (!SceneSetupWatcher.HasPending)
            {
                CloseProgrammatically();
                return;
            }

            EditorGUILayout.LabelField(_message, EditorStyles.wordWrappedLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(ConvertContent))
            {
                SceneSetupWatcher.ConvertActiveScene();
            }
            if (GUILayout.Button(SkipContent))
            {
                SceneSetupWatcher.SkipPending();
            }
            if (GUILayout.Button(IgnoreContent))
            {
                SceneSetupWatcher.IgnorePending();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CloseProgrammatically()
        {
            if (_closingProgrammatically)
            {
                return;
            }

            _closingProgrammatically = true;
            Close();
        }

        internal static void CloseIfOpen()
        {
            if (!HasOpenInstances<SceneSetupPromptWindow>())
            {
                return;
            }

            foreach (SceneSetupPromptWindow window in Resources.FindObjectsOfTypeAll<SceneSetupPromptWindow>())
            {
                window.CloseProgrammatically();
            }
        }

        private void OnDestroy()
        {
            if (!_closingProgrammatically && SceneSetupWatcher.HasPending)
            {
                // NOTE: 辞退処理が破棄中のウィンドウをもう一度閉じないよう、呼び出す前に閉じ始めた印を付ける。
                _closingProgrammatically = true;
                SceneSetupWatcher.SkipPending();
            }
        }
    }
}
