using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
public class TerrainCreation : EditorWindow {

    private string[] options = new string[] { "Smooth", "Flat" };

    private int index;
    private int maxHeight;
    private int xSize;
    private int ySize;
    private int gridResolution = 1024;
    private static int octavees = 8;
    private List<int> triangles = new List<int>();

    private static float noiseScale = 1.0f;
    
    private bool flat;
    private bool color;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();

    private List<Vector2> uvs = new List<Vector2>();

    private Color highColor = Color.blue;
    private Color mediumColor = Color.red;
    private Color lowColor = Color.green;
    private Color[] pix;
    public List<Color> colors = new List<Color>();
    
    private static Texture2D noiseTex;
    private Texture2D heightmap;

    private MeshFilter mesh;

	[MenuItem("Terrain/Create Terrain...")]
	private static void Init () {
		EditorWindow.GetWindow<TerrainCreation>().Show();
	}

	private void OnGUI () {
        var styleTittle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

        GUILayout.Label("TEXTURE", styleTittle);
		noiseScale = EditorGUILayout.FloatField("Noise Scale", noiseScale);
		octavees = EditorGUILayout.IntSlider("Number of Octavees", octavees, 0, 8);
        if (GUILayout.Button("Create New Texture")) {
            CreateNoiseTexture();
        }

        GUILayout.Label("TERRAIN SETTINGS", styleTittle);
        color = EditorGUILayout.Foldout(color, "Color");
        if (color) {
            highColor = EditorGUILayout.ColorField("High Color", highColor);
            mediumColor = EditorGUILayout.ColorField("Medium Color", mediumColor);
            lowColor = EditorGUILayout.ColorField("Low Color", lowColor);
        }
        xSize = EditorGUILayout.IntField("X Size:", xSize);
        ySize = EditorGUILayout.IntField("Y Size:", ySize);
        maxHeight = EditorGUILayout.IntSlider("Max Height:", maxHeight, 1, 100);
        GUILayout.Label("Type of Terrain:");
        index = EditorGUILayout.Popup(index, options);
        switch (index) {
            case 0:
                flat = false;
                break;
            case 1:
                flat = true;
                break;
            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
        GUILayout.Label("Select Plane:");
        mesh = (MeshFilter)EditorGUILayout.ObjectField(mesh, typeof(MeshFilter), true);
        GUILayout.Label("Select Heightmap:");
        heightmap = (Texture2D)EditorGUILayout.ObjectField(heightmap, typeof(Texture2D), true);
        
        EditorGUILayout.LabelField("GENERATE TERRAIN", styleTittle);
        if (GUILayout.Button("Generate Terrain without Color and Texture")) {
            mesh.GetComponent<Renderer>().material.shader = Shader.Find("Custom/TerrainShader");
            GenerateProceduralTerrain();
        }
        if (GUILayout.Button("Generate Terrain with Color")) {
            mesh.GetComponent<Renderer>().material.shader = Shader.Find("Custom/TerrainShaderColor");
            GenerateProceduralTerrain();
        }
        if (GUILayout.Button("Generate Terrain with Texture")) {
            mesh.GetComponent<Renderer>().material.shader = Shader.Find("Custom/TerrainShaderTexture");
            Texture();
            GenerateProceduralTerrain();
        }
    }

    /// <summary>
    /// Método para criar o noise texture
    /// </summary>
	private void CreateNoiseTexture () {
		noiseTex = new Texture2D(gridResolution, gridResolution);
		pix = new Color[gridResolution * gridResolution];
		float xOri = Random.value * 100000.0f;
		float yOri = Random.value * 100000.0f;
		float y = 0.0f;
		while (y < noiseTex.height) {
			float x = 0.0f;
			while (x < noiseTex.width) {
				float sample = OctaveesNoise2D(xOri + x / noiseTex.width, yOri + y / noiseTex.height, octavees, 1.0f, 0.75f);
				pix[(int) y * noiseTex.width + (int) x] = new Color(sample, sample, sample);
				x++;
            }
            y++;
        }
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
		byte[] bytes = noiseTex.EncodeToPNG ();
		Debug.Log("Creating Texture Heightmap: " + Application.dataPath + "/Textures/Heightmap.png");
		File.WriteAllBytes(Application.dataPath + "/Textures/Heightmap.png", bytes);
		AssetDatabase.ImportAsset("Assets/Textures/Heightmap.png");
	}

    /// <summary>
    /// Método para criar o octavees noise 2D
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="octNum">Número de octavees</param>
    /// <param name="frq">frquência</param>
    /// <param name="amp">Amplitude</param>
    /// <returns>Valor do perlin noise</returns>
	public float OctaveesNoise2D (float x, float y, int octNum, float frq, float amp) {
		float gain = 1.0f;
		float sum = 0.0f;
		for (int i = 0; i < octNum; i++) {
			sum +=  Mathf.PerlinNoise(x * gain / frq, y * gain / frq) * amp / gain;
			gain *= 2.0f;
		}
		return sum;
	}

    /// <summary>
    /// Método que reseta tudo
    /// </summary>
    private  void ClearLists () {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        colors.Clear();
        uvs.Clear();
    }

    private void Texture () {
        highColor = Color.blue;
        mediumColor = Color.red;
        lowColor = Color.green;
    }

    /// <summary>
    /// Método para criar o terreno procedural
    /// </summary>
    private void GenerateProceduralTerrain () {
        ClearLists();
        mesh.sharedMesh = new Mesh();
        mesh.sharedMesh.name = "Procedural Grid";
        CreateVertices();
        if (flat) {
            CalculateNormalsFlat();
        } else {
            CalculateNormalsSmooth();
        }
        Debug.Log("Terrain Created");
    }

    /// <summary>
    /// Método para criar a malha
    /// </summary>
    private void CreateVertices () {
        float constant;
        for (int z = 0; z <= ySize - 1; z++) {
            for (int x = 0; x <= xSize - 1; x++) {
                Vector2 uv = new Vector2((float)x / (float)xSize, (float)z / (float)ySize);
                float height = heightmap.GetPixelBilinear(uv.x, uv.y).grayscale * maxHeight;
                vertices.Add(new Vector3(x, height, z));
                uvs.Add(uv);
                if (height <= maxHeight * 0.45) {
                    colors.Add(lowColor);
                } else if (height > maxHeight * 0.45 && height <= maxHeight * 0.55) {
                    constant = (height - (maxHeight * 0.45f)) * 10;
                    colors.Add(Color.Lerp(lowColor, mediumColor, constant));
                } else if (height > maxHeight * 0.55 && height <= maxHeight * 0.75) {
                    colors.Add(mediumColor);
                } else if (height > maxHeight * 0.75 && height < maxHeight * 0.85) {
                    constant = (height - (maxHeight * 0.75f)) * 10;
                    colors.Add(Color.Lerp(mediumColor, highColor, constant));
                } else if (height > maxHeight * 0.85 && height <= maxHeight) {
                    colors.Add(highColor);
                }
            }
        }
        mesh.sharedMesh.vertices = vertices.ToArray();
        mesh.sharedMesh.colors = colors.ToArray();
        mesh.sharedMesh.uv = uvs.ToArray();
        for (int i = 0; i <= (xSize - 1) * (ySize - 1); i++) {
            if (i % xSize == 0) {
                triangles.Add(i);
                triangles.Add(i + xSize);
                triangles.Add(i + 1);
            } else if ((i + 1) % xSize == 0) {
                triangles.Add(i);
                triangles.Add(i + xSize - 1);
                triangles.Add(i + xSize);
            } else {
                triangles.Add(i);
                triangles.Add(i + xSize);
                triangles.Add(i + 1);

                triangles.Add(i);
                triangles.Add(i + xSize - 1);
                triangles.Add(i + xSize);
            }
        }
        mesh.sharedMesh.triangles = triangles.ToArray();
    }

    /*void CreateMeshRenato () {
        List<Vector3> n;
        List<Vector3> v;
        List<int> i;
        for (int x = 0; x < max - 1; x++)
        {
            for (int z = 0; z < max - 1; z++)
            {
                Vector3 v1 = new Vector3(x, 0, 2);
                Vector3 v1 = new Vector3(x, 0, 2 + 1);
                Vector3 v2 = new Vector3(x, 0, 2 + 1);
                Vector3 v3 = new Vector3(x, 0, 2 + 1);
                int index > v.

                v.Add(v1);
                v.Add(v2);
                v.Add(v3);
                i.Add(index + 1);
                i.Add(index + 2);
                i.Add(index + 3);
            }
        }
    }*/

    /// <summary>
    /// Método para calcular as normais smooth
    /// </summary>
    private void CalculateNormalsSmooth () {
        for (int i = 0; i < vertices.Count; i++) {
            Vector3 normalMed = new Vector3(0, 0, 0);
            int flag = 0;
            for (int j = 0; j < triangles.Count - 3; j += 3) {
                if (i != triangles[j] && i != triangles[j + 1] && i != triangles[j + 2]) {
                    continue;
                }
                Vector3 v1 = vertices[triangles[j + 1]] - vertices[triangles[j]];
                Vector3 v2 = vertices[triangles[j + 2]] - vertices[triangles[j]];
                Vector3 normal = Vector3.Cross(v1, v2);
                normalMed += normal;
                flag++;
            }
            normalMed = normalMed / flag;
            normalMed.Normalize();
            normals.Add(normalMed);
        }
        mesh.sharedMesh.normals = normals.ToArray();
    }

    /// <summary>
    /// Método para calcular as normais flat
    /// </summary>
    private void CalculateNormalsFlat () {
        for (int i = 0; i < vertices.Count - 3; i += 3) {
            Vector3 normalMed = new Vector3(0, 0, 0);
            int flag = 0;
            for (int j = 0; j < triangles.Count - 3; j += 3) {
                if (i != triangles[j] && i != triangles[j + 1] && i != triangles[j + 2]) {
                    continue;
                }
                Vector3 v1 = vertices[triangles[j + 1]] - vertices[triangles[j]];
                Vector3 v2 = vertices[triangles[j + 2]] - vertices[triangles[j]];
                Vector3 normal = Vector3.Cross(v1, v2);
                normalMed += normal;
                flag++;
            }
            normalMed = normalMed / flag;
            normalMed.Normalize();
            normals.Add(normalMed);
            normals.Add(normalMed);
            normals.Add(normalMed);
            if (i >= vertices.Count - 5) {
                normals.Add(normalMed);
            }
        }
        mesh.sharedMesh.normals = normals.ToArray();
    }
}