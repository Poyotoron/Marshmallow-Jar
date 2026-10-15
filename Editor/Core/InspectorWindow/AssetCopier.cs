using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class AssetCopier
    {
        private static int _cachedId;
        private static bool _cachedResult;

        internal static bool CanCopy(Object target)
        {
            if (target == null)
            {
                return false;
            }
            int id = target.GetInstanceID();
            // NOTE: Inspector のヘッダーは描画のたびに呼ばれるため、同じ対象の判定を繰り返さない。
            if (_cachedId == id)
            {
                return _cachedResult;
            }
            string path = AssetDatabase.GetAssetPath(target);
            _cachedId = id;
            _cachedResult = EditorUtility.IsPersistent(target) && AssetDatabase.IsMainAsset(target) &&
                (path.StartsWith("Assets/", StringComparison.Ordinal) || path.StartsWith("Packages/", StringComparison.Ordinal)) &&
                !AssetDatabase.IsValidFolder(path);
            return _cachedResult;
        }

        internal static bool TryCopy(string sourcePath, string destinationPath, out string message)
        {
            message = null;
            if (string.IsNullOrEmpty(destinationPath))
            {
                return false;
            }
            if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
            {
                message = "元と同じ場所には保存できません。";
                return false;
            }
            // NOTE: 上書きでアセットへの参照との対応が崩れうるため、既にある名前には書かない。
            if (AssetDatabase.LoadMainAssetAtPath(destinationPath) != null || File.Exists(destinationPath))
            {
                message = "「" + destinationPath + "」は既にあるため、コピーしませんでした。上書きはしません。別の名前を選んでください。";
                return false;
            }
            if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
            {
                message = "コピーできませんでした: " + sourcePath + " → " + destinationPath;
                return false;
            }
            message = "コピーを保存しました: " + destinationPath;
            return true;
        }

        internal static void CopyWithDialog(Object target)
        {
            string sourcePath = AssetDatabase.GetAssetPath(target);
            string folder = sourcePath.StartsWith("Assets/", StringComparison.Ordinal) ?
                Path.GetDirectoryName(sourcePath).Replace('\\', '/') : "Assets";
            string name = Path.GetFileName(AssetDatabase.GenerateUniqueAssetPath(folder + "/" + Path.GetFileName(sourcePath)));
            string extension = Path.GetExtension(sourcePath).TrimStart('.');
            // NOTE: 利用者がボタンを押した直後の操作なので、保存ダイアログと結果の通知はモーダルでよい。
            string destinationPath = EditorUtility.SaveFilePanelInProject("コピーを保存", name, extension,
                "コピーの保存先と名前を選んでください。", folder);
            if (TryCopy(sourcePath, destinationPath, out string message))
            {
                Debug.Log(MarshmallowJarInfo.LogPrefix + message);
                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(destinationPath));
            }
            else if (message != null)
            {
                Debug.LogWarning(MarshmallowJarInfo.LogPrefix + message);
                EditorUtility.DisplayDialog("コピーを保存", message, "OK");
            }
        }
    }
}
