using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Poyo.MarshmallowJar.Editor
{
    internal sealed class OutfitConversionWatcher : AssetPostprocessor
    {
        // NOTE: もちふぃった～ は変換の最後に、この接頭辞の Prefab を新しく作る。
        private const string OutputPrefix = "finalized_";
        private const double FreshSeconds = 120.0;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!MarshmallowJarToggles.IsOn(MarshmallowJarToggle.CompletionNotifications))
            {
                return;
            }
            List<string> names = null;
            foreach (string path in importedAssets)
            {
                if (!path.StartsWith("Assets/", StringComparison.Ordinal) ||
                    !string.Equals(Path.GetExtension(path), ".prefab", StringComparison.OrdinalIgnoreCase) ||
                    !Path.GetFileName(path).StartsWith(OutputPrefix, StringComparison.Ordinal))
                {
                    continue;
                }
                // NOTE: 再インポートで古い出力に反応しないよう、作成時刻を見る。
                if ((DateTime.UtcNow - File.GetCreationTimeUtc(path)).TotalSeconds > FreshSeconds)
                {
                    continue;
                }
                if (names == null)
                {
                    names = new List<string>();
                }
                names.Add(Path.GetFileNameWithoutExtension(path));
            }
            if (names != null && names.Count > 0)
            {
                // NOTE: 変換直後のモーダルダイアログでメインスレッドが止まるため、後回しにせずその場で知らせる。
                MarshmallowJarNotifier.Notify(MarshmallowJarNotificationEvent.OutfitConversion, true, "衣装の変換完了",
                    "もちふぃった～ の変換が終わりました: " + string.Join("、", names));
            }
        }
    }
}
