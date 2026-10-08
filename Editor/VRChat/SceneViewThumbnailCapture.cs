using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Poyo.MarshmallowJar.VRChat.Editor
{
    internal static class SceneViewThumbnailCapture
    {
        private const int Width = 1200;
        private const int Height = 900;

        internal static string Capture()
        {
            SceneView view = SceneView.lastActiveSceneView;
            if (view == null || view.camera == null)
            {
                return null;
            }

            GameObject cameraObject = null;
            Camera camera = null;
            RenderTexture target = null;
            Texture2D texture = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                cameraObject = EditorUtility.CreateGameObjectWithHideFlags(
                    "Marshmallow Jar Thumbnail Camera", HideFlags.HideAndDontSave, typeof(Camera));
                camera = cameraObject.GetComponent<Camera>();
                camera.CopyFrom(view.camera);
                // NOTE: CopyFrom は cameraType も写すため、Scene ビュー用の描画（グリッドやギズモ）に乗らないよう戻す。
                camera.cameraType = CameraType.Game;
                camera.enabled = false;
                camera.aspect = (float)Width / Height;
                target = RenderTexture.GetTemporary(Width, Height, 24);
                camera.targetTexture = target;
                camera.Render();

                RenderTexture.active = target;
                texture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                texture.Apply();
                RenderTexture.active = previous;

                string directory = Path.Combine(Path.GetTempPath(), "MarshmallowJar");
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, Guid.NewGuid().ToString("N") + "_thumbnail.png");
                File.WriteAllBytes(path, texture.EncodeToPNG());
                return path;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return null;
            }
            finally
            {
                // NOTE: 読み出しに失敗した場合も、エディタ全体で共有する描画先を元に戻す。
                RenderTexture.active = previous;
                if (camera != null)
                {
                    camera.targetTexture = null;
                }
                if (target != null)
                {
                    RenderTexture.ReleaseTemporary(target);
                }
                if (texture != null)
                {
                    Object.DestroyImmediate(texture);
                }
                if (cameraObject != null)
                {
                    Object.DestroyImmediate(cameraObject);
                }
            }
        }
    }
}
