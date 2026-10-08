using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.IrodoriColorizer.Editor
{
    internal static class IrodoriSettingsMigration
    {
        private static readonly Regex ScriptReferencePattern =
            new Regex(@"m_Script: \{fileID: 11500000, guid: [0-9a-f]{32}, type: 3\}");
        private static bool _checked;

        internal static void ImportStandaloneSettings()
        {
            if (_checked)
            {
                return;
            }

            try
            {
                if (File.Exists(IrodoriInfo.SettingsFilePath) ||
                    !File.Exists(IrodoriInfo.StandaloneSettingsFilePath))
                {
                    _checked = true;
                    return;
                }

                string guid = AssetDatabase.AssetPathToGUID(IrodoriInfo.SettingsScriptPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning("Irodori Colorizer: 単体版の設定を引き継げませんでした。設定スクリプトが見つかりません。");
                    return;
                }

                string contents = File.ReadAllText(IrodoriInfo.StandaloneSettingsFilePath);
                if (!ScriptReferencePattern.IsMatch(contents))
                {
                    Debug.LogWarning("Irodori Colorizer: 単体版の設定ファイルの形式を読み取れなかったため、引き継ぎを中止しました。");
                    _checked = true;
                    return;
                }

                string migrated = ScriptReferencePattern.Replace(contents,
                    "m_Script: {fileID: 11500000, guid: " + guid + ", type: 3}", 1);
                File.WriteAllText(IrodoriInfo.SettingsFilePath, migrated);
                Debug.Log("Irodori Colorizer: 単体版の設定を引き継ぎました。");
                _checked = true;
            }
            catch (IOException exception)
            {
                Debug.LogException(exception);
                _checked = true;
            }
            catch (UnauthorizedAccessException exception)
            {
                Debug.LogException(exception);
                _checked = true;
            }
        }
    }
}
