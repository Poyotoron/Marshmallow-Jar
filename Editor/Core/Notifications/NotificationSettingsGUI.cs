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
        private const float TabWidth = 160f;
        private const string OtherItemsMessage = "かんたん編集に出ない項目も、JSON にはそのまま残ります。";
        private const string EmptyContentMessage = "メッセージ・タイトル・本文・フッターがすべて空だと、Discord は受け付けません。";
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
        private static readonly GUIContent TemplateHeading = new GUIContent("Webhook で送る内容");
        private static readonly GUIContent[] TabContents =
        {
            new GUIContent("かんたん編集", "欄ごとに入力します。Discord 用です。"),
            new GUIContent("JSON", "送る JSON を直接書きます。かんたん編集で変えると、JSON は決まった形に整えられます。"),
        };
        private static readonly int[] SelectedTabs = { 0, 0 };
        private static readonly string[] CachedTemplates = new string[2];
        private static readonly WebhookSimpleSnapshot[] CachedSnapshots = new WebhookSimpleSnapshot[2];
        private static readonly int[] InsertTargets = { 4, 4 };
        private static readonly string[] FieldControlNames =
        {
            "MarshmallowJar.Webhook.Username", "MarshmallowJar.Webhook.AvatarUrl",
            "MarshmallowJar.Webhook.Content", "MarshmallowJar.Webhook.Title",
            "MarshmallowJar.Webhook.Description", "MarshmallowJar.Webhook.Footer",
        };
        private static readonly GUIContent[] FieldContents =
        {
            new GUIContent("送信者名", "Discord に表示する送信者の名前です。空なら Webhook 側で設定した名前になります。"),
            new GUIContent("アイコンの URL", "送信者のアイコン画像の URL です。空なら Webhook 側で設定したアイコンになります。"),
            new GUIContent("メッセージ", "埋め込みの上に出る、ふつうのメッセージです。空なら出しません。"),
            new GUIContent("タイトル", "埋め込みのタイトルです。"),
            new GUIContent("本文", "埋め込みの本文です。"),
            new GUIContent("フッター", "埋め込みの下に小さく出る文字です。"),
        };
        private static readonly GUIContent InsertLabel = new GUIContent("差し込む");
        private static readonly string[] Placeholders = { "{title}", "{message}", "{result}", "{project}", "{time}" };
        private static readonly GUIContent[] PlaceholderContents =
        {
            new GUIContent("{title}", "通知のタイトルに置き換わります。例: アップロード完了"),
            new GUIContent("{message}", "通知の本文に置き換わります。例: VRChat へのアップロードが完了しました（3 分 05 秒）"),
            new GUIContent("{result}", "結果に置き換わります。「成功」または「失敗」"),
            new GUIContent("{project}", "プロジェクトのフォルダ名に置き換わります。"),
            new GUIContent("{time}", "知らせた時刻に置き換わります。例: 2026-10-15 21:30:00"),
        };
        private static readonly GUIContent InsertDescription = new GUIContent(
            "差し込むボタンは、最後に入力していた欄の末尾に追加します。新しく作る埋め込みの色は、成功なら緑、失敗なら赤になります。");
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
            Rect tabRect = fieldRect;
            tabRect.xMin = tabRect.xMax - TabWidth;
            fieldRect.xMax = tabRect.xMin - 4f;
            GUI.Label(fieldRect, TemplateHeading);
            int page = project ? 1 : 0;
            int tab = GUI.Toolbar(tabRect, SelectedTabs[page], TabContents, EditorStyles.miniButton);
            if (tab != SelectedTabs[page])
            {
                SelectedTabs[page] = tab;
                GUIUtility.keyboardControl = 0;
            }
            DrawValueState(MarshmallowJarNotificationValue.WebhookTemplate, project, stateRect, state);
            string template = project ? MarshmallowJarNotificationSettings.WebhookTemplate : global.webhookTemplate;
            WebhookSimpleSnapshot snapshot = GetSnapshot(page, template);
            if (SelectedTabs[page] == 0)
            {
                DrawSimpleFields(project, page, template, snapshot);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                string value = EditorGUILayout.TextArea(template, TemplateOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    SetWebhookTemplate(project, value);
                }
                EditorGUILayout.LabelField(TemplateDescription, MarshmallowJarToggleGUI.DescriptionStyle);
            }
            if (GUILayout.Button(ResetTemplate))
            {
                GUIUtility.keyboardControl = 0;
                SetWebhookTemplate(project, MarshmallowJarGlobalSettings.DiscordWebhookTemplate);
            }
        }

        private static WebhookSimpleSnapshot GetSnapshot(int page, string template)
        {
            // NOTE: 解析は文字列と木を作るので、JSON 編集やほかのプロジェクトでの変更も文字列の比較で検出し、変わったときだけ行う。
            if (CachedSnapshots[page] == null || !string.Equals(CachedTemplates[page], template, StringComparison.Ordinal))
            {
                CachedTemplates[page] = template;
                CachedSnapshots[page] = WebhookSimpleFields.Read(template);
            }
            return CachedSnapshots[page];
        }

        private static void DrawSimpleFields(bool project, int page, string template, WebhookSimpleSnapshot snapshot)
        {
            if (snapshot.ErrorMessage != null)
            {
                EditorGUILayout.HelpBox(snapshot.ErrorMessage, MessageType.Warning);
            }
            EditorGUI.indentLevel++;
            for (int i = 0; i < WebhookSimpleFields.Count; i++)
            {
                EditorGUI.BeginDisabledGroup(!snapshot.Editable[i]);
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName(FieldControlNames[i]);
                string value = EditorGUILayout.TextField(FieldContents[i], snapshot.Values[i]);
                if (EditorGUI.EndChangeCheck() && WebhookSimpleFields.TryApply(template, i, value, out string result))
                {
                    SetWebhookTemplate(project, result);
                    template = result;
                    snapshot = GetSnapshot(page, result);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel--;
            string focused = GUI.GetNameOfFocusedControl();
            for (int i = 0; i < FieldControlNames.Length; i++)
            {
                if (string.Equals(focused, FieldControlNames[i], StringComparison.Ordinal))
                {
                    InsertTargets[page] = i;
                    break;
                }
            }
            EditorGUI.BeginDisabledGroup(snapshot.ErrorMessage != null);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(InsertLabel);
            for (int i = 0; i < Placeholders.Length; i++)
            {
                if (GUILayout.Button(PlaceholderContents[i], EditorStyles.miniButton))
                {
                    // NOTE: 入力欄はフォーカス中に文字列の写しを表示するため、外さないと追加が見えず、次の入力で消える。
                    GUIUtility.keyboardControl = 0;
                    int target = InsertTargets[page];
                    // NOTE: カーソル位置は公開 API で取れないため、最後に入力していた欄の末尾に足す。
                    if (WebhookSimpleFields.TryApply(template, target, snapshot.Values[target] + Placeholders[i], out string result))
                    {
                        SetWebhookTemplate(project, result);
                        template = result;
                        snapshot = GetSnapshot(page, result);
                    }
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField(InsertDescription, MarshmallowJarToggleGUI.DescriptionStyle);
            if (snapshot.ErrorMessage == null)
            {
                if (snapshot.HasOtherItems)
                {
                    EditorGUILayout.HelpBox(OtherItemsMessage, MessageType.Info);
                }
                if (!snapshot.HasVisibleContent)
                {
                    EditorGUILayout.HelpBox(EmptyContentMessage, MessageType.Warning);
                }
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
