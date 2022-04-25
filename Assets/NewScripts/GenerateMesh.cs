using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class GenerateMesh : MonoBehaviour
{
    private int _height = 3;
    private float _noiseFrequency = 0.3f;
    private bool _useFlatShading;
    private MeshData _meshData;

    [SerializeField] private float _delay;
    [SerializeField] private Slider _sliderHeight;
    [SerializeField] private Slider _sliderNoiseFrequency;
    [SerializeField] private Text _textHeight;
    [SerializeField] private Text _textNoiseFrequency;
    [SerializeField] private VertexData _vertexData;

    private void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        _meshData = new MeshData(mesh, _vertexData, this, _delay);
        _meshData.GenerateMesh(_useFlatShading, _height, _noiseFrequency);
        _sliderHeight.value = _height;
        _sliderNoiseFrequency.value = (int) (_noiseFrequency * 10);
        _textHeight.text = (_height - 2).ToString();
        _textNoiseFrequency.text = ((int) (_noiseFrequency * 10)).ToString();
    }

    public void MakeFlat()
    {
        _useFlatShading = !_useFlatShading;
        _meshData.GenerateMesh(_useFlatShading, _height, _noiseFrequency);
    }
    
    public void ChangeHeight()
    {
         _height = (int) _sliderHeight.value;
         _meshData.GenerateMesh(_useFlatShading, _height, _noiseFrequency);
         _textHeight.text = (_height - 2).ToString();
    }
    
    public void ChangeNoiseFrequency()
    {
        _noiseFrequency = _sliderNoiseFrequency.value * 0.1f;
        _meshData.GenerateMesh(_useFlatShading, _height, _noiseFrequency);
        _textNoiseFrequency.text = ((int) (_noiseFrequency * 10)).ToString();
    }
}