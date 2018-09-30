using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
public class TerrainCreation : EditorWindow {

    private enum options { Smooth, Flat }
    private options mode;

    private int maxHeight;
    private int xSize = 1;
    private int ySize = 1;
    private int textureResolution = 1024;
    private static int octavees = 8;
    private List<int> triangles = new List<int>();
    
    private bool flat;
    private bool color;

    private GameObject terrain;

    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();

    private List<Vector2> uvs = new List<Vector2>();

    private Color topColor = Color.blue;
    private Color middleColor = Color.red;
    private Color bottomColor = Color.green;
    private Color[] pix;
    public List<Color> colors = new List<Color>();

    private Texture2D heightmap;
    private static Texture2D noiseTex;

    private Material material;

    #region EditorWindow
    /// <summary>
    /// Terrain editor startup method
    /// </summary>
	[MenuItem("Terrain/Create Terrain...")]
	private static void Init () {
		EditorWindow.GetWindow<TerrainCreation>().Show();
	}

    /// <summary>
    /// Inspector configuration method
    /// </summary>
	private void OnGUI () {
        var centralizedWords = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

        GUILayout.Label("TEXTURE SETTINGS", centralizedWords);
		octavees = EditorGUILayout.IntSlider("Number of Octavees:", octavees, 0, 8);
        textureResolution = EditorGUILayout.IntField("Texture Resolution:", textureResolution);
        heightmap = (Texture2D)EditorGUILayout.ObjectField("Select Heightmap:", heightmap, typeof(Texture2D), true);
        if (!heightmap) {
            LoadHeightmap();
        }
        if (GUILayout.Button("Create New Texture")) {
            CreateNoiseTexture();
        }

        EditorGUILayout.Separator();
        GUILayout.Label("COLOR SETTINGS", centralizedWords);
        topColor = EditorGUILayout.ColorField("Top Color:", topColor);
        middleColor = EditorGUILayout.ColorField("Middle Color:", middleColor);
        bottomColor = EditorGUILayout.ColorField("Bottom Color:", bottomColor);

        EditorGUILayout.Separator();
        GUILayout.Label("TERRAIN SETTINGS", centralizedWords);
        xSize = EditorGUILayout.IntField("X Size:", xSize);
        ySize = EditorGUILayout.IntField("Y Size:", ySize);
        maxHeight = EditorGUILayout.IntSlider("Max Height:", maxHeight, 1, 100);
        mode = (options)EditorGUILayout.EnumPopup("Type of Terrain:", mode);
        switch (mode) {
            case options.Smooth:
                flat = false;
                break;
            case options.Flat:
                flat = true;
                break;
            default:
                Debug.LogError("Unrecognized Option");
                break;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("GENERATE TERRAIN", centralizedWords);
        EditorGUI.BeginDisabledGroup(!heightmap);
        if (GUILayout.Button("Generate Terrain without Color and Texture")) {
            GenerateTerrain();
            terrain.GetComponent<MeshRenderer>().sharedMaterial.shader = Shader.Find("Custom/TerrainShader");
            SaveMesh("Basic");
        }
        if (GUILayout.Button("Generate Terrain with Color")) {
            GenerateTerrain();
            terrain.GetComponent<MeshRenderer>().sharedMaterial.shader = Shader.Find("Custom/TerrainShaderColor");
            SaveMesh("Color");
        }
        if (GUILayout.Button("Generate Terrain with Texture")) {
            Texture();
            GenerateTerrain();
            terrain.GetComponent<MeshRenderer>().sharedMaterial.shader = Shader.Find("Custom/TerrainShaderTexture");
            SaveMesh("Texture");
        }
        EditorGUI.EndDisabledGroup();
        
        GUILayout.Space(15);
        if (!heightmap) {
            GUILayout.Label("CREATE A NEW TEXTURE", centralizedWords);
        } else {
            GUILayout.Label("");
        }
    }
    #endregion

    #region Texture
    /// <summary>
    /// Method to create noise texture
    /// </summary>
	private void CreateNoiseTexture () {
		noiseTex = new Texture2D(textureResolution, textureResolution);
		pix = new Color[textureResolution * textureResolution];
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
    /// Method for creating octavees noise 2D
    /// </summary>
    /// <param name="x">X</param>
    /// <param name="y">Y</param>
    /// <param name="octNum">Number of octavees</param>
    /// <param name="frq">Frequency</param>
    /// <param name="amp">Amplitude</param>
    /// <returns>Value of perlin noise</returns>
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
    /// Method to load heightmap texture
    /// </summary>
    private void LoadHeightmap () {
        heightmap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/Heightmap.png");
    }

    /// <summary>
    /// Method to set texture's Read/Write option to true
    /// </summary>
    /// <param name="heightmap">Heighmap</param>
    /// <param name="isReadable">If you can read</param>
    private void TextureIsRead(Texture2D heightmap, bool isReadable) {
        if (heightmap == null) {
            return;
        }
        string assetPath = AssetDatabase.GetAssetPath(heightmap);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null) {
            importer.isReadable = isReadable;
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Method to set colors
    /// </summary>
    private void Texture () {
        topColor = Color.blue;
        middleColor = Color.red;
        bottomColor = Color.green;
    }
    #endregion

    #region Creation
    /// <summary>
    /// Method to create terrain
    /// </summary>
    private void GenerateTerrain () {
        ClearLists();
        CreateMesh();
        terrain.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        terrain.GetComponent<MeshFilter>().sharedMesh.name = "Procedural Mesh";
        CreateVertices();
        if (flat) {
            CalculateNormalsFlat();
        } else {
            CalculateNormalsSmooth();
        }
        Debug.Log("Terrain Created");
    }

    /// <summary>
    /// Method for creating the terrain base mesh
    /// </summary>
    private void CreateMesh () {
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Terrain.mat");
        if (!terrain && !(terrain = GameObject.Find("Terrain"))) {
            terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.transform.position = new Vector3(0, 0, 0);
            terrain.name = "Terrain";
            terrain.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }

    /// <summary>
    /// Method to save the terrain mesh
    /// </summary>
    /// <param name="type">Type of terrain</param>
    private void SaveMesh (string type) {
        string name = "Assets/Meshs/" + type + ".asset";
        AssetDatabase.CreateAsset(terrain.GetComponent<MeshFilter>().sharedMesh, name);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Method for creating the vertices
    /// </summary>
    private void CreateVertices () {
        float constant;
        for (int z = 0; z <= ySize - 1; z++) {
            for (int x = 0; x <= xSize - 1; x++) {
                Vector2 uv = new Vector2((float)x / (float)xSize, (float)z / (float)ySize);
                float height;
                try {
                    height = heightmap.GetPixelBilinear(uv.x, uv.y).grayscale * maxHeight;
                } catch (UnityException) {
                    this.TextureIsRead(heightmap, true);
                    height = heightmap.GetPixelBilinear(uv.x, uv.y).grayscale * maxHeight;
                }
                vertices.Add(new Vector3(x, height, z));
                uvs.Add(uv);
                if (height <= maxHeight * 0.45) {
                    colors.Add(bottomColor);
                } else if (height > maxHeight * 0.45 && height <= maxHeight * 0.55) {
                    constant = (height - (maxHeight * 0.45f)) * 10;
                    colors.Add(Color.Lerp(bottomColor, middleColor, constant));
                } else if (height > maxHeight * 0.55 && height <= maxHeight * 0.75) {
                    colors.Add(middleColor);
                } else if (height > maxHeight * 0.75 && height < maxHeight * 0.85) {
                    constant = (height - (maxHeight * 0.75f)) * 10;
                    colors.Add(Color.Lerp(middleColor, topColor, constant));
                } else if (height > maxHeight * 0.85 && height <= maxHeight) {
                    colors.Add(topColor);
                }
            }
        }
        terrain.GetComponent<MeshFilter>().sharedMesh.vertices = vertices.ToArray();
        terrain.GetComponent<MeshFilter>().sharedMesh.colors = colors.ToArray();
        terrain.GetComponent<MeshFilter>().sharedMesh.uv = uvs.ToArray();
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
        terrain.GetComponent<MeshFilter>().sharedMesh.triangles = triangles.ToArray();
    }

    /// <summary>
    /// Method that cleans everything
    /// </summary>
    private void ClearLists () {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        colors.Clear();
        uvs.Clear();
    }
    #endregion

    #region CalculateNormals
    /// <summary>
    /// Method for calculating normal smooth
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
                Vector3 normal = Math.CrossProduct(v1, v2);
                normalMed += normal;
                flag++;
            }
            normalMed = normalMed / flag;
            normalMed.Normalize();
            normals.Add(normalMed);
        }
        terrain.GetComponent<MeshFilter>().sharedMesh.normals = normals.ToArray();
    }
    
    /// <summary>
    /// Method for calculating normal flat
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
                Vector3 normal = Math.CrossProduct(v1, v2);
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
        terrain.GetComponent<MeshFilter>().sharedMesh.normals = normals.ToArray();
    }
    #endregion
}