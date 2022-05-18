using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshData _meshData;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshData = new MeshData();
        UIManager.OnUpdateUI += OnUpdateUI;
    }

    private void OnDestroy()
    {
        UIManager.OnUpdateUI -= OnUpdateUI;
    }

    private void OnUpdateUI(MeshSettings meshSettings)
    {
        GenerateMesh(meshSettings);
    }

    private void GenerateMesh(MeshSettings meshSettings)
    {
        _meshFilter.sharedMesh = _meshData.GenerateMesh(meshSettings);
    }
}
