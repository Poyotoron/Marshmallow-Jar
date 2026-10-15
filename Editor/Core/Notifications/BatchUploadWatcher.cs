using System.IO;
using UnityEditor;
using UnityEngine;

#if MARSHMALLOW_JAR_HAS_CAU
namespace Poyo.MarshmallowJar.Editor
{
    [InitializeOnLoad]
    internal static class BatchUploadWatcher
    {
        private const double PollInterval = 1.0;
        // NOTE: Continuous Avatar Uploader は連続アップロードの間だけこのアセットを置き、全部終わると消すため、終わりの合図に使う。
        private const string ProgressAssetPath = "Assets/com.anatawa12.continuous-avatar-uploader.uploader-progress.asset";
        private const string StartKey = "net.maaaaa.marshmallow-jar.Notify.BatchStart";
        private const string AvatarCountKey = "net.maaaaa.marshmallow-jar.Notify.BatchAvatars";
        private const string PlatformCountKey = "net.maaaaa.marshmallow-jar.Notify.BatchPlatforms";
        private const string ErrorCountKey = "net.maaaaa.marshmallow-jar.Notify.BatchErrors";
        private static double _nextPollTime;

        static BatchUploadWatcher()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now < _nextPollTime)
            {
                return;
            }
            _nextPollTime = now + PollInterval;
            Evaluate(File.Exists(ProgressAssetPath));
        }

        internal static void Evaluate(bool exists)
        {
            bool running = SessionState.GetBool(MarshmallowJarNotifier.BatchRunningSessionKey, false);
            if (exists && !running)
            {
                if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.CompletionNotifications))
                {
                    return;
                }
                SessionState.SetBool(MarshmallowJarNotifier.BatchRunningSessionKey, true);
                SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
                SessionState.SetInt(AvatarCountKey, 0);
                SessionState.SetInt(PlatformCountKey, 0);
                SessionState.SetInt(ErrorCountKey, 0);
            }
            else if (exists && running)
            {
                ReadCounts();
            }
            else if (!exists && running)
            {
                Finish();
            }
        }

        private static void ReadCounts()
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(ProgressAssetPath);
            if (asset == null)
            {
                return;
            }
            // NOTE: 型が公開されていないため、保存された配列の長さだけを読む。
            using (SerializedObject serialized = new SerializedObject(asset))
            {
                StoreCount(serialized, "uploadSettings", AvatarCountKey);
                StoreCount(serialized, "targetPlatforms", PlatformCountKey);
                StoreCount(serialized, "uploadErrors", ErrorCountKey);
            }
        }

        private static void StoreCount(SerializedObject serialized, string propertyName, string key)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property != null && property.isArray)
            {
                SessionState.SetInt(key, property.arraySize);
            }
        }

        private static void Finish()
        {
            double elapsed = EditorApplication.timeSinceStartup - SessionState.GetFloat(StartKey, 0);
            int avatars = SessionState.GetInt(AvatarCountKey, 0);
            int platforms = SessionState.GetInt(PlatformCountKey, 0);
            int errors = SessionState.GetInt(ErrorCountKey, 0);
            // NOTE: 途中で通知を OFF にしていても、他の通知を止め続けないよう必ず状態を消す。
            SessionState.EraseBool(MarshmallowJarNotifier.BatchRunningSessionKey);
            SessionState.EraseFloat(StartKey);
            SessionState.EraseInt(AvatarCountKey);
            SessionState.EraseInt(PlatformCountKey);
            SessionState.EraseInt(ErrorCountKey);
            string duration = MarshmallowJarNotifier.FormatElapsed(elapsed);
            if (errors > 0)
            {
                MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.BatchUpload, false, "連続アップロードで失敗あり",
                    "Continuous Avatar Uploader のアップロードが終わりました。" + errors + " 件失敗しています（" + duration + "）");
            }
            else
            {
                string detail = avatars > 0 ? "アバター " + avatars + " 体 × " + platforms + " プラットフォーム、" : string.Empty;
                MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.BatchUpload, true, "連続アップロード完了",
                    "Continuous Avatar Uploader のアップロードが終わりました（" + detail + duration + "）");
            }
        }
    }
}
#endif
