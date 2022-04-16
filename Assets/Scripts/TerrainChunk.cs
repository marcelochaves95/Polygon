using System;
using UnityEngine;

public class TerrainChunk
{
    private const float COLLIDER_GENERATION_DISTANCE_THRESHOLD = 5;
    public event Action<TerrainChunk, bool> OnVisibilityChanged;
    public Vector2 Coord;

    private readonly GameObject _meshObject;
    private readonly Vector2 _sampleCentre;
    private Bounds _bounds;

    private readonly MeshRenderer _meshRenderer;
    private readonly MeshFilter _meshFilter;
    private readonly MeshCollider _meshCollider;

    private readonly LODInfo[] _detailLevels;
    private readonly LODMesh[] _lodMeshes;
    private readonly int _colliderLODIndex;

    private HeightMap _heightMap;
    private bool _heightMapReceived;
    private int _previousLODIndex = -1;
    private bool _hasSetCollider;
    private readonly float _maxViewDst;

    private readonly HeightMapSettings _heightMapSettings;
    private readonly MeshSettings _meshSettings;
    private readonly Transform _viewer;
    
    private Vector2 _viewerPosition => new Vector2(_viewer.position.x, _viewer.position.z);

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        Coord = coord;
        _detailLevels = detailLevels;
        _colliderLODIndex = colliderLODIndex;
        _heightMapSettings = heightMapSettings;
        _meshSettings = meshSettings;
        _viewer = viewer;

        _sampleCentre = coord * meshSettings.MeshWorldSize / meshSettings.MeshScale;
        Vector2 position = coord * meshSettings.MeshWorldSize;
        _bounds = new Bounds(position, Vector2.one * meshSettings.MeshWorldSize);

        _meshObject = new GameObject("Terrain Chunk");
        _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
        _meshFilter = _meshObject.AddComponent<MeshFilter>();
        _meshCollider = _meshObject.AddComponent<MeshCollider>();
        _meshRenderer.material = material;

        _meshObject.transform.position = new Vector3(position.x, 0, position.y);
        _meshObject.transform.parent = parent;
        SetVisible(false);

        _lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            _lodMeshes[i] = new LODMesh(detailLevels[i].LOD);
            _lodMeshes[i].OnUpdateCallback += OnUpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                _lodMeshes[i].OnUpdateCallback += OnUpdateCollisionMesh;
            }
        }

        _maxViewDst = detailLevels[detailLevels.Length - 1].VisibleDstThreshold;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(GenerateHeightMap, OnHeightMapReceived);

        object GenerateHeightMap()
        {
            return HeightMapGenerator.GenerateHeightMap(_meshSettings.NumVertsPerLine, _meshSettings.NumVertsPerLine, _heightMapSettings, _sampleCentre);
        }

        void OnHeightMapReceived(object heightMapObject)
        {
            _heightMap = (HeightMap) heightMapObject;
            _heightMapReceived = true;
            OnUpdateTerrainChunk();
        }
    }

    public void OnUpdateTerrainChunk()
    {
        if (!_heightMapReceived) return;

        float viewerDstFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(_viewerPosition));

        bool wasVisible = IsVisible();
        bool visible = viewerDstFromNearestEdge <= _maxViewDst;

        if (visible)
        {
            int lodIndex = 0;

            for (int i = 0; i < _detailLevels.Length - 1; i++)
            {
                if (!(viewerDstFromNearestEdge > _detailLevels[i].VisibleDstThreshold))
                {
                    break;
                }

                lodIndex = i + 1;
            }

            if (lodIndex != _previousLODIndex)
            {
                LODMesh lodMesh = _lodMeshes[lodIndex];
                if (lodMesh.HasMesh)
                {
                    _previousLODIndex = lodIndex;
                    _meshFilter.mesh = lodMesh.Mesh;
                }
                else if (!lodMesh.HasRequestedMesh)
                {
                    lodMesh.RequestMesh(_heightMap, _meshSettings);
                }
            }
        }

        if (wasVisible != visible)
        {
            SetVisible(visible);
            OnVisibilityChanged?.Invoke(this, visible);
        }
    }

    public void OnUpdateCollisionMesh()
    {
        if (_hasSetCollider) return;

        float sqrDstFromViewerToEdge = _bounds.SqrDistance(_viewerPosition);

        if (sqrDstFromViewerToEdge < _detailLevels[_colliderLODIndex].SqrVisibleDstThreshold)
        {
            if (!_lodMeshes[_colliderLODIndex].HasRequestedMesh)
            {
                _lodMeshes[_colliderLODIndex].RequestMesh(_heightMap, _meshSettings);
            }
        }

        if (sqrDstFromViewerToEdge < COLLIDER_GENERATION_DISTANCE_THRESHOLD * COLLIDER_GENERATION_DISTANCE_THRESHOLD)
        {
            if (_lodMeshes[_colliderLODIndex].HasMesh)
            {
                _meshCollider.sharedMesh = _lodMeshes[_colliderLODIndex].Mesh;
                _hasSetCollider = true;
            }
        }
    }

    private void SetVisible(bool visible)
    {
        _meshObject.SetActive(visible);
    }

    private bool IsVisible()
    {
        return _meshObject.activeSelf;
    }
}