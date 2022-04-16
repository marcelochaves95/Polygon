using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class TextureData : UpdatableData
{
    [SerializeField]
	public Layer[] _layers;

    private const int TEXTURE_SIZE = 512;
    private const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB565;
	private float _savedMinHeight;
    private float _savedMaxHeight;

    private static readonly int _layerCount = Shader.PropertyToID("layerCount");
    private static readonly int _baseColours = Shader.PropertyToID("baseColours");
    private static readonly int _baseStartHeights = Shader.PropertyToID("baseStartHeights");
    private static readonly int _baseBlends = Shader.PropertyToID("baseBlends");
    private static readonly int _baseColourStrength = Shader.PropertyToID("baseColourStrength");
    private static readonly int _baseTextureScales = Shader.PropertyToID("baseTextureScales");
    private static readonly int _baseTextures = Shader.PropertyToID("baseTextures");
    private static readonly int _minHeight = Shader.PropertyToID("minHeight");
    private static readonly int _maxHeight = Shader.PropertyToID("maxHeight");

    public void ApplyToMaterial(Material material)
    {
        material.SetInt(_layerCount, _layers.Length);
        material.SetColorArray(_baseColours, _layers.Select(x => x.Tint).ToArray());
        material.SetFloatArray(_baseStartHeights, _layers.Select(x => x.StartHeight).ToArray());
        material.SetFloatArray(_baseBlends, _layers.Select(x => x.BlendStrength).ToArray());
        material.SetFloatArray(_baseColourStrength, _layers.Select(x => x.TintStrength).ToArray());
        material.SetFloatArray(_baseTextureScales, _layers.Select(x => x.TextureScale).ToArray());

        Texture2DArray texturesArray = GenerateTextureArray(_layers.Select(x => x.Texture).ToArray());
        material.SetTexture(_baseTextures, texturesArray);

        UpdateMeshHeights(material, _savedMinHeight, _savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        _savedMinHeight = minHeight;
        _savedMaxHeight = maxHeight;
        material.SetFloat(_minHeight, minHeight);
        material.SetFloat(_maxHeight, maxHeight);
    }

    private Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        var textureArray = new Texture2DArray(TEXTURE_SIZE, TEXTURE_SIZE, textures.Length, TEXTURE_FORMAT, true);

        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }

        textureArray.Apply();

        return textureArray;
    }
}
