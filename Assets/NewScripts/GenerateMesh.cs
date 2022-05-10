using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateMesh : MonoBehaviour
{
    private MeshData _meshData;

    [SerializeField] private MeshSettings _meshSettings;

    [Header("UI")]
    [SerializeField] private Slider _sliderHeight;
    [SerializeField] private Slider _sliderNoiseFrequency;
    [SerializeField] private Text _textHeight;
    [SerializeField] private Text _textNoiseFrequency;
    [SerializeField] private Toggle _toggleUseFlatShading;

    private void Start()
    {
        if (!_meshSettings)
        {
            return;
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        _meshSettings.SetDefaultValues();
        _meshData = new MeshData(mesh);
        _meshData.GenerateMesh(_meshSettings);

        SetupUI();
    }

    private void SetupUI()
    {
        if (!_meshSettings)
        {
            return;
        }

        _sliderHeight.value = _meshSettings.Height.Value;
        _sliderNoiseFrequency.value = (int) (_meshSettings.NoiseFrequency.Value * 10);
        _textHeight.text = (_meshSettings.Height.Value - 2).ToString(CultureInfo.InvariantCulture);
        _textNoiseFrequency.text = ((int) (_meshSettings.NoiseFrequency.Value * 10)).ToString();
        _toggleUseFlatShading.isOn = _meshSettings.UseFlatShading;
    }

    public void MakeFlat()
    {
        _meshSettings.UseFlatShading = !_meshSettings.UseFlatShading;
        _meshData.GenerateMesh(_meshSettings);
    }

    public void ChangeHeight()
    {
        _meshSettings.Height.Value = (int) _sliderHeight.value;
        _textHeight.text = (_meshSettings.Height.Value - 2).ToString(CultureInfo.InvariantCulture);
        _meshData.GenerateMesh(_meshSettings);
    }

    public void ChangeNoiseFrequency()
    {
        _meshSettings.NoiseFrequency.Value = _sliderNoiseFrequency.value * 0.1f;
        _textNoiseFrequency.text = ((int) (_meshSettings.NoiseFrequency.Value * 10)).ToString();
        _meshData.GenerateMesh(_meshSettings);
    }
}
