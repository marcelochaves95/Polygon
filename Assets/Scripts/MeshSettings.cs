using UnityEngine;

namespace Polygon
{
    [CreateAssetMenu]
    public class MeshSettings : ScriptableObject
    {
        public GenerationMode GenerationMode;
        public float Height;
        public float NoiseFrequency;
        public VertexSettings vertexSettings;

        public void SetDefaultValues()
        {
            GenerationMode = GenerationMode.Flat;
            Height = 3f;
            NoiseFrequency = 0.3f;
        }
    }
}
