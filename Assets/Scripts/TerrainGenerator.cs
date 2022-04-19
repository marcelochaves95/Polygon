using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralTerrain
{
    public class TerrainGenerator
    {
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();

        private GameObject _terrain;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private readonly Material _material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Terrain.mat");
        private readonly EMeshType _meshType;
        private readonly int _xSize;
        private readonly int _ySize;
        private readonly Texture2D _heightmap;
        private readonly int _maxHeight;
        private readonly TerrainColor _terrainColor;
        private readonly EShaderType _shaderType;

        private readonly Shader _terrainShader = Shader.Find("Custom/TerrainShader");
        private readonly Shader _terrainShaderColor = Shader.Find("Custom/TerrainShaderColor");
        private readonly Shader _terrainShaderTexture = Shader.Find("Custom/TerrainShaderTexture");

        public TerrainGenerator(EMeshType meshType, int xSize, int ySize, Texture2D heightmap, int maxHeight, TerrainColor terrainColor, EShaderType shaderType)
        {
            _meshType = meshType;
            _xSize = xSize;
            _ySize = ySize;
            _heightmap = heightmap;
            _maxHeight = maxHeight;
            _terrainColor = terrainColor;
            _shaderType = shaderType;
        }

        public void GenerateTerrain(bool hasSaveMesh = false)
        {
            ClearLists();
            CreateMesh();
            CreateVertices(_xSize, _ySize, _heightmap, _maxHeight, _terrainColor);
            
            switch (_meshType)
            {
                case EMeshType.Smooth:
                    CalculateNormalsSmooth();
                    break;
                case EMeshType.Flat:
                    CalculateNormalsFlat();
                    break;
            }

            switch (_shaderType)
            {
                case EShaderType.Basic:
                    _meshRenderer.sharedMaterial.shader = _terrainShader;
                    break;
                case EShaderType.Color:
                    _meshRenderer.sharedMaterial.shader = _terrainShaderColor;
                    break;
                case EShaderType.Texture:
                    _meshRenderer.sharedMaterial.shader = _terrainShaderTexture;
                    break;
            }

            if (hasSaveMesh)
            {
                SaveMesh(_shaderType);
            }
        }

        private void CreateMesh()
        {
            _terrain = GameObject.Find("Terrain");
            if (!_terrain)
            {
                //Object.DestroyImmediate(terrain);
                _terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
                _terrain.name = "Terrain";
                _meshRenderer = _terrain.GetComponent<MeshRenderer>();
                _meshRenderer.sharedMaterial = _material;
                _meshFilter = _terrain.GetComponent<MeshFilter>();
                _meshFilter.sharedMesh = new Mesh
                {
                    name = "Procedural Mesh"
                };
            }

            if (!_meshRenderer)
            {
                _meshRenderer = _terrain.GetComponent<MeshRenderer>();
                _meshRenderer.sharedMaterial = _material;
            }

            if (!_meshFilter)
            {
                _meshFilter = _terrain.GetComponent<MeshFilter>();
                _meshFilter.sharedMesh = new Mesh
                {
                    name = "Procedural Mesh"
                };
            }
            
            _terrain.transform.position = new Vector3(0, 0, 0);
        }

        private void SaveMesh(EShaderType shaderType)
        {
            string meshes = "Assets/Meshes/" + shaderType + ".asset";
            AssetDatabase.CreateAsset(_meshFilter.sharedMesh, meshes);
            AssetDatabase.SaveAssets();
        }

        private void CreateVertices(int xSize, int ySize, Texture2D heightmap, int maxHeight, TerrainColor terrainColor)
        {
            List<Color> colours = new List<Color>();
            for (int z = 0; z <= ySize - 1; z++)
            {
                for (int x = 0; x <= xSize - 1; x++)
                {
                    Vector2 uv = new Vector2(x / (float) xSize, z / (float) ySize);
                    float height;
                    try
                    {
                        height = heightmap.GetPixelBilinear(uv.x, uv.y).grayscale * maxHeight;
                    }
                    catch (UnityException)
                    {
                        TextureIsRead(heightmap);
                        height = heightmap.GetPixelBilinear(uv.x, uv.y).grayscale * maxHeight;
                    }

                    _vertices.Add(new Vector3(x, height, z));
                    _uvs.Add(uv);
                    if (height <= maxHeight * 0.45)
                    {
                        colours.Add(terrainColor.BottomColor);
                    }
                    else
                    {
                        float constant;
                        if (height > maxHeight * 0.45 && height <= maxHeight * 0.55)
                        {
                            constant = (height - (maxHeight * 0.45f)) * 10;
                            colours.Add(Color.Lerp(terrainColor.BottomColor, terrainColor.MiddleColor, constant));
                        }
                        else if (height > maxHeight * 0.55 && height <= maxHeight * 0.75)
                        {
                            colours.Add(terrainColor.MiddleColor);
                        }
                        else if (height > maxHeight * 0.75 && height < maxHeight * 0.85)
                        {
                            constant = (height - (maxHeight * 0.75f)) * 10;
                            colours.Add(Color.Lerp(terrainColor.MiddleColor, terrainColor.TopColor, constant));
                        }
                        else if (height > maxHeight * 0.85 && height <= maxHeight)
                        {
                            colours.Add(terrainColor.TopColor);
                        }
                    }
                }
            }

            _meshFilter.sharedMesh.vertices = _vertices.ToArray();
            _meshFilter.sharedMesh.colors = colours.ToArray();
            _meshFilter.sharedMesh.uv = _uvs.ToArray();
            for (int i = 0; i <= (xSize - 1) * (ySize - 1); i++)
            {
                if (i % xSize == 0)
                {
                    _triangles.Add(i);
                    _triangles.Add(i + xSize);
                    _triangles.Add(i + 1);
                }
                else if ((i + 1) % xSize == 0)
                {
                    _triangles.Add(i);
                    _triangles.Add(i + xSize - 1);
                    _triangles.Add(i + xSize);
                }
                else
                {
                    _triangles.Add(i);
                    _triangles.Add(i + xSize);
                    _triangles.Add(i + 1);
                    _triangles.Add(i);
                    _triangles.Add(i + xSize - 1);
                    _triangles.Add(i + xSize);
                }
            }

            _meshFilter.sharedMesh.triangles = _triangles.ToArray();
        }

        private void CalculateNormalsSmooth()
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                Vector3 normalMed = new Vector3(0, 0, 0);
                int flag = 0;
                for (int j = 0; j < _triangles.Count - 3; j += 3)
                {
                    if (i != _triangles[j] && i != _triangles[j + 1] && i != _triangles[j + 2])
                    {
                        continue;
                    }
                    Vector3 v1 = _vertices[_triangles[j + 1]] - _vertices[_triangles[j]];
                    Vector3 v2 = _vertices[_triangles[j + 2]] - _vertices[_triangles[j]];
                    Vector3 normal = Vector3.Cross(v1, v2);
                    normalMed += normal;
                    flag++;
                }
                normalMed /= flag;
                normalMed.Normalize();
                _normals.Add(normalMed);
            }

            _meshFilter.sharedMesh.normals = _normals.ToArray();
        }

        private void CalculateNormalsFlat()
        {
            for (int i = 0; i < _vertices.Count - 3; i += 3)
            {
                Vector3 normalMed = new Vector3(0, 0, 0);
                int flag = 0;
                for (int j = 0; j < _triangles.Count - 3; j += 3)
                {
                    if (i != _triangles[j] && i != _triangles[j + 1] && i != _triangles[j + 2])
                    {
                        continue;
                    }
                    Vector3 v1 = _vertices[_triangles[j + 1]] - _vertices[_triangles[j]];
                    Vector3 v2 = _vertices[_triangles[j + 2]] - _vertices[_triangles[j]];
                    Vector3 normal = Vector3.Cross(v1, v2);
                    normalMed += normal;
                    flag++;
                }
                normalMed /= flag;
                normalMed.Normalize();
                _normals.Add(normalMed);
                _normals.Add(normalMed);
                _normals.Add(normalMed);
                if (i >= _vertices.Count - 5)
                {
                    _normals.Add(normalMed);
                }
            }

            _meshFilter.sharedMesh.normals = _normals.ToArray();
        }

        private void TextureIsRead(Texture2D heightmap)
        {
            string assetPath = AssetDatabase.GetAssetPath(heightmap);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }
        
        private void ClearLists()
        {
            _vertices.Clear();
            _triangles.Clear();
            _normals.Clear();
            _uvs.Clear();
        }
    }
}
