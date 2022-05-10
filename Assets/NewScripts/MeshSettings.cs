using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : ScriptableObject
{
    public bool UseFlatShading;
    public bool UseDelay;
    public float Delay;
    public UISettings Height;
    public UISettings NoiseFrequency;
    public VertexData VertexData;
}