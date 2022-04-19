﻿using System;
using UnityEngine;
using UnityEditor;
using ProceduralTerrain;

namespace ProceduralTerrain.Editor
{
    public class TerrainEditor : EditorWindow
    {
        private EMeshType _meshType;
        private EShaderType _shaderType;

        private int maxHeight;
        private int xSize = 1;
        private int ySize = 1;
        private int textureResolution = 1024;
        private static int octavees = 8;

        private TerrainColor _terrainColor;

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
                octavees = EditorGUILayout.IntSlider("Number of Octaves:", octavees, 0, 8);
                textureResolution = EditorGUILayout.IntField("Texture Resolution:", textureResolution);
                heightmap = (Texture2D) EditorGUILayout.ObjectField("Select Heightmap:", heightmap, typeof(Texture2D), true);
                if (!heightmap)
                {
                    LoadHeightmap();
                }

                if (GUILayout.Button("Generate Texture"))
                {
                    Noise.CreateNoiseTexture(textureResolution);
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
                    xSize = EditorGUILayout.IntSlider("X Size:", xSize, 2, 100);
                    ySize = EditorGUILayout.IntSlider("Y Size:", ySize, 2, 100);
                    maxHeight = EditorGUILayout.IntSlider("Max Height:", maxHeight, 1, 100);
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
                        var meshGenerator = new ProceduralTerrain.TerrainGenerator(_meshType, xSize, ySize, heightmap, maxHeight, _terrainColor, _shaderType);
                        meshGenerator.GenerateTerrain(true);
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
            var meshGenerator = new TerrainGenerator(_meshType, xSize, ySize, heightmap, maxHeight, _terrainColor, _shaderType);
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
