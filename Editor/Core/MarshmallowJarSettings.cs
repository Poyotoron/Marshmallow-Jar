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
        public float blendShapeSliderMin = DefaultBlendShapeSliderMin;
        public float blendShapeSliderMax = DefaultBlendShapeSliderMax;

        // NOTE: 行の右端には Prefab インスタンスを開く「>」ボタン（16px）と、他の拡張が置くボタンが並ぶ。
        //       既定ではそれらの分を空けて重ならない位置にする。
        public const float DefaultHierarchyToggleRightOffset = 56f;
        public const float MaxHierarchyToggleRightOffset = 200f;
        public const float DefaultBlendShapeSliderMin = 0f;
        public const float DefaultBlendShapeSliderMax = 100f;
        // NOTE: 下限と上限が近すぎるとスライダーが操作できないため、最低でもこの幅を保つ。
        public const float MinBlendShapeSliderSpan = 1f;

        internal void SetBlendShapeSliderRange(float min, float max, bool minEdited)
        {
            if (max - min < MinBlendShapeSliderSpan)
            {
                // NOTE: 入力した側を保ち、もう片方を動かすと範囲が逆転しても編集を続けられる。
                if (minEdited)
                {
                    max = min + MinBlendShapeSliderSpan;
                }
                else
                {
                    min = max - MinBlendShapeSliderSpan;
                }
            }

            blendShapeSliderMin = min;
            blendShapeSliderMax = max;
            SaveChanges();
        }

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
