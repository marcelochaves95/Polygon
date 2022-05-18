using System.IO;
using UnityEngine;
using UnityEditor;

public class NoiseGeneratorEditor : EditorWindow
{
    private const string HEIGHTMAP_PATH = "Assets/Textures/Heightmap.png";

    private Texture2D _heightmap;
    private int _textureResolution = 1024;
    private int _octaves = 8;

    [MenuItem("Window/Noise Generator", priority = 99999)]
    private static void Init()
    {
        NoiseGeneratorEditor noiseGeneratorEditor = GetWindow<NoiseGeneratorEditor>();
        noiseGeneratorEditor.titleContent.text = "Noise Generator";
        noiseGeneratorEditor.Show();
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("TEXTURE SETTINGS", style);
            _octaves = EditorGUILayout.IntSlider("Number of Octaves:", _octaves, 0, 8);
            _textureResolution = EditorGUILayout.IntField("Texture Resolution:", _textureResolution);
            _heightmap = (Texture2D) EditorGUILayout.ObjectField("Select Heightmap:", _heightmap, typeof(Texture2D), true);
            if (!_heightmap)
            {
                LoadHeightmap();
            }

            if (GUILayout.Button("Generate Noise Texture"))
            {
                GenerateNoiseTexture(_textureResolution, _octaves);
            }
        }
    }

    private void GenerateNoiseTexture(int textureResolution, int octaves)
    {
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
        SaveFileHeightMap(bytes);
    }

    private float OctavesNoise2D(float x, float y, int octaves, float frequency, float amplitude)
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

    private void SaveFileHeightMap(byte[] bytes)
    {
        string assetPath = $"{Application.dataPath}/Textures/Heightmap.png";
        Debug.Log($"Creating Texture Heightmap: {assetPath}");
        File.WriteAllBytes(assetPath, bytes);
        AssetDatabase.ImportAsset(HEIGHTMAP_PATH);
        SetTextureImporterFormat();
    }

    private void SetTextureImporterFormat()
    {
        TextureImporter importer = AssetImporter.GetAtPath(HEIGHTMAP_PATH) as TextureImporter;
        if (importer)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(HEIGHTMAP_PATH);
            AssetDatabase.Refresh();
        }
    }

    private void LoadHeightmap()
    {
        _heightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(HEIGHTMAP_PATH);
    }
}
