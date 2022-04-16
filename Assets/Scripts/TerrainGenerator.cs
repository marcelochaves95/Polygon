using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private int _colliderLODIndex;

    [SerializeField] private LODInfo[] _detailLevels;

    [SerializeField] private Transform _viewer;

    [SerializeField] private Material _mapMaterial;

    [SerializeField] private TerrainSettings _terrainSettings;

    private int _chunksVisibleInViewDst;

    private float _meshWorldSize;

    private const float VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = 25f;
    private const float SQR_VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE * VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE;

    private Vector2 _viewerPosition;
    private Vector2 _viewerPositionOld;

    private readonly List<TerrainChunk> _visibleTerrainChunks = new List<TerrainChunk>();

    private readonly Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    private void Start()
    {
        _terrainSettings.ApplyToMaterial(_mapMaterial);
        _terrainSettings.UpdateMeshHeights(_mapMaterial, _terrainSettings.MinHeight, _terrainSettings.MaxHeight);

        float maxViewDst = _detailLevels[_detailLevels.Length - 1].VisibleDstThreshold;
        _meshWorldSize = _terrainSettings.MeshWorldSize;
        _chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / _meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        Vector3 position = _viewer.position;
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
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, _terrainSettings, _detailLevels, _colliderLODIndex, transform, _viewer, _mapMaterial);
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