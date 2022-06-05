using System;
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
            _sliderHeight.value = _meshSettings.Height;
            _sliderNoiseFrequency.value = _meshSettings.NoiseFrequency * 10;
            _textHeight.text = _meshSettings.Height.ToString();
            _textNoiseFrequency.text = _meshSettings.NoiseFrequency.ToString();
            switch (_meshSettings.GenerationMode)
            {
                case GenerationMode.Flat:
                    _toggleUseFlatShading.isOn = true;
                    break;
                case GenerationMode.Smooth:
                    _toggleUseFlatShading.isOn = false;
                    break;
            }
    
            UpdateUI();
        }

        public void MakeFlat()
        {
            _meshSettings.GenerationMode = _toggleUseFlatShading.isOn ? GenerationMode.Flat : GenerationMode.Smooth;
            UpdateUI();
        }
    
        public void UpdateHeight()
        {
            _meshSettings.Height = (int) _sliderHeight.value;
            _textHeight.text = _meshSettings.Height.ToString();
            UpdateUI();
        }
    
        public void UpdateNoiseFrequency()
        {
            _meshSettings.NoiseFrequency = _sliderNoiseFrequency.value * 0.1f;
            _textNoiseFrequency.text = _meshSettings.NoiseFrequency.ToString();
            UpdateUI();
        }
    }
}
