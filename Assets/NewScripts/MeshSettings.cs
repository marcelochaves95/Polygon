using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : ScriptableObject
{
    public bool UseFlatShading;
    public UISettings Height;
    public UISettings NoiseFrequency;
    public VertexData VertexData;

    public void SetDefaultValues()
    {
        UseFlatShading = true;
        Height.Value = 3f;
        NoiseFrequency.Value = 0.3f;
    }
}
