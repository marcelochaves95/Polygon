using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public EDrawMode DrawMode;

    public Renderer TextureRender;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;

    public MeshSettings MeshSettings;
    public HeightMapSettings HeightMapSettings;
    public TextureData TextureData;

    public Material TerrainMaterial;

    [Range(0, MeshSettings.NUM_SUPPORTED_LODS - 1)]
    public int EditorPreviewLOD;
    public bool AutoUpdate;

    public void DrawMapInEditor()
    {
        TextureData.ApplyToMaterial(TerrainMaterial);
        TextureData.UpdateMeshHeights(TerrainMaterial, HeightMapSettings.MinHeight, HeightMapSettings.MaxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(MeshSettings.NumVertsPerLine, MeshSettings.NumVertsPerLine, HeightMapSettings, Vector2.zero);

        switch (DrawMode)
        {
            case EDrawMode.NoiseMap:
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
                break;
            case EDrawMode.Mesh:
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.Values, MeshSettings, EditorPreviewLOD));
                break;
            case EDrawMode.FalloffMap:
                DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(MeshSettings.NumVertsPerLine), 0, 1)));
                break;
        }
    }

    private void DrawTexture(Texture2D texture)
    {
        TextureRender.sharedMaterial.mainTexture = texture;
        TextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        TextureRender.gameObject.SetActive(true);
        MeshFilter.gameObject.SetActive(false);
    }

    private void DrawMesh(MeshData meshData)
    {
        MeshFilter.sharedMesh = meshData.CreateMesh();
        TextureRender.gameObject.SetActive(false);
        MeshFilter.gameObject.SetActive(true);
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
        TextureData.ApplyToMaterial(TerrainMaterial);
    }

    private void OnValidate()
    {
        if (MeshSettings != null)
        {
            MeshSettings.OnValuesUpdated -= OnValuesUpdated;
            MeshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (HeightMapSettings != null)
        {
            HeightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            HeightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (TextureData != null)
        {
            TextureData.OnValuesUpdated -= OnTextureValuesUpdated;
            TextureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }
}
