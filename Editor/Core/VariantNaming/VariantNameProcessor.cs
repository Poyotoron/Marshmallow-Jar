using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    internal sealed class VariantNameProcessor : AssetPostprocessor
    {
        private const string VariantSuffix = " Variant";
        private const string PrefabExtension = ".prefab";
        private const string MetaExtension = ".meta";

        private static readonly HashSet<string> CreatedPaths = new HashSet<string>(StringComparer.Ordinal);
        private static readonly List<string> PendingRenames = new List<string>();
        private static bool _renameScheduled;

        internal static void NotifyCreating(string path)
        {
            if (!MarshmallowJarSettings.instance.stripVariantSuffix)
            {
                return;
            }

            if (path.EndsWith(MetaExtension, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - MetaExtension.Length);
            }

            if (!path.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileNameWithoutExtension(path).EndsWith(VariantSuffix, StringComparison.Ordinal))
            {
                return;
            }

            CreatedPaths.Add(path);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (CreatedPaths.Count == 0)
            {
                return;
            }

            bool added = false;
            foreach (string path in importedAssets)
            {
                if (CreatedPaths.Remove(path))
                {
                    PendingRenames.Add(path);
                    added = true;
                }
            }

            if (added && !_renameScheduled)
            {
                // NOTE: インポート中の改名はインポートを再入させるため、処理が終わったあとに行う。
                _renameScheduled = true;
                EditorApplication.delayCall += ProcessPendingRenames;
            }
        }

        private static void ProcessPendingRenames()
        {
            _renameScheduled = false;
            string[] paths = PendingRenames.ToArray();
            PendingRenames.Clear();
            foreach (string path in paths)
            {
                TryStripSuffix(path);
            }
        }

        private static void TryStripSuffix(string path)
        {
            if (!MarshmallowJarSettings.instance.stripVariantSuffix)
            {
                return;
            }

            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root == null || PrefabUtility.GetPrefabAssetType(root) != PrefabAssetType.Variant)
            {
                return;
            }

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(root);
            if (source == null)
            {
                return;
            }

            string baseName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(source));
            if (string.IsNullOrEmpty(baseName) ||
                !string.Equals(Path.GetFileNameWithoutExtension(path), baseName + VariantSuffix, StringComparison.Ordinal))
            {
                return;
            }

            string target = Path.GetDirectoryName(path).Replace('\\', '/') + "/" + baseName + PrefabExtension;
            if (AssetDatabase.LoadMainAssetAtPath(target) != null || File.Exists(target))
            {
                return;
            }

            string error = AssetDatabase.RenameAsset(path, baseName);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning(MarshmallowJarInfo.LogPrefix + "Prefab Variant の名前を変更できませんでした: " + error);
            }
        }
    }
}
