using System;
using UnityEngine;

namespace Polygon
{
    [Serializable]
    public class VertexSettings
    {
        public int XSize = 20;
        public int ZSize = 20;
        public Gradient Gradient;
        public float MinTerrainHeight;
        public float MaxTerrainHeight;
    }
}
