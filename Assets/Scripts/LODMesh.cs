using System;
using UnityEngine;

public class LODMesh
{
    private readonly int _lod;

    public Mesh Mesh;
    public bool HasRequestedMesh;
    public bool HasMesh;
    public event Action OnUpdateCallback;

    public LODMesh(int lod)
    {
        _lod = lod;
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        HasRequestedMesh = true;
        ThreadedDataRequester.RequestData(GenerateTerrainMesh, OnMeshDataReceived);

        object GenerateTerrainMesh()
        {
            return MeshGenerator.GenerateTerrainMesh(heightMap.Values, meshSettings, _lod);
        }

        void OnMeshDataReceived(object meshDataObject)
        {
            var meshData = meshDataObject as MeshData;
            Mesh = meshData?.CreateMesh();
            HasMesh = true;
            OnUpdateCallback?.Invoke();
        }
    }
}
