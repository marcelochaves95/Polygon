using System;
using UnityEngine;

[Serializable]
public class Layer
{
    [Range(0, 1)]
    public float TintStrength;

    [Range(0, 1)]
    public float StartHeight;

    [Range(0, 1)]
    public float BlendStrength;

    public Texture2D Texture;
    public Color Tint;
    public float TextureScale;
}
