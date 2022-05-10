using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    private Mesh _mesh;
    private readonly VertexData _vertexData;
    private readonly WaitForSeconds _waitForSeconds;

    private int[] _triangles;
    private Vector3[] _vertices;
    private Vector2[] _uv;
    private Color[] _colors;
    private int _height;
    private float _noiseFrequency;

    public MeshData(Mesh mesh, VertexData vertexData, MonoBehaviour monoBehaviour, float delay)
    {
        _mesh = mesh;
        _vertexData = vertexData;
        _waitForSeconds = new WaitForSeconds(delay);

        GenerateVertices();
        //monoBehaviour.StartCoroutine(GenerateTriangles());
        GenerateTriangles2();
        GenerateUV();
        GenerateColors();
    }

    public void GenerateMesh(bool useFlatShading, int height, float noiseFrequency)
    {
        _height = height;
        _noiseFrequency = noiseFrequency;
        GenerateVertices();
        GenerateTriangles2();
        GenerateUV();
        GenerateColors();

        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uv;
        _mesh.colors = _colors;

        if (useFlatShading)
        {
            FlatShading(ref _mesh);
        }

        CalculateNormals(ref _mesh);
    }

    private void GenerateVertices()
    {
        _vertices = new Vector3[(_vertexData.XSize + 1) * (_vertexData.ZSize + 1)];
        for (int i = 0, z = 0; z <= _vertexData.ZSize; z++)
        {
            for (int x = 0; x <= _vertexData.XSize; x++)
            {
                float y = GetNoiseSample(x, z);
                _vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
    }

    private float GetNoiseSample(int x, int z)
    {
        return Mathf.PerlinNoise(x * _noiseFrequency, z * _noiseFrequency) * _height;
    }

    private IEnumerator GenerateTriangles()
    {
        _triangles = new int[_vertexData.XSize * _vertexData.ZSize * 6];

        int vertexCount = 0;
        int trianglesCount = 0;
        for (int z = 0; z < _vertexData.ZSize; z++)
        {
            for (int x = 0; x < _vertexData.XSize; x++)
            {
                _triangles[trianglesCount + 0] = vertexCount + 0;
                _triangles[trianglesCount + 1] = vertexCount + _vertexData.XSize + 1;
                _triangles[trianglesCount + 2] = vertexCount + 1;
                _triangles[trianglesCount + 3] = vertexCount + 1;
                _triangles[trianglesCount + 4] = vertexCount + _vertexData.XSize + 1;
                _triangles[trianglesCount + 5] = vertexCount + _vertexData.XSize + 2;

                vertexCount++;
                trianglesCount += 6;

                yield return _waitForSeconds;
            }

            vertexCount++;
        }
    }
    
    private void GenerateTriangles2()
    {
        _triangles = new int[_vertexData.XSize * _vertexData.ZSize * 6];

        int vertexCount = 0;
        int trianglesCount = 0;
        for (int z = 0; z < _vertexData.ZSize; z++)
        {
            for (int x = 0; x < _vertexData.XSize; x++)
            {
                _triangles[trianglesCount + 0] = vertexCount + 0;
                _triangles[trianglesCount + 1] = vertexCount + _vertexData.XSize + 1;
                _triangles[trianglesCount + 2] = vertexCount + 1;
                _triangles[trianglesCount + 3] = vertexCount + 1;
                _triangles[trianglesCount + 4] = vertexCount + _vertexData.XSize + 1;
                _triangles[trianglesCount + 5] = vertexCount + _vertexData.XSize + 2;

                vertexCount++;
                trianglesCount += 6;
            }

            vertexCount++;
        }
    }

    private void GenerateUV()
    {
        _uv = new Vector2[_vertices.Length];
        for (int i = 0, z = 0; z <= _vertexData.ZSize; z++)
        {
            for (int x = 0; x <= _vertexData.XSize; x++)
            {
                _uv[i] = new Vector2((float) x / _vertexData.XSize, (float) z / _vertexData.ZSize);
                i++;
            }
        }
    }
    
    private void GenerateColors()
    {
        _colors = new Color[_vertices.Length];
        for (int i = 0, z = 0; z <= _vertexData.ZSize; z++)
        {
            for (int x = 0; x <= _vertexData.XSize; x++)
            {
                _colors[i] = _vertexData.Gradient.Evaluate(_vertices[i].y);
                i++;
            }
        }
    }

    private void CalculateNormals(ref Mesh mesh)
    {
        Vector3[] normals = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            int v0 = mesh.triangles[i];
            int v1 = mesh.triangles[++i];
            int v2 = mesh.triangles[++i];

            Vector3 normal = CalculateTriangleNormal(mesh, v0, v1, v2);

            normals[v0] += normal;
            normals[v1] += normal;
            normals[v2] += normal;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i].Normalize();
        }

        mesh.normals = normals;
    }

    private Vector3 CalculateTriangleNormal(Mesh mesh, int v0, int v1, int v2)
    {
        Vector3 toV1 = mesh.vertices[v1] - mesh.vertices[v0];
        Vector3 toV2 = mesh.vertices[v2] - mesh.vertices[v0];

        return Vector3.Cross(toV1, toV2);
    }

    private void FlatShading(ref Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Color> colors = new List<Color>();
        List<int> indices = new List<int>();

        for (int index = 0, i = 0; i < mesh.triangles.Length; i++)
        {
            int indexTriangle = mesh.triangles[i];
            vertices.Add(mesh.vertices[indexTriangle]);
            uv.Add(mesh.uv[indexTriangle]);
            colors.Add(mesh.colors[indexTriangle]);

            indices.Add(index++);
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.colors = colors.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
    }
}