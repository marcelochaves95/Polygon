using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProceduralTerrain
{
    public static class Noise
    {
        public static void CreateNoiseTexture(int textureResolution)
        {
            int octaves = 8;
            Texture2D noiseTexture = new Texture2D(textureResolution, textureResolution);
            Color[] pixes = new Color[textureResolution * textureResolution];
            float xOriginal = Random.value * 100000f;
            float yOriginal = Random.value * 100000f;
            float height = 0f;

            while (height < noiseTexture.height)
            {
                float width = 0f;
                while (width < noiseTexture.width)
                {
                    float sample = OctavesNoise2D(xOriginal + width / noiseTexture.width, yOriginal + height / noiseTexture.height, octaves, 1f, 0.75f);
                    pixes[(int) height * noiseTexture.width + (int) width] = new Color(sample, sample, sample);
                    width++;
                }
                height++;
            }

            noiseTexture.SetPixels(pixes);
            noiseTexture.Apply();
            byte[] bytes = noiseTexture.EncodeToPNG();
            SaveFile(bytes);
        }

        private static float OctavesNoise2D(float x, float y, int octaves, float frequency, float amplitude)
        {
            float gain = 1f;
            float sum = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum +=  Mathf.PerlinNoise(x * gain / frequency, y * gain / frequency) * amplitude / gain;
                gain *= 2f;
            }

            return sum;
        }

        private static void SaveFile(byte[] bytes)
        {
            string pathFile = string.Concat(Application.dataPath, "/Textures/Heightmap.png");
            Debug.Log(string.Concat("Creating Texture Heightmap:", pathFile));
            File.WriteAllBytes(pathFile, bytes);
            AssetDatabase.ImportAsset("Assets/Textures/Heightmap.png");
        }
    }
}
