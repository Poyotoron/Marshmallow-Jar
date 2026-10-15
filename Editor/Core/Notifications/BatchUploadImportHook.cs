#if MARSHMALLOW_JAR_HAS_CAU
using System;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    // NOTE: Continuous Avatar Uploader が使う SaveAssetIfDirty は保存前のコールバックを呼ばないが、保存中に再インポートするため、ここで保存したばかりの値を読める。
    internal sealed class BatchUploadImportHook : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!SessionState.GetBool(MarshmallowJarNotifier.BatchRunningSessionKey, false))
            {
                return;
            }
            foreach (string path in importedAssets)
            {
                if (!string.Equals(path, BatchUploadWatcher.ProgressAssetPath, StringComparison.Ordinal))
                {
                    continue;
                }
                try
                {
                    BatchUploadWatcher.OnProgressSaved();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
                break;
            }
        }
    }
}
#endif
