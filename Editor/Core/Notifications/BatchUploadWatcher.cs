#if MARSHMALLOW_JAR_HAS_CAU
using System;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Poyo.MarshmallowJar.Editor
{
    [InitializeOnLoad]
    internal static class BatchUploadWatcher
    {
        private const double PollInterval = 1.0;
        // NOTE: Continuous Avatar Uploader は連続アップロードの間だけこのアセットを置き、全部終わると消すため、終わりの合図に使う。
        internal const string ProgressAssetPath = "Assets/com.anatawa12.continuous-avatar-uploader.uploader-progress.asset";
        private const string StartKey = "net.maaaaa.marshmallow-jar.Notify.BatchStart";
        private const string AvatarCountKey = "net.maaaaa.marshmallow-jar.Notify.BatchAvatars";
        private const string PlatformCountKey = "net.maaaaa.marshmallow-jar.Notify.BatchPlatforms";
        private const string ErrorCountKey = "net.maaaaa.marshmallow-jar.Notify.BatchErrors";
        private const string NotifiedKey = "net.maaaaa.marshmallow-jar.Notify.BatchNotified";
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
                SessionState.EraseBool(NotifiedKey);
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

        internal static void OnProgressSaved()
        {
            if (!SessionState.GetBool(MarshmallowJarNotifier.BatchRunningSessionKey, false) ||
                SessionState.GetBool(NotifiedKey, false))
            {
                return;
            }
            Object asset = AssetDatabase.LoadMainAssetAtPath(ProgressAssetPath);
            if (asset == null)
            {
                return;
            }
            using (SerializedObject serialized = new SerializedObject(asset))
            {
                SerializedProperty avatars = serialized.FindProperty("uploadSettings");
                SerializedProperty targets = serialized.FindProperty("targetPlatforms");
                SerializedProperty finished = serialized.FindProperty("uploadFinishedPlatforms");
                SerializedProperty errors = serialized.FindProperty("uploadErrors");
                SerializedProperty index = serialized.FindProperty("uploadingAvatarIndex");
                SerializedProperty platform = serialized.FindProperty("uploadingTargetPlatform");
                SerializedProperty continueOnError = serialized.FindProperty("continueUploadOnError");
                if (avatars == null || targets == null || finished == null || errors == null ||
                    index == null || platform == null || continueOnError == null)
                {
                    return;
                }
                HandleSnapshot(index.intValue, avatars.arraySize, platform.intValue,
                    ReadPlatforms(targets), ReadPlatforms(finished), errors.arraySize, continueOnError.boolValue);
            }
        }

        private static int[] ReadPlatforms(SerializedProperty property)
        {
            int[] platforms = new int[property.arraySize];
            for (int i = 0; i < platforms.Length; i++)
            {
                platforms[i] = property.GetArrayElementAtIndex(i).intValue;
            }
            return platforms;
        }

        internal static void HandleSnapshot(int avatarIndex, int avatarCount, int currentPlatform,
            int[] targetPlatforms, int[] finishedPlatforms, int errorCount, bool continueOnError)
        {
            if (!SessionState.GetBool(MarshmallowJarNotifier.BatchRunningSessionKey, false) ||
                SessionState.GetBool(NotifiedKey, false))
            {
                return;
            }
            SessionState.SetInt(AvatarCountKey, avatarCount);
            SessionState.SetInt(PlatformCountKey, targetPlatforms.Length);
            SessionState.SetInt(ErrorCountKey, errorCount);
            bool stoppedByError = !continueOnError && errorCount > 0;
            bool allDone = avatarIndex >= avatarCount;
            foreach (int platform in targetPlatforms)
            {
                if (platform != currentPlatform && Array.IndexOf(finishedPlatforms, platform) < 0)
                {
                    allDone = false;
                    break;
                }
            }
            if (!stoppedByError && !allDone)
            {
                return;
            }
            // NOTE: Continuous Avatar Uploader は完了ダイアログのあとに進捗アセットを消す。最後の保存からダイアログまで処理が途切れないので、保存の中で知らせる。
            SessionState.SetBool(NotifiedKey, true);
            // NOTE: 戻すプラットフォームへの切り替えのコンパイルを知らせないよう、進捗アセットが消えるまで実行中のままにする。
            NotifyResult();
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
            // NOTE: 保存で終わりを判定できなかったキャンセルや Continuous Avatar Uploader 側のエラーなどの受け皿。
            bool notified = SessionState.GetBool(NotifiedKey, false);
            if (!notified)
            {
                // NOTE: 通知は数と開始時刻を読むため、状態を消す前に呼ぶ。
                NotifyResult();
            }
            // NOTE: 途中で通知を OFF にしていても、他の通知を止め続けないよう必ず状態を消す。
            SessionState.EraseBool(MarshmallowJarNotifier.BatchRunningSessionKey);
            SessionState.EraseFloat(StartKey);
            SessionState.EraseInt(AvatarCountKey);
            SessionState.EraseInt(PlatformCountKey);
            SessionState.EraseInt(ErrorCountKey);
            SessionState.EraseBool(NotifiedKey);
        }

        private static void NotifyResult()
        {
            double elapsed = EditorApplication.timeSinceStartup - SessionState.GetFloat(StartKey, 0);
            int avatars = SessionState.GetInt(AvatarCountKey, 0);
            int platforms = SessionState.GetInt(PlatformCountKey, 0);
            int errors = SessionState.GetInt(ErrorCountKey, 0);
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
