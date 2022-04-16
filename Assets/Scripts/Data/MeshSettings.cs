using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdatableData
{
	[Range(0, NUM_SUPPORTED_CHUNK_SIZES - 1)]
    public int ChunkSizeIndex;

    [Range(0, NUM_SUPPORTED_FLAT_CHUNK_SIZES - 1)]
    public int FlatChunkSizeIndex;

    public const int NUM_SUPPORTED_LODS = 5;
	public float MeshScale = 2.5f;
    public bool UseFlatShading;
    private const int NUM_SUPPORTED_CHUNK_SIZES = 9;
    private const int NUM_SUPPORTED_FLAT_CHUNK_SIZES = 3;
    private static readonly int[] _supportedChunkSizes =
    {
        48, 72, 96, 120, 144,
        168, 192, 216, 240
    };

	public int NumVertsPerLine => _supportedChunkSizes[UseFlatShading ? FlatChunkSizeIndex : ChunkSizeIndex] + 5;
    public float MeshWorldSize => (NumVertsPerLine - 3) * MeshScale;
}
