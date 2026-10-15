using System;
using UnityEngine;
using static Poyo.MarshmallowJar.Editor.MarshmallowJarToggleGUI;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class MarshmallowJarNotificationSettings
    {
        internal static int MinSeconds => MarshmallowJarUserSettings.instance.overrideNotifyMinSeconds ?
            MarshmallowJarUserSettings.instance.notifyMinSeconds : MarshmallowJarGlobalSettings.Instance.notifyMinSeconds;
        internal static string SuccessSoundPath => MarshmallowJarUserSettings.instance.overrideSuccessSoundPath ?
            MarshmallowJarUserSettings.instance.successSoundPath : MarshmallowJarGlobalSettings.Instance.successSoundPath;
        internal static string FailureSoundPath => MarshmallowJarUserSettings.instance.overrideFailureSoundPath ?
            MarshmallowJarUserSettings.instance.failureSoundPath : MarshmallowJarGlobalSettings.Instance.failureSoundPath;
        internal static string WebhookUrl => MarshmallowJarUserSettings.instance.overrideWebhookUrl ?
            MarshmallowJarUserSettings.instance.webhookUrl : MarshmallowJarGlobalSettings.Instance.webhookUrl;
        internal static string WebhookTemplate => MarshmallowJarUserSettings.instance.overrideWebhookTemplate ?
            MarshmallowJarUserSettings.instance.webhookTemplate : MarshmallowJarGlobalSettings.Instance.webhookTemplate;

        internal static bool HasOverride(MarshmallowJarNotificationValue value)
        {
            MarshmallowJarUserSettings settings = MarshmallowJarUserSettings.instance;
            switch (value)
            {
                case MarshmallowJarNotificationValue.MinSeconds:
                    return settings.overrideNotifyMinSeconds;
                case MarshmallowJarNotificationValue.SuccessSound:
                    return settings.overrideSuccessSoundPath;
                case MarshmallowJarNotificationValue.FailureSound:
                    return settings.overrideFailureSoundPath;
                case MarshmallowJarNotificationValue.WebhookUrl:
                    return settings.overrideWebhookUrl;
                case MarshmallowJarNotificationValue.WebhookTemplate:
                    return settings.overrideWebhookTemplate;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        internal static void ClearOverride(MarshmallowJarNotificationValue value)
        {
            MarshmallowJarUserSettings settings = MarshmallowJarUserSettings.instance;
            switch (value)
            {
                case MarshmallowJarNotificationValue.MinSeconds:
                    settings.overrideNotifyMinSeconds = false;
                    break;
                case MarshmallowJarNotificationValue.SuccessSound:
                    settings.overrideSuccessSoundPath = false;
                    break;
                case MarshmallowJarNotificationValue.FailureSound:
                    settings.overrideFailureSoundPath = false;
                    break;
                case MarshmallowJarNotificationValue.WebhookUrl:
                    settings.overrideWebhookUrl = false;
                    break;
                case MarshmallowJarNotificationValue.WebhookTemplate:
                    settings.overrideWebhookTemplate = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
            settings.SaveChanges();
        }

        internal static bool HasAnyOverride()
        {
            MarshmallowJarUserSettings settings = MarshmallowJarUserSettings.instance;
            return settings.overrideNotifyMinSeconds || settings.overrideSuccessSoundPath || settings.overrideFailureSoundPath ||
                settings.overrideWebhookUrl || settings.overrideWebhookTemplate;
        }

        internal static OverrideState GetOverrideState(MarshmallowJarNotificationValue value)
        {
            if (!HasOverride(value))
            {
                return OverrideState.None;
            }
            MarshmallowJarGlobalSettings global = MarshmallowJarGlobalSettings.Instance;
            bool same;
            switch (value)
            {
                case MarshmallowJarNotificationValue.MinSeconds:
                    same = MinSeconds == global.notifyMinSeconds;
                    break;
                case MarshmallowJarNotificationValue.SuccessSound:
                    same = AreEqual(SuccessSoundPath, global.successSoundPath);
                    break;
                case MarshmallowJarNotificationValue.FailureSound:
                    same = AreEqual(FailureSoundPath, global.failureSoundPath);
                    break;
                case MarshmallowJarNotificationValue.WebhookUrl:
                    same = AreEqual(WebhookUrl, global.webhookUrl);
                    break;
                case MarshmallowJarNotificationValue.WebhookTemplate:
                    same = AreEqual(WebhookTemplate, global.webhookTemplate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
            return same ? OverrideState.Same : OverrideState.Different;
        }

        private static bool AreEqual(string a, string b)
        {
            return string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.Ordinal);
        }

        internal static void SetMinSeconds(int seconds)
        {
            MarshmallowJarUserSettings settings = MarshmallowJarUserSettings.instance;
            settings.notifyMinSeconds = Mathf.Clamp(seconds, 0, 600);
            settings.overrideNotifyMinSeconds = true;
            settings.SaveChanges();
        }

        internal static void SetSoundPath(bool success, string path)
        {
            MarshmallowJarUserSettings settings = MarshmallowJarUserSettings.instance;
            if (success)
            {
                settings.successSoundPath = path;
                settings.overrideSuccessSoundPath = true;
            }
            else
            {
                settings.failureSoundPath = path;
                settings.overrideFailureSoundPath = true;
            }
            settings.SaveChanges();
        }

        internal static void SetWebhookUrl(string url)
        {
            MarshmallowJarUserSettings settings = MarshmallowJarUserSettings.instance;
            settings.webhookUrl = (url ?? string.Empty).Trim();
            settings.overrideWebhookUrl = true;
            settings.SaveChanges();
        }

        internal static void SetWebhookTemplate(string template)
        {
            MarshmallowJarUserSettings settings = MarshmallowJarUserSettings.instance;
            settings.webhookTemplate = template;
            settings.overrideWebhookTemplate = true;
            settings.SaveChanges();
        }
    }
}
