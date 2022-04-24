using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralTerrain
{
    public class TerrainGenerator
    {
        //private readonly List<int> _triangles = new List<int>();
        //private readonly List<Vector3> _vertices = new List<Vector3>();
        //private readonly List<Vector3> _normals = new List<Vector3>();
        //private readonly List<Vector2> _uvs = new List<Vector2>();

        private GameObject _terrain;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private readonly Material _material;
        private readonly EMeshType _meshType;
        private readonly int _xSize;
        private readonly int _ySize;
        private readonly Texture2D _heightmap;
        private readonly int _maxHeight;
        private readonly TerrainColor _terrainColor;
        private readonly EShaderType _shaderType;

        private readonly Shader _terrainShader;
        private readonly Shader _terrainShaderColor;
        private readonly Shader _terrainShaderTexture;

        public TerrainGenerator(EMeshType meshType, int xSize, int ySize, Texture2D heightmap, int maxHeight, TerrainColor terrainColor, EShaderType shaderType)
        {
            _meshType = meshType;
            _xSize = xSize;
            _ySize = ySize;
            _heightmap = heightmap;
            _maxHeight = maxHeight;
            _terrainColor = terrainColor;
            _shaderType = shaderType;

            _terrainShader = Shader.Find("Custom/TerrainShader");
            _terrainShaderColor = Shader.Find("Custom/TerrainShaderColor");
            _terrainShaderTexture = Shader.Find("Custom/TerrainShaderTexture");
            _material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Terrain.mat");
        }

        public void GenerateTerrain()
        {
            CreateTerrain();
            CreateMesh();
        }

        private void CreateTerrain()
        {
            _terrain = GameObject.Find("Terrain");
            if (!_terrain)
            {
                _terrain = GameObject.Find("Terrain");
                _terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
                _terrain.name = "Terrain";
                _terrain.transform.position = new Vector3(0, 0, 0);
            }

            meshFilter = TryAddComponent<MeshFilter>();
            meshRenderer = TryAddComponent<MeshRenderer>();
        }

        private void CreateMesh()
        {
            Mesh mesh = new Mesh();

            CreateVertices(ref mesh, _xSize, _ySize, _heightmap, _maxHeight, _terrainColor);

            if (_meshType == EMeshType.Flat)
            {
                CalculateNormalsFlat(ref mesh);
            }

            CalculateNormals(ref mesh);
            switch (_shaderType)
            {
                case EShaderType.Basic:
                    meshRenderer.sharedMaterial.shader = _terrainShader;
                    break;
                case EShaderType.Color:
                    meshRenderer.sharedMaterial.shader = _terrainShaderColor;
                    break;
                case EShaderType.Texture:
                    meshRenderer.sharedMaterial.shader = _terrainShaderTexture;
                    break;
            }
        }

        private T TryAddComponent<T>() where T : Component
        {
            T component = _terrain.GetComponent<T>();
            return component ? component : _terrain.AddComponent<T>();
        }

        private void CreateVertices(ref Mesh mesh, int xSize, int ySize, Texture2D heightmap, int maxHeight, TerrainColor terrainColor)
        {
            List<Color> colours = new List<Color>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            for (int z = 0; z <= ySize - 1; z++)
            {
                for (int x = 0; x <= xSize - 1; x++)
                {
                    Vector2 uv = new Vector2(x / (float) xSize, z / (float) ySize);
                    float height = heightmap.GetPixelBilinear(uv.x, uv.y).grayscale * maxHeight;

                    vertices.Add(new Vector3(x, height, z));
                    uvs.Add(uv);
                    if (height <= maxHeight * 0.45)
                    {
                        colours.Add(terrainColor.BottomColor);
                    }
                    else
                    {
                        float constant;
                        if (height > maxHeight * 0.45 && height <= maxHeight * 0.55)
                        {
                            constant = (height - maxHeight * 0.45f) * 10;
                            colours.Add(Color.Lerp(terrainColor.BottomColor, terrainColor.MiddleColor, constant));
                        }
                        else if (height > maxHeight * 0.55 && height <= maxHeight * 0.75)
                        {
                            colours.Add(terrainColor.MiddleColor);
                        }
                        else if (height > maxHeight * 0.75 && height < maxHeight * 0.85)
                        {
                            constant = (height - maxHeight * 0.75f) * 10;
                            colours.Add(Color.Lerp(terrainColor.MiddleColor, terrainColor.TopColor, constant));
                        }
                        else if (height > maxHeight * 0.85 && height <= maxHeight)
                        {
                            colours.Add(terrainColor.TopColor);
                        }
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.colors = colours.ToArray();
            mesh.uv = uvs.ToArray();
            List<int> triangles = new List<int>();
            for (int i = 0; i <= (xSize - 1) * (ySize - 1); i++)
            {
                if (i % xSize == 0)
                {
                    triangles.Add(i);
                    triangles.Add(i + xSize);
                    triangles.Add(++i);
                }
                else if ((i + 1) % xSize == 0)
                {
                    triangles.Add(i);
                    triangles.Add(i + xSize - 1);
                    triangles.Add(i + xSize);
                }
                else
                {
                    triangles.Add(i);
                    triangles.Add(i + xSize);
                    triangles.Add(++i);
                    triangles.Add(i);
                    triangles.Add(i + xSize - 1);
                    triangles.Add(i + xSize);
                }
            }

            mesh.triangles = triangles.ToArray();
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

        private void CalculateNormalsFlat(ref Mesh mesh)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<int> indices = new List<int>();

            int index = 0;
            for (int triangle = 0; triangle < mesh.triangles.Length; triangle++)
            {
                int indexTriangle = mesh.triangles[triangle];

                vertices.Add(mesh.vertices[indexTriangle]);
                uvs.Add(mesh.uv[indexTriangle]);
                colors.Add(mesh.colors[indexTriangle]);

                indices.Add(index++);
            }

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.colors = colors.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        }
    }
}
