using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdatableData
{
    public NoiseSettings NoiseSettings;
    public AnimationCurve HeightCurve;
	public float HeightMultiplier;

    public float MinHeight => HeightMultiplier * HeightCurve.Evaluate(0);
    public float MaxHeight => HeightMultiplier * HeightCurve.Evaluate(1);

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        NoiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}
