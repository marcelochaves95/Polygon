using System;
using UnityEngine;
using UnityEditor;

namespace ProceduralTerrain.Editor
{
    public class TerrainEditor : EditorWindow
    {
        private EMeshType _meshType;
        private EShaderType _shaderType;

        private int _maxHeight;
        private int _xSize = 1;
        private int _ySize = 1;
        private int _textureResolution = 1024;
        private int _octaves = 8;

        private TerrainColor _terrainColor = new TerrainColor();
        private Texture2D heightmap;

        [MenuItem("Terrain/Create Terrain...")]
        private static void Init()
        {
            TerrainEditor window = GetWindow<TerrainEditor>();
            window.titleContent.text = nameof(TerrainEditor);
            window.Show();
        }

        private void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("TEXTURE SETTINGS", style);
                _octaves = EditorGUILayout.IntSlider("Number of Octaves:", _octaves, 0, 8);
                _textureResolution = EditorGUILayout.IntField("Texture Resolution:", _textureResolution);
                heightmap = (Texture2D) EditorGUILayout.ObjectField("Select Heightmap:", heightmap, typeof(Texture2D), true);
                if (!heightmap)
                {
                    LoadHeightmap();
                }

                if (GUILayout.Button("Generate Texture"))
                {
                    Noise.CreateNoiseTexture(_textureResolution);
                }
            }

            EditorGUILayout.Space(1f);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("COLOR SETTINGS", style);
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    _terrainColor.TopColor = EditorGUILayout.ColorField("Top Color:", _terrainColor.TopColor);
                    _terrainColor.MiddleColor = EditorGUILayout.ColorField("Middle Color:", _terrainColor.MiddleColor);
                    _terrainColor.BottomColor = EditorGUILayout.ColorField("Bottom Color:", _terrainColor.BottomColor);

                    if (scope.changed)
                    {
                        //GenerateTerrain();
                    }
                }
            }

            EditorGUILayout.Space(1f);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("TERRAIN SETTINGS", style);
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    _xSize = EditorGUILayout.IntSlider("X Size:", _xSize, 2, 100);
                    _ySize = EditorGUILayout.IntSlider("Y Size:", _ySize, 2, 100);
                    _maxHeight = EditorGUILayout.IntSlider("Max Height:", _maxHeight, 1, 100);
                    _meshType = (EMeshType) EditorGUILayout.EnumPopup("Type of Terrain:", _meshType);

                    if (scope.changed)
                    {
                        //GenerateTerrain();
                    }
                }

                using (new EditorGUI.DisabledScope(!heightmap))
                {
                    _shaderType = (EShaderType) EditorGUILayout.EnumPopup("Shader type:", _shaderType);
                    if (GUILayout.Button("Generate Terrain"))
                    {
                        var meshGenerator = new TerrainGenerator(_meshType, _xSize, _ySize, heightmap, _maxHeight, _terrainColor, _shaderType);
                        meshGenerator.GenerateTerrain();
                    }
                }
            }

            EditorGUILayout.Space(1f);

            if (!heightmap)
            {
                GUILayout.Label("CREATE A NEW TEXTURE", style);
            }
            else
            {
                GUILayout.Label("");
            }
        }

        private void DelayCall()
        {
            throw new NotImplementedException();
        }

        private void LoadHeightmap()
        {
            heightmap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Heightmap.png");
        }

        private void GenerateTerrain()
        {
            var meshGenerator = new TerrainGenerator(_meshType, _xSize, _ySize, heightmap, _maxHeight, _terrainColor, _shaderType);
            meshGenerator.GenerateTerrain();
        }
    }
    
    
}

public static class DelayCall
{
    public static EditorApplication.CallbackFunction ByNumberOfEditorFrames(int n, Action a)
    {
        void Callback()
        {
            if (n-- <= 0)
            {
                a();
            }
            else
            {
                EditorApplication.delayCall += Callback;
            }
        }

        return Callback;
    }
}
