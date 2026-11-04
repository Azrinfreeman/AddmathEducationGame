using System;
using System.Reflection;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FunMath.Editor
{
    public static class AdventurePreview
    {
        [MenuItem("Fun Math/UI/Preview/Portrait 1080 x 1920")]
        public static void Portrait() { SetResolution(1080, 1920, "Adventure portrait"); }
        [MenuItem("Fun Math/UI/Preview/Landscape 1920 x 1080")]
        public static void Landscape() { SetResolution(1920, 1080, "Adventure landscape"); }

        /// <summary>Exports the actual Game view, including overlay canvases.</summary>
        public static void CaptureGameView(string path)
        {
            var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var view = EditorWindow.GetWindow(type);
            var texture = (RenderTexture)type.GetField("m_RenderTexture", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view);
            if (!texture) throw new InvalidOperationException("Render the Game view before capturing it.");
            var previous = RenderTexture.active;
            var image = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                if (SystemInfo.graphicsUVStartsAtTop)
                {
                    var pixels = image.GetPixels32();
                    var flipped = new Color32[pixels.Length];
                    for (int y = 0; y < texture.height; y++)
                        Array.Copy(pixels, y * texture.width, flipped, (texture.height - y - 1) * texture.width, texture.width);
                    image.SetPixels32(flipped);
                }
                image.Apply();
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        // Unity has no public API for adding a Game view resolution. Keep reflection editor-only.
        private static void SetResolution(int width, int height, string label)
        {
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance").GetValue(null);
            var group = sizesType.GetProperty("currentGroup").GetValue(sizes);
            var groupType = group.GetType();
            var labels = (string[])groupType.GetMethod("GetDisplayTexts").Invoke(group, null);
            int index = Array.FindIndex(labels, text => text.Contains(label));
            if (index < 0)
            {
                var sizeType = assembly.GetType("UnityEditor.GameViewSize");
                var kind = Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"), "FixedResolution");
                var size = Activator.CreateInstance(sizeType, new object[] { kind, width, height, label });
                groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                index = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null) - 1;
            }
            var gameType = assembly.GetType("UnityEditor.GameView");
            var game = EditorWindow.GetWindow(gameType);
            gameType.GetProperty("selectedSizeIndex").SetValue(game, index);
            game.Focus(); game.Repaint();
        }
    }
}
