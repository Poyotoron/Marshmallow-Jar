using Poyo.MarshmallowJar.Editor;
using UnityEditor;
using VRC.SDKBase.Editor;

namespace Poyo.MarshmallowJar.VRChat.Editor
{
    [InitializeOnLoad]
    internal static class SdkUploadWatcher
    {
        private const double PollInterval = 0.5;
        // NOTE: SDK がマルチプラットフォームアップロード中に立てるキー。状態を持つ SDK の型が公開されていないため直接読む。
        private const string MultiPlatformBuildKey = "VRCSdkControlPanel.MultiPlatformBuild";
        private const string StartKey = "net.maaaaa.marshmallow-jar.Notify.UploadStart";
        private const string HeldCountKey = "net.maaaaa.marshmallow-jar.Notify.UploadHeldCount";
        private static double _nextPollTime;
        private static IVRCSdkBuilderApi _builder;

        static SdkUploadWatcher()
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
            FlushIfDone();
            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.CompletionNotifications) || VRCSdkControlPanel.window == null)
            {
                Unsubscribe();
                return;
            }
            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkBuilderApi>(out IVRCSdkBuilderApi api) || ReferenceEquals(api, _builder))
            {
                return;
            }
            Unsubscribe();
            api.OnSdkBuildStart += OnBuildStart;
            api.OnSdkBuildError += OnBuildError;
            api.OnSdkUploadSuccess += OnUploadSuccess;
            api.OnSdkUploadError += OnUploadError;
            _builder = api;
        }

        private static void Unsubscribe()
        {
            if (_builder == null)
            {
                return;
            }
            _builder.OnSdkBuildStart -= OnBuildStart;
            _builder.OnSdkBuildError -= OnBuildError;
            _builder.OnSdkUploadSuccess -= OnUploadSuccess;
            _builder.OnSdkUploadError -= OnUploadError;
            _builder = null;
        }

        private static void OnBuildStart(object sender, object target)
        {
            if (SessionState.GetInt(HeldCountKey, 0) <= 0)
            {
                SessionState.SetFloat(StartKey, (float)EditorApplication.timeSinceStartup);
            }
        }

        private static void OnBuildError(object sender, string message)
        {
            MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.Upload, false, "ビルド失敗",
                "VRChat 向けのビルドに失敗しました: " + message);
            Reset();
        }

        private static void OnUploadError(object sender, string message)
        {
            MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.Upload, false, "アップロード失敗",
                "VRChat へのアップロードに失敗しました: " + message);
            Reset();
        }

        private static void OnUploadSuccess(object sender, string contentId)
        {
            int count = SessionState.GetInt(HeldCountKey, 0);
            if (SessionState.GetBool(MultiPlatformBuildKey, false))
            {
                SessionState.SetInt(HeldCountKey, count + 1);
                return;
            }
            NotifySuccess(count + 1);
            Reset();
        }

        private static void FlushIfDone()
        {
            int count = SessionState.GetInt(HeldCountKey, 0);
            // NOTE: 最後の成功イベントの時点ではまだキーが立っていて、少し後に消えるため確定を待つ。
            if (count > 0 && !SessionState.GetBool(MultiPlatformBuildKey, false))
            {
                NotifySuccess(count);
                Reset();
            }
        }

        private static void NotifySuccess(int count)
        {
            float start = SessionState.GetFloat(StartKey, -1);
            string duration = MarshmallowJarNotifier.FormatElapsed(start < 0 ? -1 : EditorApplication.timeSinceStartup - start);
            string detail = count >= 2 ? count + " プラットフォーム" : string.Empty;
            if (!string.IsNullOrEmpty(duration))
            {
                detail += (detail.Length > 0 ? "、" : string.Empty) + duration;
            }
            string message = "VRChat へのアップロードが完了しました" + (detail.Length > 0 ? "（" + detail + "）" : "。");
            MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.Upload, true, "アップロード完了", message);
        }

        private static void Reset()
        {
            SessionState.EraseFloat(StartKey);
            SessionState.EraseInt(HeldCountKey);
        }
    }
}
