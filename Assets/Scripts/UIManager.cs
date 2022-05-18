using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Polygon
{
    public class UIManager : MonoBehaviour
    {
        public static event Action<MeshSettings> OnUpdateUI;
    
        [SerializeField] private MeshSettings _meshSettings;
        [SerializeField] private Slider _sliderHeight;
        [SerializeField] private Slider _sliderNoiseFrequency;
        [SerializeField] private Text _textHeight;
        [SerializeField] private Text _textNoiseFrequency;
        [SerializeField] private Toggle _toggleUseFlatShading;
    
        private void Start()
        {
            SetupUI();
        }
    
        private void UpdateUI()
        {
            OnUpdateUI?.Invoke(_meshSettings);
        }
    
        private void SetupUI()
        {
            _meshSettings.SetDefaultValues();
            _sliderHeight.value = _meshSettings.Height.Value;
            _sliderNoiseFrequency.value = (int) (_meshSettings.NoiseFrequency.Value * 10);
            _textHeight.text = (_meshSettings.Height.Value - 2).ToString(CultureInfo.InvariantCulture);
            _textNoiseFrequency.text = ((int) (_meshSettings.NoiseFrequency.Value * 10)).ToString();
            _toggleUseFlatShading.isOn = _meshSettings.UseFlatShading;
    
            UpdateUI();
        }
    
        public void MakeFlat()
        {
            _meshSettings.UseFlatShading = !_meshSettings.UseFlatShading;
            UpdateUI();
        }
    
        public void UpdateHeight()
        {
            _meshSettings.Height.Value = (int) _sliderHeight.value;
            _textHeight.text = (_meshSettings.Height.Value - 2).ToString(CultureInfo.InvariantCulture);
            UpdateUI();
        }
    
        public void UpdateNoiseFrequency()
        {
            _meshSettings.NoiseFrequency.Value = _sliderNoiseFrequency.value * 0.1f;
            _textNoiseFrequency.text = ((int) (_meshSettings.NoiseFrequency.Value * 10)).ToString();
            UpdateUI();
        }
    }
}
