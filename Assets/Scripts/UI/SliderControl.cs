using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace mash2.UI.Controls
{
    /// <summary>
    /// Универсальный slider control с label и value display
    /// </summary>
    public class SliderControl : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI valueText;

        [Header("Settings")]
        [SerializeField] private string labelText = "Setting:";
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 1f;
        [SerializeField] private float currentValue = 0.5f;
        [SerializeField] private bool showAsPercentage = true;
        [SerializeField] private string valueFormat = "F0"; // 0 decimal places
        [SerializeField] private string valueSuffix = "%";

        // События
        public event Action<float> OnValueChanged;

        private void Awake()
        {
            // Находим компоненты автоматически, если не назначены
            if (label == null)
                label = transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (slider == null)
                slider = transform.Find("Slider")?.GetComponent<Slider>();
            if (valueText == null)
                valueText = transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            InitializeSlider();
            
            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnSliderValueChanged);
            }
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            }
        }

        /// <summary>
        /// Инициализирует slider с текущими настройками
        /// </summary>
        private void InitializeSlider()
        {
            if (label != null)
                label.text = labelText;

            if (slider != null)
            {
                slider.minValue = minValue;
                slider.maxValue = maxValue;
                slider.value = currentValue;
            }

            UpdateValueDisplay();
        }

        /// <summary>
        /// Обработчик изменения значения slider
        /// </summary>
        private void OnSliderValueChanged(float value)
        {
            currentValue = value;
            UpdateValueDisplay();
            OnValueChanged?.Invoke(value);
        }

        /// <summary>
        /// Обновляет текст с текущим значением
        /// </summary>
        private void UpdateValueDisplay()
        {
            if (valueText == null)
                return;

            float displayValue = currentValue;
            
            if (showAsPercentage)
            {
                displayValue = Mathf.Lerp(0, 100, Mathf.InverseLerp(minValue, maxValue, currentValue));
            }

            valueText.text = displayValue.ToString(valueFormat) + valueSuffix;
        }

        // ============================================
        // ПУБЛИЧНЫЕ МЕТОДЫ
        // ============================================

        public void SetLabel(string text)
        {
            labelText = text;
            if (label != null)
                label.text = text;
        }

        public void SetValue(float value)
        {
            currentValue = Mathf.Clamp(value, minValue, maxValue);
            if (slider != null)
                slider.value = currentValue;
            UpdateValueDisplay();
        }

        public float GetValue()
        {
            return currentValue;
        }

        public void SetRange(float min, float max)
        {
            minValue = min;
            maxValue = max;
            
            if (slider != null)
            {
                slider.minValue = min;
                slider.maxValue = max;
            }
            
            UpdateValueDisplay();
        }

        public void SetValueFormat(bool asPercentage, string format = "F0", string suffix = "%")
        {
            showAsPercentage = asPercentage;
            valueFormat = format;
            valueSuffix = suffix;
            UpdateValueDisplay();
        }

        /// <summary>
        /// Настраивает контрол для процентов (0-100%)
        /// </summary>
        public void SetupAsPercentage(string labelText, float initialValue = 0.8f)
        {
            SetLabel(labelText);
            SetRange(0f, 1f);
            SetValueFormat(true, "F0", "%");
            SetValue(initialValue);
        }

        /// <summary>
        /// Настраивает контрол для чувствительности (0.1-2.0)
        /// </summary>
        public void SetupAsSensitivity(string labelText, float initialValue = 1f)
        {
            SetLabel(labelText);
            SetRange(0.1f, 2f);
            SetValueFormat(false, "F1", "x");
            SetValue(initialValue);
        }
    }
}
