using System;
using System.Collections.Generic;

namespace Poyo.MarshmallowJar.Editor
{
    public static class MarshmallowJarToggles
    {
        private static readonly MarshmallowJarToggle[] AllToggles =
            (MarshmallowJarToggle[])Enum.GetValues(typeof(MarshmallowJarToggle));
        internal static IReadOnlyList<MarshmallowJarToggle> All => AllToggles;
        private static bool[] _resolved;

        public static bool IsOn(MarshmallowJarToggle toggle)
        {
            if (_resolved == null)
            {
                // NOTE: Hierarchy などの全行コールバックから呼ばれるため、行ごとのリスト検索を避ける。
                var resolved = new bool[AllToggles.Length];
                foreach (MarshmallowJarToggle item in AllToggles)
                {
                    resolved[(int)item] = TryGetOverride(item, out bool value) ? value : GetGlobal(item);
                }
                _resolved = resolved;
            }
            return _resolved[(int)toggle];
        }

        internal static bool GetInitialValue(MarshmallowJarToggle toggle)
        {
            switch (toggle)
            {
                case MarshmallowJarToggle.NotifyUploadSound:
                case MarshmallowJarToggle.NotifyUploadWebhook:
                case MarshmallowJarToggle.NotifyBatchUploadSound:
                case MarshmallowJarToggle.NotifyBatchUploadWebhook:
                case MarshmallowJarToggle.NotifyOutfitConversionSound:
                case MarshmallowJarToggle.NotifyOutfitConversionWebhook:
                case MarshmallowJarToggle.NotifyPlayModeSound:
                case MarshmallowJarToggle.NotifyImportSound:
                    return true;
                case MarshmallowJarToggle.HierarchyToggles:
                case MarshmallowJarToggle.StripVariantSuffix:
                case MarshmallowJarToggle.SceneSetup:
                case MarshmallowJarToggle.VrcSdkAutoFill:
                case MarshmallowJarToggle.InspectorNewWindowButton:
                case MarshmallowJarToggle.InspectorCopyToButton:
                case MarshmallowJarToggle.IrodoriColorizer:
                case MarshmallowJarToggle.CompletionNotifications:
                case MarshmallowJarToggle.NotifyPlayModeWebhook:
                case MarshmallowJarToggle.NotifyImportWebhook:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(toggle));
            }
        }

        internal static bool GetGlobal(MarshmallowJarToggle toggle)
        {
            return MarshmallowJarGlobalSettings.Instance.TryGetToggle(toggle, out bool value)
                ? value : GetInitialValue(toggle);
        }

        internal static void SetGlobal(MarshmallowJarToggle toggle, bool value)
        {
            MarshmallowJarGlobalSettings.Instance.SetToggle(toggle, value);
            MarshmallowJarGlobalSettings.Instance.Save();
        }

        private static bool TryGetOverride(MarshmallowJarToggle toggle, out bool value)
        {
            string key = toggle.ToString();
            foreach (MarshmallowJarToggleValue entry in MarshmallowJarSettings.instance.GetToggleOverrides())
            {
                if (entry != null && entry.key == key)
                {
                    value = entry.value;
                    return true;
                }
            }
            value = false;
            return false;
        }

        internal static bool HasOverride(MarshmallowJarToggle toggle)
        {
            return TryGetOverride(toggle, out _);
        }

        internal static void SetOverride(MarshmallowJarToggle toggle, bool value)
        {
            List<MarshmallowJarToggleValue> entries = MarshmallowJarSettings.instance.GetToggleOverrides();
            string key = toggle.ToString();
            MarshmallowJarToggleValue entry = entries.Find(item => item != null && item.key == key);
            if (entry == null)
            {
                entries.Add(new MarshmallowJarToggleValue { key = key, value = value });
            }
            else
            {
                entry.value = value;
            }
            MarshmallowJarSettings.instance.SaveChanges();
            Invalidate();
        }

        internal static void ClearOverride(MarshmallowJarToggle toggle)
        {
            string key = toggle.ToString();
            MarshmallowJarSettings.instance.GetToggleOverrides().RemoveAll(item => item != null && item.key == key);
            MarshmallowJarSettings.instance.SaveChanges();
            Invalidate();
        }

        internal static void Invalidate()
        {
            _resolved = null;
        }
    }
}
