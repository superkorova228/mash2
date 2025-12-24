using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace mash2.UI.Controls
{
    /// <summary>
    /// Control для переключения разрешений экрана
    /// </summary>
    public class ResolutionControl : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI resolutionText;

        [Header("Settings")]
        [SerializeField] private string labelText = "Resolution:";

        // События
        public event Action<int> OnResolutionChanged;

        private Resolution[] availableResolutions;
        private int currentIndex = 0;

        private void Awake()
        {
            // Auto-find components
            if (label == null)
                label = transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (leftButton == null)
                leftButton = transform.Find("LeftButton")?.GetComponent<Button>();
            if (rightButton == null)
                rightButton = transform.Find("RightButton")?.GetComponent<Button>();
            if (resolutionText == null)
                resolutionText = transform.Find("ResolutionText")?.GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (label != null)
                label.text = labelText;

            if (leftButton != null)
                leftButton.onClick.AddListener(OnLeftClicked);

            if (rightButton != null)
                rightButton.onClick.AddListener(OnRightClicked);

            // Получаем разрешения от SettingsManager
            if (mash2.Core.SettingsManager.Instance != null)
            {
                availableResolutions = mash2.Core.SettingsManager.Instance.AvailableResolutions;
                currentIndex = mash2.Core.SettingsManager.Instance.CurrentSettings.resolutionIndex;
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (leftButton != null)
                leftButton.onClick.RemoveListener(OnLeftClicked);
            if (rightButton != null)
                rightButton.onClick.RemoveListener(OnRightClicked);
        }

        private void OnLeftClicked()
        {
            if (availableResolutions == null || availableResolutions.Length == 0)
                return;

            currentIndex--;
            if (currentIndex < 0)
                currentIndex = availableResolutions.Length - 1;

            UpdateDisplay();
            OnResolutionChanged?.Invoke(currentIndex);
        }

        private void OnRightClicked()
        {
            if (availableResolutions == null || availableResolutions.Length == 0)
                return;

            currentIndex++;
            if (currentIndex >= availableResolutions.Length)
                currentIndex = 0;

            UpdateDisplay();
            OnResolutionChanged?.Invoke(currentIndex);
        }

        private void UpdateDisplay()
        {
            if (resolutionText == null)
                return;

            if (availableResolutions != null && currentIndex >= 0 && currentIndex < availableResolutions.Length)
            {
                Resolution res = availableResolutions[currentIndex];
                resolutionText.text = $"{res.width} x {res.height}";
            }
            else
            {
                resolutionText.text = "Unknown";
            }
        }

        // ============================================
        // ПУБЛИЧНЫЕ МЕТОДЫ
        // ============================================

        public void SetResolutionIndex(int index)
        {
            if (availableResolutions != null && index >= 0 && index < availableResolutions.Length)
            {
                currentIndex = index;
                UpdateDisplay();
            }
        }

        public int GetResolutionIndex()
        {
            return currentIndex;
        }

        public void SetLabel(string text)
        {
            labelText = text;
            if (label != null)
                label.text = text;
        }
    }
}