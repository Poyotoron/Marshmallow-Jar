using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class NotificationSound
    {
#if UNITY_EDITOR_WIN
        // NOTE: 非同期再生なら、モーダルダイアログでメインスレッドが止まっていても鳴り、呼び出し元を待たせない。
        private const uint SndAsync = 0x0001;
        private const uint SndNoDefault = 0x0002;
        private const uint SndFilename = 0x00020000;

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);
#endif

        internal static string ResolvePath(bool success, string configuredPath)
        {
            if (!string.IsNullOrEmpty(configuredPath))
            {
                return configuredPath;
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media",
                success ? "Windows Notify System Generic.wav" : "Windows Critical Stop.wav");
        }

        internal static void Play(bool success)
        {
            PlayPath(success, success ? MarshmallowJarNotificationSettings.SuccessSoundPath : MarshmallowJarNotificationSettings.FailureSoundPath);
        }

        internal static void PlayPath(bool success, string configuredPath)
        {
#if UNITY_EDITOR_WIN
            string path = ResolvePath(success, configuredPath);
            if (File.Exists(path) && PlaySound(path, IntPtr.Zero, SndFilename | SndAsync | SndNoDefault))
            {
                return;
            }
#endif
            // NOTE: PlaySound は Windows の API のため、ほかの OS ではビープ音にする。
            EditorApplication.Beep();
        }
    }
}
