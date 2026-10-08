namespace Poyo.MarshmallowJar.IrodoriColorizer.Editor
{
    /// <summary>パッケージ共通の定数。</summary>
    internal static class IrodoriInfo
    {
        internal const string DisplayName = "Irodori Colorizer";

        // NOTE: 色付けは Marshmallow Jar の一機能なので、右クリックメニューの親はパッケージ名に揃える。
        //       深さを変えると既存の利用者が項目を探せなくなるため、1 段のまま置き換える。
        internal const string AssetMenuRoot = "Assets/Marshmallow Jar/";
        internal const string GameObjectMenuRoot = "GameObject/Marshmallow Jar/";

        internal const string SettingsPath = "Project/Marshmallow Jar/Irodori Colorizer";

        internal const string SettingsFilePath = "ProjectSettings/MarshmallowJar.IrodoriColorizer.asset";
        internal const string StandaloneSettingsFilePath = "ProjectSettings/IrodoriColorizer.asset";
        internal const string SettingsScriptPath = "Packages/net.maaaaa.marshmallow-jar/Editor/IrodoriColorizer/IrodoriSettings.cs";
    }
}
