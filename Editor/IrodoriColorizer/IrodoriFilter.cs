using System;
using System.Collections.Generic;
using UnityEditor;

namespace Poyo.MarshmallowJar.IrodoriColorizer.Editor
{
    internal static class IrodoriFilter
    {
        private const string SessionKey = "MarshmallowJar.IrodoriColorizer.Filter";
        private static readonly HashSet<string> SelectedLabelIds = new HashSet<string>(StringComparer.Ordinal);
        private static bool _loaded;
        private static HashSet<string> _visibleGuids;

        internal static bool IsActive
        {
            get
            {
                EnsureLoaded();
                return SelectedLabelIds.Count > 0;
            }
        }

        internal static bool IsSelected(string labelId)
        {
            EnsureLoaded();
            return SelectedLabelIds.Contains(labelId);
        }

        internal static void SetSelected(string labelId, bool selected)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(labelId))
            {
                return;
            }

            if (selected)
            {
                SelectedLabelIds.Add(labelId);
            }
            else
            {
                SelectedLabelIds.Remove(labelId);
            }

            Save();
            Invalidate();
            EditorApplication.RepaintProjectWindow();
        }

        internal static void Clear()
        {
            EnsureLoaded();
            SelectedLabelIds.Clear();
            Save();
            Invalidate();
            EditorApplication.RepaintProjectWindow();
        }

        internal static void Invalidate()
        {
            _visibleGuids = null;
        }

        internal static bool IsVisible(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return true;
            }

            EnsureLoaded();
            if (_visibleGuids == null)
            {
                BuildVisibleGuids();
            }

            return _visibleGuids.Contains(guid);
        }

        private static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            string saved = SessionState.GetString(SessionKey, string.Empty);
            string[] ids = saved.Split('|');
            for (int i = 0; i < ids.Length; i++)
            {
                if (!string.IsNullOrEmpty(ids[i]))
                {
                    SelectedLabelIds.Add(ids[i]);
                }
            }
        }

        private static void Save()
        {
            // NOTE: 絞り込みは一時的な表示なので、設定ファイルには残さずエディタのセッション内だけで保つ。
            SessionState.SetString(SessionKey, string.Join("|", SelectedLabelIds));
        }

        private static void BuildVisibleGuids()
        {
            _visibleGuids = new HashSet<string>(StringComparer.Ordinal);
            List<IrodoriAssignment> assignments = IrodoriSettings.Instance.projectAssignments;
            if (assignments == null)
            {
                return;
            }

            for (int i = 0; i < assignments.Count; i++)
            {
                IrodoriAssignment assignment = assignments[i];
                if (assignment == null || string.IsNullOrEmpty(assignment.key) || !SelectedLabelIds.Contains(assignment.labelId))
                {
                    continue;
                }

                string path = AssetDatabase.GUIDToAssetPath(assignment.key);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                _visibleGuids.Add(assignment.key);
                // NOTE: 色を付けたフォルダの中身と、そこへたどる親フォルダを明るく保つと目的のアセットまで辿れる。
                string parent = path;
                int slash = parent.LastIndexOf('/');
                while (slash >= 0)
                {
                    parent = parent.Substring(0, slash);
                    string parentGuid = AssetDatabase.AssetPathToGUID(parent);
                    if (!string.IsNullOrEmpty(parentGuid))
                    {
                        _visibleGuids.Add(parentGuid);
                    }

                    slash = parent.LastIndexOf('/');
                }

                if (AssetDatabase.IsValidFolder(path))
                {
                    string[] descendants = AssetDatabase.FindAssets(string.Empty, new[] { path });
                    for (int j = 0; j < descendants.Length; j++)
                    {
                        _visibleGuids.Add(descendants[j]);
                    }
                }
            }
        }
    }
}
