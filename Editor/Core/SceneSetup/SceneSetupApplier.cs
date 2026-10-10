using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Poyo.MarshmallowJar.Editor
{
    internal static class SceneSetupApplier
    {
        internal static void Apply(Scene scene)
        {
            // NOTE: RenderSettings はアクティブなシーンの値なので、別シーンの光源と組み合わせて変更しない。
            if (!scene.IsValid() || !scene.isLoaded || scene != SceneManager.GetActiveScene())
            {
                return;
            }

            // NOTE: RenderSettings の変更を Undo に載せる公開 API は無いため、光源の変更だけを記録する。
            RenderSettings.ambientMode = SceneSetupRules.AmbientModeValue;
            RenderSettings.ambientSkyColor = SceneSetupRules.AmbientSkyColor;
            RenderSettings.ambientEquatorColor = SceneSetupRules.AmbientEquatorColor;
            RenderSettings.ambientGroundColor = SceneSetupRules.AmbientGroundColor;
            RenderSettings.subtractiveShadowColor = SceneSetupRules.ShadowColor;
            Light light = SceneSetupRules.FindDirectionalLight(scene);
            if (light != null)
            {
                Undo.RecordObjects(new Object[] { light, light.transform }, "推奨ライティングの適用");
                light.transform.rotation = SceneSetupRules.LightRotation;
                light.color = SceneSetupRules.LightColor;
                light.intensity = SceneSetupRules.LightIntensity;
                if (PrefabUtility.IsPartOfPrefabInstance(light))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(light);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(light.transform);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            ApplySceneViewCamera();
        }

        internal static void ApplySceneViewCamera()
        {
            foreach (SceneView view in SceneView.sceneViews)
            {
                SceneView.CameraSettings settings = view.cameraSettings;
                settings.fieldOfView = SceneSetupRules.SceneViewFieldOfView;
                view.cameraSettings = settings;
                view.Repaint();
            }
        }
    }
}
