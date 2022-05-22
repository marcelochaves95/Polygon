using System;
using System.Collections.Generic;
using UnityEngine;

namespace Polygon
{
    public class MeshData
    {
        private Mesh _mesh;
        private int[] _triangles;
        private Vector3[] _vertices;
        private Vector2[] _uv;
        private Color[] _colors;

        public MeshData()
        {
            _mesh = new Mesh();
            _triangles = Array.Empty<int>();
            _vertices = Array.Empty<Vector3>();
            _uv = Array.Empty<Vector2>();
            _colors = Array.Empty<Color>();
        }

        public Mesh GenerateMesh(MeshSettings meshSettings)
        {
            GenerateVertices(ref _vertices, meshSettings);
            GenerateTriangles(ref _triangles, meshSettings.vertexSettings);
            GenerateUV(ref _uv, meshSettings.vertexSettings);
            GenerateColors(ref _colors, meshSettings.vertexSettings);

            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
            _mesh.uv = _uv;
            _mesh.colors = _colors;

            if (meshSettings.UseFlatShading)
            {
                FlatShading(ref _mesh);
            }

            CalculateNormals(ref _mesh);

            return _mesh;
        }

        private void GenerateVertices(ref Vector3[] vertices, MeshSettings meshSettings)
        {
            if (vertices == null)
            {
                throw new ArgumentNullException(nameof(vertices));
            }

            vertices = new Vector3[(meshSettings.vertexSettings.XSize + 1) * (meshSettings.vertexSettings.ZSize + 1)];
            for (int i = 0, z = 0; z <= meshSettings.vertexSettings.ZSize; z++)
            {
                for (int x = 0; x <= meshSettings.vertexSettings.XSize; x++)
                {
                    float y = Mathf.PerlinNoise(x * meshSettings.NoiseFrequency.Value, z * meshSettings.NoiseFrequency.Value) * meshSettings.Height.Value;
                    vertices[i] = new Vector3(x, y, z);
                    i++;
                }
            }
        }
        
        private void GenerateTriangles(ref int[] triangles, VertexSettings vertexSettings)
        {
            if (triangles == null)
            {
                throw new ArgumentNullException(nameof(triangles));
            }

            int vertexCount = 0;
            int trianglesCount = 0;
            triangles = new int[vertexSettings.XSize * vertexSettings.ZSize * 6];
            for (int z = 0; z < vertexSettings.ZSize; z++)
            {
                for (int x = 0; x < vertexSettings.XSize; x++)
                {
                    triangles[trianglesCount + 0] = vertexCount + 0;
                    triangles[trianglesCount + 1] = vertexCount + vertexSettings.XSize + 1;
                    triangles[trianglesCount + 2] = vertexCount + 1;
                    triangles[trianglesCount + 3] = vertexCount + 1;
                    triangles[trianglesCount + 4] = vertexCount + vertexSettings.XSize + 1;
                    triangles[trianglesCount + 5] = vertexCount + vertexSettings.XSize + 2;

                    vertexCount++;
                    trianglesCount += 6;
                }

                vertexCount++;
            }
        }

        private void GenerateUV(ref Vector2[] uv, VertexSettings vertexSettings)
        {
            if (uv == null)
            {
                throw new ArgumentNullException(nameof(uv));
            }

            uv = new Vector2[_vertices.Length];
            for (int i = 0, z = 0; z <= vertexSettings.ZSize; z++)
            {
                for (int x = 0; x <= vertexSettings.XSize; x++)
                {
                    uv[i] = new Vector2((float) x / vertexSettings.XSize, (float) z / vertexSettings.ZSize);
                    i++;
                }
            }
        }
        
        private void GenerateColors(ref Color[] colors, VertexSettings vertexSettings)
        {
            if (colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            colors = new Color[_vertices.Length];
            for (int i = 0, z = 0; z <= vertexSettings.ZSize; z++)
            {
                for (int x = 0; x <= vertexSettings.XSize; x++)
                {
                    colors[i] = vertexSettings.Gradient.Evaluate(_vertices[i].y);
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

        public void Clear()
        {
            _mesh.Clear();
            Array.Clear(_triangles, 0, _triangles.Length);
            Array.Clear(_vertices, 0, _vertices.Length);
            Array.Clear(_uv, 0, _uv.Length);
            Array.Clear(_colors, 0, _colors.Length);
        }
    }
}
