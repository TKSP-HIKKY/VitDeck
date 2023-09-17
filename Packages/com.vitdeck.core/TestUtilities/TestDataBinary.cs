using UnityEditor;
using System.Linq;
using System.IO;
using System;

namespace VitDeck.TestUtilities
{
    public static class TestDataBinary
    {
        private const string testDataLabel = "VitDeck.TestData";
        private const string testDataSearchFilter = "l:" + testDataLabel + " ";

        public static void WriteAudio(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Audio t:AudioClip").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            System.IO.File.Copy(dataPath, path);
        }

        public static void WriteLightingData(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "LightingData").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            System.IO.File.Copy(dataPath, path);
        }

        public static void WriteObj(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Mesh").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            System.IO.File.Copy(dataPath, path);
        }

        public static void WriteFbx(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Model").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            System.IO.File.Copy(dataPath, path);
        }

        public static void WriteText(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Text").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            System.IO.File.Copy(dataPath, path);
        }

        public static void WriteImage(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Texture").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            System.IO.File.Copy(dataPath, path);
        }

        public static void WriteShader(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            System.IO.File.WriteAllText(path,
                "Shader\"" + name + "\"{SubShader{Pass{Material{Diffuse(1,.5,.5,1)}Lighting On}}}");
        }

        public static void WriteSprite(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Sprite").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            AssetDatabase.CopyAsset(dataPath, path);
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), new string[] { });
        }

        public static void WriteCubeMap(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "ReflectionProbe-0").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            AssetDatabase.CopyAsset(dataPath, path);
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), new string[] { });
        }

        public static void WriteScene(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Scene").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            AssetDatabase.CopyAsset(dataPath, path);
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), new string[] { });
        }

        public static void WriteAnimatorController(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test AnimatorController").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            AssetDatabase.CopyAsset(dataPath, path);
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), new string[] { });
        }

        public static void WriteAudioMixer(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Audio Mixer").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            AssetDatabase.CopyAsset(dataPath, path);
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), new string[] { });
        }

        public static void WriteComputeShader(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test ComputeShader").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            System.IO.File.Copy(dataPath, path);
        }

        public static void WriteVideo(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Movie").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            AssetDatabase.CopyAsset(dataPath, path);
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), new string[] { });
        }

        public static void WriteAvatarMask(string path)
        {
            var dataGUID = AssetDatabase.FindAssets(testDataSearchFilter + "Test Avatar Mask").FirstOrDefault();
            var dataPath = AssetDatabase.GUIDToAssetPath(dataGUID);
            AssetDatabase.CopyAsset(dataPath, path);
            AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), new string[] { });
        }
    }
}
