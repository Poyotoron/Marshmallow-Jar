using System;
using Poyo.MarshmallowJar.Editor;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3A.Editor;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;

namespace Poyo.MarshmallowJar.VRChat.Editor
{
    [InitializeOnLoad]
    internal static class SdkAutoFill
    {
        private const double PollInterval = 0.5;
        private static double _nextPollTime;
        private static VRCSdkControlPanelAvatarBuilder _builder;

        // NOTE: 入力値はアバターごとに保存されないため、別アバターの自動入力が残っている場合だけ入れ直す。
        private static int _filledAvatarId;
        private static string _filledName;
        private static string _filledThumbPath;

        static SdkAutoFill()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now < _nextPollTime)
            {
                return;
            }

            _nextPollTime = now + PollInterval;
            if (!MarshmallowJarSettings.instance.vrcSdkAutoFill || VRCSdkControlPanel.window == null)
            {
                Unsubscribe();
                return;
            }

            if (!VRCSdkControlPanel.TryGetBuilder<IVRCSdkAvatarBuilderApi>(out IVRCSdkAvatarBuilderApi api))
            {
                return;
            }

            VRCSdkControlPanelAvatarBuilder builder = api as VRCSdkControlPanelAvatarBuilder;
            if (builder == null || ReferenceEquals(builder, _builder))
            {
                return;
            }

            Unsubscribe();
            builder.ContentInfoLoaded += OnContentInfoLoaded;
            _builder = builder;
            GameObject selected = builder.SelectedAvatar;
            if (selected != null)
            {
                ScheduleFill(selected);
            }
        }

        private static void Unsubscribe()
        {
            if (_builder != null)
            {
                _builder.ContentInfoLoaded -= OnContentInfoLoaded;
                _builder = null;
            }
        }

        private static void OnContentInfoLoaded(object sender, object payload)
        {
            if (payload is ValueTuple<GameObject, VRCAvatar, string> info && info.Item1 != null)
            {
                ScheduleFill(info.Item1);
            }
        }

        private static void ScheduleFill(GameObject avatar)
        {
            // NOTE: このイベントはビルダーの非同期処理の途中で発火する。ここで選び直すと読み込みが入れ子になるため、次のフレームに回す。
            EditorApplication.delayCall += () => TryFill(avatar);
        }

        private static void TryFill(GameObject avatar)
        {
            if (!MarshmallowJarSettings.instance.vrcSdkAutoFill || avatar == null || _builder == null ||
                _builder.BuildState != SdkBuildState.Idle || _builder.UploadState != SdkUploadState.Idle ||
                EditorApplication.isPlayingOrWillChangePlaymode || _builder.SelectedAvatar != avatar ||
                !avatar.TryGetComponent(out PipelineManager pipeline) || !string.IsNullOrWhiteSpace(pipeline.blueprintId))
            {
                return;
            }

            int avatarId = avatar.GetInstanceID();
            string currentName = AvatarBuilderSessionState.AvatarName;
            string currentThumb = AvatarBuilderSessionState.AvatarThumbPath;
            bool nameEmpty = string.IsNullOrWhiteSpace(currentName) ||
                (avatarId != _filledAvatarId && currentName == _filledName);
            bool thumbEmpty = string.IsNullOrWhiteSpace(currentThumb) ||
                (avatarId != _filledAvatarId && currentThumb == _filledThumbPath);
            if (!nameEmpty && !thumbEmpty)
            {
                return;
            }

            bool changed = false;
            if (nameEmpty)
            {
                string name = avatar.name.Trim();
                // NOTE: 空の名前を再入力しても空のままなので、再読み込みの繰り返しを防ぐ。
                if (!string.IsNullOrWhiteSpace(name))
                {
                    AvatarBuilderSessionState.AvatarName = name;
                    _filledName = name;
                    changed = true;
                }
            }

            if (thumbEmpty)
            {
                string path = SceneViewThumbnailCapture.Capture();
                if (path != null)
                {
                    AvatarBuilderSessionState.AvatarThumbPath = path;
                    _filledThumbPath = path;
                    changed = true;
                }
            }

            if (changed)
            {
                _filledAvatarId = avatarId;
                _builder.SelectAvatar(avatar);
            }
        }
    }
}
