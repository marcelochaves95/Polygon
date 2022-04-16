using System;
using UnityEngine;

[Serializable]
public class NoiseSettings
{
    public ENormalizeMode NormalizeMode;

    public float Scale = 50;

    public int Octaves = 6;

    [Range(0, 1)]
    public float Persistance = .6f;
    public float Lacunarity = 2;

    public int Seed;
    public Vector2 Offset;

    public void ValidateValues()
    {
        Scale = Mathf.Max(Scale, 0.01f);
        Octaves = Mathf.Max(Octaves, 1);
        Lacunarity = Mathf.Max(Lacunarity, 1);
        Persistance = Mathf.Clamp01(Persistance);
    }
}