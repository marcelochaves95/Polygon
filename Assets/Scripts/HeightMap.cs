public readonly struct HeightMap
{
    public readonly float[,] Values;
    public readonly float MinValue;
    public readonly float MaxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        Values = values;
        MinValue = minValue;
        MaxValue = maxValue;
    }
}
