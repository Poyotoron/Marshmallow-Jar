using System.IO;
using UnityEditor;
using UnityEditor.Compilation;

namespace Poyo.MarshmallowJar.Editor
{
    [InitializeOnLoad]
    internal static class EditorActivityWatcher
    {
        private const string PlayModeStartKey = "net.maaaaa.marshmallow-jar.Notify.PlayModeStart";
        private const string ImportActiveKey = "net.maaaaa.marshmallow-jar.Notify.ImportActive";
        private const string ImportStartKey = "net.maaaaa.marshmallow-jar.Notify.ImportStart";
        private const string ImportPackageKey = "net.maaaaa.marshmallow-jar.Notify.ImportPackage";
        private const string ImportCompileErrorKey = "net.maaaaa.marshmallow-jar.Notify.ImportCompileError";
        private static bool _importActive;
        private static double _idleSince = -1;

        static EditorActivityWatcher()
        {
            // NOTE: コンパイル後のドメインリロードでも開始時刻と結果を引き継ぎ、一連の処理として知らせる。
            _importActive = SessionState.GetBool(ImportActiveKey, false);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssetDatabase.importPackageStarted += OnImportPackageStarted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;
            AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
            EditorApplication.update += OnUpdate;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (MarshmallowJarToggles.IsOn(MarshmallowJarToggle.CompletionNotifications))
                    {
                        SessionState.SetFloat(PlayModeStartKey, (float)EditorApplication.timeSinceStartup);
                    }
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    float start = SessionState.GetFloat(PlayModeStartKey, -1);
                    SessionState.EraseFloat(PlayModeStartKey);
                    double elapsed = EditorApplication.timeSinceStartup - start;
                    if (start >= 0 && elapsed >= MarshmallowJarNotifier.MinSeconds)
                    {
                        MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.PlayMode, true, "Play モード",
                            "Play モードに入りました（" + MarshmallowJarNotifier.FormatElapsed(elapsed) + "）");
                    }
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    SessionState.EraseFloat(PlayModeStartKey);
                    break;
            }
        }

        private static void Begin(string packageName)
        {
            if (_importActive || !MarshmallowJarToggles.IsOn(MarshmallowJarToggle.CompletionNotifications))
            {
                return;
            }
            _importActive = true;
            SessionState.SetBool(ImportActiveKey, true);
            SessionState.SetFloat(ImportStartKey, (float)EditorApplication.timeSinceStartup);
            SessionState.SetString(ImportPackageKey, packageName);
            SessionState.SetBool(ImportCompileErrorKey, false);
            _idleSince = -1;
        }

        private static void OnImportPackageStarted(string name)
        {
            // NOTE: Unity はパッケージのフルパスを渡す。通知にユーザー名を含むローカルのパスを出さないため、ファイル名だけにする。
            name = Path.GetFileName(name);
            Begin(name);
            if (_importActive && string.IsNullOrEmpty(SessionState.GetString(ImportPackageKey, string.Empty)))
            {
                SessionState.SetString(ImportPackageKey, name);
            }
        }

        private static void OnCompilationStarted(object context)
        {
            Begin(string.Empty);
        }

        private static void OnImportPackageCompleted(string name)
        {
            // NOTE: 続くコンパイルとリロードも含めるため、完了は静かになった後に判定する。
        }

        private static void OnImportPackageFailed(string name, string error)
        {
            if (!_importActive)
            {
                return;
            }
            MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.Import, false, "インポート失敗",
                "パッケージ「" + Path.GetFileName(name) + "」のインポートに失敗しました: " + error);
            End();
        }

        private static void OnImportPackageCancelled(string name)
        {
            if (_importActive)
            {
                End();
            }
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (!_importActive)
            {
                return;
            }
            foreach (CompilerMessage message in messages)
            {
                if (message.type == CompilerMessageType.Error)
                {
                    SessionState.SetBool(ImportCompileErrorKey, true);
                    return;
                }
            }
        }

        private static void OnUpdate()
        {
            if (!_importActive)
            {
                return;
            }
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                _idleSince = -1;
                return;
            }
            double now = EditorApplication.timeSinceStartup;
            if (_idleSince < 0)
            {
                _idleSince = now;
            }
            if (now - _idleSince >= 1.0)
            {
                Finish();
            }
        }

        private static void Finish()
        {
            double elapsed = EditorApplication.timeSinceStartup - SessionState.GetFloat(ImportStartKey, 0);
            string name = SessionState.GetString(ImportPackageKey, string.Empty);
            bool error = SessionState.GetBool(ImportCompileErrorKey, false);
            End();
            if (elapsed < MarshmallowJarNotifier.MinSeconds)
            {
                return;
            }
            string duration = MarshmallowJarNotifier.FormatElapsed(elapsed);
            string title;
            string message;
            if (error)
            {
                title = "コンパイルエラー";
                message = (string.IsNullOrEmpty(name) ? string.Empty : "パッケージ「" + name + "」のインポート後、") +
                    "スクリプトのコンパイルでエラーが出ました（" + duration + "）";
            }
            else if (!string.IsNullOrEmpty(name))
            {
                title = "インポート完了";
                message = "パッケージ「" + name + "」のインポートが終わりました（" + duration + "）";
            }
            else
            {
                title = "コンパイル完了";
                message = "スクリプトのコンパイルが終わりました（" + duration + "）";
            }
            MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.Import, !error, title, message);
        }

        private static void End()
        {
            _importActive = false;
            SessionState.EraseBool(ImportActiveKey);
            SessionState.EraseFloat(ImportStartKey);
            SessionState.EraseString(ImportPackageKey);
            SessionState.EraseBool(ImportCompileErrorKey);
            _idleSince = -1;
        }
    }
}
