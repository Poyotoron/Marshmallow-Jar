using System;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    public static class MarshmallowJarNotifier
    {
        internal const string BatchRunningSessionKey = "net.maaaaa.marshmallow-jar.Notify.BatchRunning";
        public static int MinSeconds => MarshmallowJarNotificationSettings.MinSeconds;

        public static void Notify(MarshmallowJarNotificationEvent notificationEvent, bool success, string title, string message)
        {
            if (Application.isBatchMode)
            {
                return;
            }
            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.CompletionNotifications))
            {
                return;
            }
            // NOTE: 連続アップロードの途中のアップロード・Play モード・コンパイルは、終わりにまとめて知らせる。
            if ((notificationEvent == MarshmallowJarNotificationEvent.Upload ||
                notificationEvent == MarshmallowJarNotificationEvent.PlayMode ||
                notificationEvent == MarshmallowJarNotificationEvent.Import) &&
                SessionState.GetBool(BatchRunningSessionKey, false))
            {
                return;
            }
            GetToggles(notificationEvent, out MarshmallowJarToggle sound, out MarshmallowJarToggle webhook);
            bool playSound = MarshmallowJarToggles.IsOn(sound);
            bool sendWebhook = MarshmallowJarToggles.IsOn(webhook);
            if (!playSound && !sendWebhook)
            {
                return;
            }
            Debug.Log(MarshmallowJarInfo.LogPrefix + title + ": " + message);
            if (playSound)
            {
                NotificationSound.Play(success);
            }
            if (sendWebhook)
            {
                WebhookSender.Send(MarshmallowJarNotificationSettings.WebhookUrl, MarshmallowJarNotificationSettings.WebhookTemplate,
                    title, message, success, false);
            }
        }

        internal static void GetToggles(MarshmallowJarNotificationEvent e, out MarshmallowJarToggle sound, out MarshmallowJarToggle webhook)
        {
            switch (e)
            {
                case MarshmallowJarNotificationEvent.Upload:
                    sound = MarshmallowJarToggle.NotifyUploadSound;
                    webhook = MarshmallowJarToggle.NotifyUploadWebhook;
                    break;
                case MarshmallowJarNotificationEvent.BatchUpload:
                    sound = MarshmallowJarToggle.NotifyBatchUploadSound;
                    webhook = MarshmallowJarToggle.NotifyBatchUploadWebhook;
                    break;
                case MarshmallowJarNotificationEvent.OutfitConversion:
                    sound = MarshmallowJarToggle.NotifyOutfitConversionSound;
                    webhook = MarshmallowJarToggle.NotifyOutfitConversionWebhook;
                    break;
                case MarshmallowJarNotificationEvent.PlayMode:
                    sound = MarshmallowJarToggle.NotifyPlayModeSound;
                    webhook = MarshmallowJarToggle.NotifyPlayModeWebhook;
                    break;
                case MarshmallowJarNotificationEvent.Import:
                    sound = MarshmallowJarToggle.NotifyImportSound;
                    webhook = MarshmallowJarToggle.NotifyImportWebhook;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        public static string FormatElapsed(double seconds)
        {
            if (seconds < 0)
            {
                return string.Empty;
            }
            int total = (int)Math.Round(seconds);
            return total < 60 ? $"{total} 秒" : $"{total / 60} 分 {total % 60:00} 秒";
        }
    }
}
