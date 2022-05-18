using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : ScriptableObject
{
    public bool UseFlatShading;
    public UISettings Height;
    public UISettings NoiseFrequency;
    public VertexSettings vertexSettings;

    public void SetDefaultValues()
    {
        UseFlatShading = true;
        Height.Value = 3f;
        NoiseFrequency.Value = 0.3f;
    }
}
