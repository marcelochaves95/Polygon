using System;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    [SerializeField]
    private EDrawMode _drawMode;

    [SerializeField, Range(0, TerrainSettings.NUM_SUPPORTED_LODS - 1)] private int _editorPreviewLOD;
    public bool AutoUpdate;

    [SerializeField] private Renderer _textureRender;

    [SerializeField] private MeshFilter _meshFilter;

    [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] private Material _terrainMaterial;

    [SerializeField] private TerrainSettings _terrainSettings;

    private void Update()
    {
        DrawMapInEditor();
    }

    public void DrawMapInEditor()
    {
        _terrainSettings.ApplyToMaterial(_terrainMaterial);
        _terrainSettings.UpdateMeshHeights(_terrainMaterial, _terrainSettings.MinHeight, _terrainSettings.MaxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(_terrainSettings.NumVertsPerLine, _terrainSettings.NumVertsPerLine, _terrainSettings, Vector2.zero);

        switch (_drawMode)
        {
            case EDrawMode.NoiseMap:
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
                break;
            case EDrawMode.Mesh:
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.Values, _terrainSettings, _editorPreviewLOD));
                break;
            case EDrawMode.FalloffMap:
                DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(_terrainSettings.NumVertsPerLine), 0, 1)));
                break;
        }
    }

    private void DrawTexture(Texture2D texture)
    {
        _textureRender.sharedMaterial.mainTexture = texture;
        _textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        _textureRender.gameObject.SetActive(true);
        _meshFilter.gameObject.SetActive(false);
    }

    private void DrawMesh(MeshData meshData)
    {
        _meshFilter.sharedMesh = meshData.CreateMesh();
        _textureRender.gameObject.SetActive(false);
        _meshFilter.gameObject.SetActive(true);
    }

    private void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    private void OnTextureValuesUpdated()
    {
        _terrainSettings.ApplyToMaterial(_terrainMaterial);
    }

    private void OnValidate()
    {
        if (_terrainSettings != null)
        {
            _terrainSettings.OnValuesUpdated -= OnValuesUpdated;
            _terrainSettings.OnValuesUpdated += OnValuesUpdated;
            _terrainSettings.OnValuesUpdated -= OnTextureValuesUpdated;
            _terrainSettings.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}
