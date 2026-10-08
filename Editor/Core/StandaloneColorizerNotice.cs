using UnityEditor;
using UnityEngine;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class StandaloneColorizerNotice
    {
        internal const string WindowWarning =
            "単体版の Irodori Colorizer が導入されているため、こちらの色付けを止めています。パッケージ管理ツールで単体版を削除してください。ラベルと設定は引き継がれます。";

#if MARSHMALLOW_JAR_HAS_STANDALONE_COLORIZER
        internal const bool IsStandaloneInstalled = true;
        private const string ShownKey = "MarshmallowJar.StandaloneColorizerNotice.Shown";
        private const string NoticeMessage =
            "単体版の Irodori Colorizer が導入されています。\n\n" +
            "Marshmallow Jar には同じ機能が含まれているため、単体版が入っているあいだは Marshmallow Jar 側の色付けを止めています。\n\n" +
            "パッケージ管理ツールで、このプロジェクトから「Irodori Colorizer」を削除してください。付けたラベルと設定はそのまま引き継がれます。";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (Application.isBatchMode || SessionState.GetBool(ShownKey, false))
            {
                return;
            }

            SessionState.SetBool(ShownKey, true);
            Debug.LogWarning(MarshmallowJarInfo.LogPrefix + NoticeMessage.Replace("\n\n", " "));
        }
#else
        internal const bool IsStandaloneInstalled = false;
#endif
    }
}
