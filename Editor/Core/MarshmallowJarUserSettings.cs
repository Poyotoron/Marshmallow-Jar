using UnityEditor;

namespace Poyo.MarshmallowJar.Editor
{
    // NOTE: UserSettings はプロジェクトごと・利用者ごとの保存先で、Unity の標準の .gitignore でリポジトリから外れる。
    //       秘密の Webhook URL と PC ごとに違う音のパスを置くため、ProjectSettings には保存しない。
    [FilePath("UserSettings/MarshmallowJar.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class MarshmallowJarUserSettings : ScriptableSingleton<MarshmallowJarUserSettings>
    {
        public int schemaVersion = 1;
        public bool overrideNotifyMinSeconds = false;
        public int notifyMinSeconds = MarshmallowJarGlobalSettings.DefaultNotifyMinSeconds;
        public bool overrideSuccessSoundPath = false;
        public string successSoundPath = string.Empty;
        public bool overrideFailureSoundPath = false;
        public string failureSoundPath = string.Empty;
        public bool overrideWebhookUrl = false;
        public string webhookUrl = string.Empty;
        public bool overrideWebhookTemplate = false;
        public string webhookTemplate = MarshmallowJarGlobalSettings.DiscordWebhookTemplate;

        internal void SaveChanges()
        {
            Save(true);
        }
    }
}
