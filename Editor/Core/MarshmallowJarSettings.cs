using System.Collections.Generic;
using UnityEditor;

namespace Poyo.MarshmallowJar.Editor
{
    // NOTE: ProjectSettings に置くと Assets やビルド成果物を汚さず、プロジェクト単位で設定を持てる。
    [FilePath("ProjectSettings/MarshmallowJar.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class MarshmallowJarSettings : ScriptableSingleton<MarshmallowJarSettings>
    {
        public int schemaVersion = 1;
        public bool hierarchyToggles = true;
        public bool stripVariantSuffix = true;
        public bool sceneSetup = true;
        public bool vrcSdkAutoFill = true;
        public bool inspectorNewWindowButton = true;
        public List<string> sceneSetupIgnoredSceneGuids = new List<string>();
        public float hierarchyToggleRightOffset = DefaultHierarchyToggleRightOffset;

        // NOTE: 行の右端には Prefab インスタンスを開く「>」ボタン（16px）と、他の拡張が置くボタンが並ぶ。
        //       既定ではそれらの分を空けて重ならない位置にする。
        public const float DefaultHierarchyToggleRightOffset = 56f;
        public const float MaxHierarchyToggleRightOffset = 200f;

        public void SaveChanges()
        {
            Save(true);
        }

        internal List<string> GetIgnoredSceneGuids()
        {
            if (sceneSetupIgnoredSceneGuids == null)
            {
                sceneSetupIgnoredSceneGuids = new List<string>();
            }

            return sceneSetupIgnoredSceneGuids;
        }
    }
}
