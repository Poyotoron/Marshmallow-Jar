using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Poyo.MarshmallowJar.Editor.MarshmallowJarToggleGUI;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class NotificationSettingsGUI
    {
        private const float SoundWidth = 40f;
        private const float WebhookWidth = 60f;
        private const float ButtonWidth = 64f;
#if MARSHMALLOW_JAR_HAS_VRCSDK_BASE
        private const bool HasVrcSdkBase = true;
#else
        private const bool HasVrcSdkBase = false;
#endif
#if MARSHMALLOW_JAR_HAS_CAU
        private const bool HasCau = true;
#else
        private const bool HasCau = false;
#endif
        private static readonly GUIContent Heading = new GUIContent("完了通知");
        private static readonly GUIContent MasterTitle = new GUIContent("時間のかかる処理が終わったら知らせる");
        private static readonly GUIContent MasterDescription = new GUIContent("アップロードや変換などが終わったとき、音と Webhook で知らせます。イベントごとに選べます。");
        private static readonly GUIContent SoundHeading = new GUIContent("音");
        private static readonly GUIContent WebhookHeading = new GUIContent("Webhook");
        private static readonly GUIContent MinSecondsContent = new GUIContent("知らせる最短の時間（秒）", "Play モードへの切り替えと、インポート・コンパイルは、これより短く終わったときは知らせません。");
        private static readonly GUIContent SuccessSound = new GUIContent("成功したときの音");
        private static readonly GUIContent FailureSound = new GUIContent("失敗したときの音");
        private static readonly GUIContent DefaultSound = new GUIContent("既定の音");
        private static readonly GUIContent ChooseSound = new GUIContent("選ぶ…");
        private static readonly GUIContent ResetSound = new GUIContent("既定に戻す");
        private static readonly GUIContent PlaySound = new GUIContent("鳴らす");
        private static readonly GUIContent OtherOs = new GUIContent("Windows 以外ではビープ音で知らせます。");
        private static readonly GUIContent UrlContent = new GUIContent("Webhook URL");
        private static readonly GUIContent TestSend = new GUIContent("テスト送信");
        private static readonly GUIContent TemplateHeading = new GUIContent("本文のテンプレート（JSON）");
        private static readonly GUIContent ResetTemplate = new GUIContent("Discord 用に戻す");
        private static readonly GUIContent TemplateDescription = new GUIContent("{title} タイトル / {message} 本文 / {result} 成功・失敗 / {project} プロジェクト名 / {time} 時刻 / {color} 色（数値。Discord の埋め込み用）");
        private static readonly GUIContent OpenGlobal = new GUIContent("全体設定を開く");
        private static readonly GUIContent ProjectDescription = new GUIContent("ここで変えた値は、このプロジェクトだけに効きます。イベントごとの ON / OFF は ProjectSettings に、音・Webhook・時間は UserSettings（リポジトリで共有しない場所）に保存します。");
        private static readonly GUILayoutOption[] TemplateOptions = { GUILayout.MinHeight(140f) };
        private static readonly MarshmallowJarNotificationEvent[] Events =
        {
            MarshmallowJarNotificationEvent.Upload, MarshmallowJarNotificationEvent.BatchUpload,
            MarshmallowJarNotificationEvent.OutfitConversion, MarshmallowJarNotificationEvent.PlayMode,
            MarshmallowJarNotificationEvent.Import,
        };
        private static readonly GUIContent[] EventContents =
        {
            new GUIContent(HasVrcSdkBase ? "VRChat へのアップロード" : "VRChat へのアップロード（VRChat SDK 3.10.0 以上が必要）"),
            new GUIContent(HasCau ? "Continuous Avatar Uploader の連続アップロード" : "Continuous Avatar Uploader の連続アップロード（0.3.8 以上が必要）"),
            new GUIContent("もちふぃった～ の衣装変換"),
            new GUIContent("Play モードへの切り替え"),
            new GUIContent("パッケージのインポートとスクリプトのコンパイル"),
        };
        private static readonly string[] CachedSoundPaths = new string[4];
        private static readonly GUIContent[] CachedSoundFiles = new GUIContent[4];

        internal static void DrawGlobal()
        {
            MarshmallowJarToggleGUI.EnsureStyles();
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(Heading, EditorStyles.boldLabel);
            MarshmallowJarToggleGUI.DrawGlobalFeature(MarshmallowJarToggle.CompletionNotifications, MasterTitle, MasterDescription, true);
            EditorGUI.BeginDisabledGroup(!MarshmallowJarToggles.GetGlobal(MarshmallowJarToggle.CompletionNotifications));
            DrawTable(false);
            DrawValues(false);
            EditorGUI.EndDisabledGroup();
        }

        internal static void DrawProject()
        {
            MarshmallowJarToggleGUI.EnsureStyles();
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(Heading, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(ProjectDescription, MarshmallowJarToggleGUI.DescriptionStyle);
            if (GUILayout.Button(OpenGlobal))
            {
                SettingsService.OpenUserPreferences(MarshmallowJarInfo.PreferencesPath);
            }
            MarshmallowJarToggleGUI.DrawProjectFeature(MarshmallowJarToggle.CompletionNotifications, MasterTitle, MasterDescription, true);
            EditorGUI.BeginDisabledGroup(!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.CompletionNotifications));
            DrawTable(true);
            DrawValues(true);
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawValues(bool project)
        {
            MarshmallowJarGlobalSettings global = MarshmallowJarGlobalSettings.Instance;
            OverrideState state = GetValueRects(MarshmallowJarNotificationValue.MinSeconds, out Rect fieldRect, out Rect stateRect);
            EditorGUI.BeginChangeCheck();
            int minSeconds = EditorGUI.IntSlider(fieldRect, MinSecondsContent,
                project ? MarshmallowJarNotificationSettings.MinSeconds : global.notifyMinSeconds, 0, 600);
            if (EditorGUI.EndChangeCheck())
            {
                if (project)
                {
                    MarshmallowJarNotificationSettings.SetMinSeconds(minSeconds);
                }
                else
                {
                    global.notifyMinSeconds = minSeconds;
                    global.Save();
                }
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            DrawValueState(MarshmallowJarNotificationValue.MinSeconds, project, stateRect, state);
#if UNITY_EDITOR_WIN
            DrawSoundRow(true, project);
            DrawSoundRow(false, project);
#else
            EditorGUILayout.LabelField(OtherOs, MarshmallowJarToggleGUI.DescriptionStyle);
#endif
            state = GetValueRects(MarshmallowJarNotificationValue.WebhookUrl, out fieldRect, out stateRect);
            EditorGUI.BeginChangeCheck();
            string url = EditorGUI.PasswordField(fieldRect, UrlContent,
                project ? MarshmallowJarNotificationSettings.WebhookUrl : global.webhookUrl);
            if (EditorGUI.EndChangeCheck())
            {
                if (project)
                {
                    MarshmallowJarNotificationSettings.SetWebhookUrl(url);
                }
                else
                {
                    global.webhookUrl = url.Trim();
                    global.Save();
                }
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            DrawValueState(MarshmallowJarNotificationValue.WebhookUrl, project, stateRect, state);
            url = project ? MarshmallowJarNotificationSettings.WebhookUrl : global.webhookUrl;
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(url));
            if (GUILayout.Button(TestSend))
            {
                WebhookSender.Send(url, project ? MarshmallowJarNotificationSettings.WebhookTemplate : global.webhookTemplate,
                    "テスト送信", "Marshmallow Jar からのテスト通知です。", true, true);
            }
            EditorGUI.EndDisabledGroup();

            state = GetValueRects(MarshmallowJarNotificationValue.WebhookTemplate, out fieldRect, out stateRect);
            GUI.Label(fieldRect, TemplateHeading);
            DrawValueState(MarshmallowJarNotificationValue.WebhookTemplate, project, stateRect, state);
            EditorGUI.BeginChangeCheck();
            string template = EditorGUILayout.TextArea(
                project ? MarshmallowJarNotificationSettings.WebhookTemplate : global.webhookTemplate, TemplateOptions);
            if (EditorGUI.EndChangeCheck())
            {
                SetWebhookTemplate(project, template);
            }
            EditorGUILayout.LabelField(TemplateDescription, MarshmallowJarToggleGUI.DescriptionStyle);
            if (GUILayout.Button(ResetTemplate))
            {
                SetWebhookTemplate(project, MarshmallowJarGlobalSettings.DiscordWebhookTemplate);
            }
        }

        private static OverrideState GetValueRects(MarshmallowJarNotificationValue value, out Rect fieldRect, out Rect stateRect)
        {
            Rect row = EditorGUILayout.GetControlRect();
            stateRect = new Rect(row.xMax - StateWidth, row.y, StateWidth, row.height);
            fieldRect = new Rect(row.x, row.y, row.width - StateWidth - 4f, row.height);
            OverrideState state = MarshmallowJarNotificationSettings.GetOverrideState(value);
            DrawOverrideHighlight(row, state);
            return state;
        }

        private static void DrawValueState(MarshmallowJarNotificationValue value, bool project, Rect rect, OverrideState state)
        {
            if (project)
            {
                if (DrawStateCell(rect, state != OverrideState.None, RevertContent))
                {
                    MarshmallowJarNotificationSettings.ClearOverride(value);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
            else
            {
                DrawProjectStateLabel(rect, state, ProjectDiffersContent);
            }
        }

        private static void SetWebhookTemplate(bool project, string template)
        {
            if (project)
            {
                MarshmallowJarNotificationSettings.SetWebhookTemplate(template);
            }
            else
            {
                MarshmallowJarGlobalSettings.Instance.webhookTemplate = template;
                MarshmallowJarGlobalSettings.Instance.Save();
            }
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private static void DrawTable(bool project)
        {
            Rect header = EditorGUILayout.GetControlRect();
            SplitRow(header, out Rect label, out Rect sound, out Rect webhook, out Rect stateRect);
            GUI.Label(sound, SoundHeading, EditorStyles.miniLabel);
            GUI.Label(webhook, WebhookHeading, EditorStyles.miniLabel);
            for (int i = 0; i < Events.Length; i++)
            {
                MarshmallowJarNotificationEvent e = Events[i];
                Rect row = EditorGUILayout.GetControlRect();
                SplitRow(row, out label, out sound, out webhook, out stateRect);
                MarshmallowJarNotifier.GetToggles(e, out MarshmallowJarToggle soundToggle, out MarshmallowJarToggle webhookToggle);
                OverrideState state = Combine(GetOverrideState(soundToggle), GetOverrideState(webhookToggle));
                bool overridden = state != OverrideState.None;
                DrawOverrideHighlight(row, state);
                EditorGUI.BeginDisabledGroup((e == MarshmallowJarNotificationEvent.Upload && !HasVrcSdkBase) ||
                    (e == MarshmallowJarNotificationEvent.BatchUpload && !HasCau));
                GUI.Label(label, EventContents[i], project && overridden ? EditorStyles.boldLabel : EditorStyles.label);
                DrawCell(sound, soundToggle, project);
                DrawCell(webhook, webhookToggle, project);
                if (project && MarshmallowJarToggleGUI.DrawStateCell(stateRect, overridden, MarshmallowJarToggleGUI.RevertContent))
                {
                    MarshmallowJarToggles.ClearOverride(soundToggle);
                    MarshmallowJarToggles.ClearOverride(webhookToggle);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
                if (!project)
                {
                    DrawProjectStateLabel(stateRect, state, ProjectDiffersContent);
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private static void SplitRow(Rect row, out Rect label, out Rect sound, out Rect webhook, out Rect state)
        {
            float stateWidth = MarshmallowJarToggleGUI.StateWidth + 4f;
            float right = row.xMax - stateWidth;
            label = new Rect(row.x, row.y, Mathf.Max(0, right - row.x - SoundWidth - WebhookWidth), row.height);
            sound = new Rect(right - SoundWidth - WebhookWidth, row.y, SoundWidth, row.height);
            webhook = new Rect(right - WebhookWidth, row.y, WebhookWidth, row.height);
            state = new Rect(row.xMax - MarshmallowJarToggleGUI.StateWidth, row.y, MarshmallowJarToggleGUI.StateWidth, row.height);
        }

        private static void DrawCell(Rect rect, MarshmallowJarToggle toggle, bool project)
        {
            bool current = project ? MarshmallowJarToggles.IsOn(toggle) : MarshmallowJarToggles.GetGlobal(toggle);
            bool value = EditorGUI.Toggle(rect, current);
            if (value == current)
            {
                return;
            }
            if (project)
            {
                MarshmallowJarToggles.SetOverride(toggle, value);
            }
            else
            {
                MarshmallowJarToggles.SetGlobal(toggle, value);
            }
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private static GUIContent GetSoundFile(bool success, bool project, string path)
        {
            // NOTE: ファイル名はパスが変わるときだけ作り直し、毎回の描画では割り当てない。
            int index = (project ? 2 : 0) + (success ? 1 : 0);
            if (CachedSoundFiles[index] == null || CachedSoundPaths[index] != path)
            {
                CachedSoundPaths[index] = path;
                CachedSoundFiles[index] = string.IsNullOrEmpty(path) ? DefaultSound : new GUIContent(Path.GetFileName(path));
            }
            return CachedSoundFiles[index];
        }

        private static void DrawSoundRow(bool success, bool project)
        {
            MarshmallowJarGlobalSettings settings = MarshmallowJarGlobalSettings.Instance;
            string path = project ?
                (success ? MarshmallowJarNotificationSettings.SuccessSoundPath : MarshmallowJarNotificationSettings.FailureSoundPath) :
                (success ? settings.successSoundPath : settings.failureSoundPath);
            MarshmallowJarNotificationValue value = success ? MarshmallowJarNotificationValue.SuccessSound : MarshmallowJarNotificationValue.FailureSound;
            OverrideState state = GetValueRects(value, out Rect row, out Rect stateRect);
            Rect titleRect = new Rect(row.x, row.y, EditorGUIUtility.labelWidth, row.height);
            Rect fileRect = new Rect(titleRect.xMax, row.y, Mathf.Max(0, row.width - titleRect.width - ButtonWidth * 3f), row.height);
            Rect chooseRect = new Rect(row.xMax - ButtonWidth * 3f, row.y, ButtonWidth, row.height);
            Rect resetRect = new Rect(row.xMax - ButtonWidth * 2f, row.y, ButtonWidth, row.height);
            Rect playRect = new Rect(row.xMax - ButtonWidth, row.y, ButtonWidth, row.height);
            GUI.Label(titleRect, success ? SuccessSound : FailureSound);
            GUI.Label(fileRect, GetSoundFile(success, project, path));
            if (GUI.Button(chooseRect, ChooseSound))
            {
                string directory = string.IsNullOrEmpty(path)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media")
                    : Path.GetDirectoryName(path);
                string selected = EditorUtility.OpenFilePanel("通知音を選ぶ", directory, "wav");
                if (!string.IsNullOrEmpty(selected))
                {
                    SetSoundPath(success, project, selected);
                }
                // NOTE: ファイルダイアログの後はレイアウトの状態が変わるため、その描画を続けない。
                GUIUtility.ExitGUI();
            }
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(path));
            if (GUI.Button(resetRect, ResetSound))
            {
                SetSoundPath(success, project, string.Empty);
            }
            EditorGUI.EndDisabledGroup();
            if (GUI.Button(playRect, PlaySound))
            {
                NotificationSound.PlayPath(success, path);
            }
            DrawValueState(value, project, stateRect, state);
        }

        private static void SetSoundPath(bool success, bool project, string path)
        {
            if (project)
            {
                MarshmallowJarNotificationSettings.SetSoundPath(success, path);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                return;
            }
            MarshmallowJarGlobalSettings settings = MarshmallowJarGlobalSettings.Instance;
            if (success)
            {
                settings.successSoundPath = path;
            }
            else
            {
                settings.failureSoundPath = path;
            }
            settings.Save();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
