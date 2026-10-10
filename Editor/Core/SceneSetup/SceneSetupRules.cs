using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class SceneSetupRules
    {
        // NOTE: 既定の環境光は青寄りで、そのまま色を合わせるとワールドではアバターが黄色寄りに見える。
        internal const AmbientMode AmbientModeValue = AmbientMode.Trilight;
        internal static readonly Color ShadowColor = new Color32(0xA0, 0xA0, 0xA0, 0xFF);
        // NOTE: Gradient の既定の 3 色は暗い青灰色で、アバターの下半分が青く沈む。
        //       無彩色にして色味の判断を妨げず、上から明るい順にして陰影は残す。
        internal static readonly Color AmbientSkyColor = new Color32(0xA0, 0xA0, 0xA0, 0xFF);
        internal static readonly Color AmbientEquatorColor = new Color32(0x80, 0x80, 0x80, 0xFF);
        internal static readonly Color AmbientGroundColor = new Color32(0x60, 0x60, 0x60, 0xFF);
        // NOTE: 既定の Directional Light は黄色い太陽光。白にして右前上からの向きに揃えると、影の付き方を判断しやすい。
        internal static readonly Quaternion LightRotation = Quaternion.Euler(40f, -145f, -20f);
        internal static readonly Color LightColor = Color.white;
        internal const float LightIntensity = 0.8f;
        // NOTE: 既定の視野角だと顔が歪んで見える。標準レンズ相当の画角にして、VR で見たときの印象に近づける。
        internal const float SceneViewFieldOfView = 40f;

        internal static Light FindDirectionalLight(Scene scene)
        {
            Light sun = RenderSettings.sun;
            if (sun != null && sun.type == LightType.Directional && sun.gameObject.scene == scene)
            {
                return sun;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Light light in root.GetComponentsInChildren<Light>(true))
                {
                    if (light.type == LightType.Directional)
                    {
                        return light;
                    }
                }
            }

            return null;
        }

        internal static bool IsCompliant(Scene scene)
        {
            if (RenderSettings.ambientMode != AmbientModeValue ||
                !ColorApproximately(RenderSettings.ambientSkyColor, AmbientSkyColor) ||
                !ColorApproximately(RenderSettings.ambientEquatorColor, AmbientEquatorColor) ||
                !ColorApproximately(RenderSettings.ambientGroundColor, AmbientGroundColor) ||
                !ColorApproximately(RenderSettings.subtractiveShadowColor, ShadowColor))
            {
                return false;
            }

            Light light = FindDirectionalLight(scene);
            return light == null ||
                (Quaternion.Angle(light.transform.rotation, LightRotation) <= 0.5f &&
                ColorApproximately(light.color, LightColor) &&
                Mathf.Abs(light.intensity - LightIntensity) <= 0.001f);
        }

        private static bool ColorApproximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) <= 0.005f &&
                Mathf.Abs(a.g - b.g) <= 0.005f &&
                Mathf.Abs(a.b - b.b) <= 0.005f;
        }
    }
}
