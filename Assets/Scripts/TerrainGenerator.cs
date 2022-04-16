using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    private const float VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = 25f;
    private const float SQR_VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE * VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE;

    public int ColliderLODIndex;
    public LODInfo[] DetailLevels;

    public MeshSettings MeshSettings;
    public HeightMapSettings HeightMapSettings;
    public TextureData TextureSettings;

    public Transform Viewer;
    public Material MapMaterial;

    private Vector2 _viewerPosition;
    private Vector2 _viewerPositionOld;

    private float _meshWorldSize;
    private int _chunksVisibleInViewDst;

    private readonly Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private readonly List<TerrainChunk> _visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        TextureSettings.ApplyToMaterial(MapMaterial);
        TextureSettings.UpdateMeshHeights(MapMaterial, HeightMapSettings.MinHeight, HeightMapSettings.MaxHeight);

        float maxViewDst = DetailLevels[DetailLevels.Length - 1].VisibleDstThreshold;
        _meshWorldSize = MeshSettings.MeshWorldSize;
        _chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / _meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        Vector3 position = Viewer.position;
        _viewerPosition = new Vector2(position.x, position.z);

        if (_viewerPosition != _viewerPositionOld)
        {
            foreach (TerrainChunk chunk in _visibleTerrainChunks)
            {
                chunk.OnUpdateCollisionMesh();
            }
        }

        if ((_viewerPositionOld - _viewerPosition).sqrMagnitude > SQR_VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE)
        {
            _viewerPositionOld = _viewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks()
    {
        var alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = _visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(_visibleTerrainChunks[i].Coord);
            _visibleTerrainChunks[i].OnUpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(_viewerPosition.x / _meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(_viewerPosition.y / _meshWorldSize);

        for (int yOffset = -_chunksVisibleInViewDst; yOffset <= _chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        _terrainChunkDictionary[viewedChunkCoord].OnUpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, HeightMapSettings, MeshSettings, DetailLevels, ColliderLODIndex, transform, Viewer, MapMaterial);
                        _terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.OnVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            _visibleTerrainChunks.Add(chunk);
        }
        else
        {
            _visibleTerrainChunks.Remove(chunk);
        }
    }
}