namespace Poyo.MarshmallowJar.Editor
{
    /// <summary>パッケージ共通の定数。</summary>
    public static class MarshmallowJarInfo
    {
        public const string PackageId = "net.maaaaa.marshmallow-jar";
        public const string DisplayName = "Marshmallow Jar";
        public const string Version = "0.2.0";
        public const string GlobalSettingsKey = PackageId + ".GlobalSettings";
        public const string PreferencesPath = "Preferences/" + DisplayName;
        public const string PreferencesMenuPath = "Tools/" + DisplayName + "/Preferences…";
        public const int PreferencesMenuPriority = 1001;

        // NOTE: Tools/Marshmallow Jar は子メニューを持つフォルダにする。Unity は同じパスを
        //       メニュー項目とフォルダの両方には使えないため、設定を開く項目も子に置く。
        public const string MenuPath = "Tools/" + DisplayName + "/Project Settings…";
        public const int MenuPriority = 1000;
        public const string SettingsPath = "Project/" + DisplayName;

        public const string LogPrefix = "[Marshmallow Jar] ";
    }
}
