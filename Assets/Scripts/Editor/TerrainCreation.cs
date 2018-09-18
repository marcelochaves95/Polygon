using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class VertexData {
	public Vector3 position;
	public Vector2 uv;
	public Color color;
	public Vector3 normal;
};

[CanEditMultipleObjects]
public class TerrainCreation : EditorWindow {
	
	int GridResolution = 1024;
	static int Octavees = 8;

	static float NoiseScale = 1.0f;

	Color[] pix;

	Texture2D noiseTex;
	
	[MenuItem("Terrain/Create Terrain...")]
	static void Init () {
		EditorWindow.GetWindow<TerrainCreation>().Show();
	}

	public void OnGUI () {
		NoiseScale = EditorGUILayout.FloatField("Noise Scale", NoiseScale);
		Octavees = EditorGUILayout.IntSlider("Number of Octavees", Octavees, 0, 8);
		if (GUILayout.Button ("Create New Texture")) {
			CreateNoiseTexture();
		}

		// TODO Add your editor extension code here
	}

	void CreateNoiseTexture () {
		noiseTex = new Texture2D(GridResolution, GridResolution);
		pix = new Color[GridResolution * GridResolution];

		float xOri = UnityEngine.Random.value * 100000.0f;
		float yOri = UnityEngine.Random.value * 100000.0f;
		
		float y = 0.0f;
		while (y < noiseTex.height) {
			float x = 0.0f;
			while (x < noiseTex.width) {
				float xCoord = xOri + x / noiseTex.width * NoiseScale + Mathf.Sin(y);
				float yCoord = yOri + y / noiseTex.height * NoiseScale;

				float sample = OctaveesNoise2D(xOri + x / noiseTex.width, yOri + y / noiseTex.height, Octavees, 1.0f, 0.75f);

				pix[(int) y * noiseTex.width + (int) x] = new Color(sample, sample, sample);
                
				x++;
            }
            y++;
        }

        noiseTex.SetPixels(pix);
        noiseTex.Apply();

		byte[] bytes = noiseTex.EncodeToPNG();

		Debug.Log("Creating Terrain Texture: " + Application.dataPath + "/TerrainTexture.png");

		File.WriteAllBytes(Application.dataPath + "/TerrainTexture.png", bytes);

		AssetDatabase.ImportAsset("Assets/TerrainTexture.png");
	}

	public float OctaveesNoise2D (float x, float y, int octNum, float frq, float amp) {
		float gain = 1.0f;
		float sum = 0.0f;

		for (int i = 0; i < octNum; i++) {
			sum +=  Mathf.PerlinNoise(x * gain / frq, y * gain / frq) * amp / gain;
			gain *= 2.0f;
		}
		return sum;
	}

	public void Test () {
		Vector2 uv = new Vector2();
		float maxHeight;
		Texture2D heightMap;
		List<Vector3> vertices;
		List<Vector2> uvs;
		List<Color> cor;
	}
}