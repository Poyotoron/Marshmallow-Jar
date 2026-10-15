using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    [Serializable]
    internal sealed class MarshmallowJarGlobalSettings
    {
        internal const int DefaultNotifyMinSeconds = 30;
        internal const string CreatedSessionKey = "net.maaaaa.marshmallow-jar.GlobalSettingsCreated";
        internal const string DiscordWebhookTemplate = "{\n  \"username\": \"Marshmallow Jar\",\n  \"embeds\": [\n    {\n      \"title\": \"{title}\",\n      \"description\": \"{message}\",\n      \"color\": {color},\n      \"footer\": { \"text\": \"{project} / {time}\" }\n    }\n  ]\n}";

        public int schemaVersion = 1;
        public List<MarshmallowJarToggleValue> toggles = new List<MarshmallowJarToggleValue>();
        public int notifyMinSeconds = DefaultNotifyMinSeconds;
        public string successSoundPath = string.Empty;
        public string failureSoundPath = string.Empty;
        public string webhookUrl = string.Empty;
        public string webhookTemplate = DiscordWebhookTemplate;

        private static MarshmallowJarGlobalSettings _instance;
        internal static MarshmallowJarGlobalSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        internal static void Reload()
        {
            _instance = null;
            MarshmallowJarToggles.Invalidate();
        }

        private static MarshmallowJarGlobalSettings Load()
        {
            if (!EditorPrefs.HasKey(MarshmallowJarInfo.GlobalSettingsKey))
            {
                MarshmallowJarGlobalSettings initial = CreateInitial();
                initial.Save();
                SessionState.SetBool(CreatedSessionKey, true);
                return initial;
            }

            try
            {
                MarshmallowJarGlobalSettings settings = JsonUtility.FromJson<MarshmallowJarGlobalSettings>(
                    EditorPrefs.GetString(MarshmallowJarInfo.GlobalSettingsKey));
                if (settings == null)
                {
                    throw new ArgumentException("設定が空です。");
                }
                if (settings.toggles == null)
                {
                    settings.toggles = new List<MarshmallowJarToggleValue>();
                }
                settings.notifyMinSeconds = Mathf.Clamp(settings.notifyMinSeconds, 0, 600);
                return settings;
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning(MarshmallowJarInfo.LogPrefix + "全体設定を読み込めませんでした。このセッションは初期値で動きます。設定を変えると上書き保存されます。: " + exception.Message);
                return CreateInitial();
            }
        }

        private static MarshmallowJarGlobalSettings CreateInitial()
        {
            var settings = new MarshmallowJarGlobalSettings();
            // NOTE: 後の版で初期値を変えても、既にある全体設定の値を変えないため全項目を保存する。
            foreach (MarshmallowJarToggle toggle in MarshmallowJarToggles.All)
            {
                settings.SetToggle(toggle, MarshmallowJarToggles.GetInitialValue(toggle));
            }
            return settings;
        }

        internal void Save()
        {
            // NOTE: 全プロジェクトで共有され、プロジェクトにファイルを置かずに済むため EditorPrefs に保存する。
            EditorPrefs.SetString(MarshmallowJarInfo.GlobalSettingsKey, JsonUtility.ToJson(this));
            MarshmallowJarToggles.Invalidate();
        }

        internal bool TryGetToggle(MarshmallowJarToggle toggle, out bool value)
        {
            string key = toggle.ToString();
            foreach (MarshmallowJarToggleValue entry in toggles)
            {
                if (entry != null && entry.key == key)
                {
                    value = entry.value;
                    return true;
                }
            }
            value = false;
            return false;
        }

        internal void SetToggle(MarshmallowJarToggle toggle, bool value)
        {
            string key = toggle.ToString();
            foreach (MarshmallowJarToggleValue entry in toggles)
            {
                if (entry != null && entry.key == key)
                {
                    entry.value = value;
                    return;
                }
            }
            toggles.Add(new MarshmallowJarToggleValue { key = key, value = value });
        }
    }
}
