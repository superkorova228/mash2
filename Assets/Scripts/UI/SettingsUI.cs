using UnityEngine;
using UnityEngine.UI;
using TMPro;
using mash2.Core;
using mash2.Data;

namespace mash2.UI
{
    /// <summary>
    /// Управляет UI меню настроек
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeLabel;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeLabel;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeLabel;

        [Header("Graphics Controls")]
        [SerializeField] private Button resolutionLeftButton;
        [SerializeField] private Button resolutionRightButton;
        [SerializeField] private TextMeshProUGUI resolutionText;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Gameplay Controls")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI mouseSensitivityLabel;

        [Header("Action Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;

        // Временные настройки (пока не нажата Apply)
        private SettingsData tempSettings;
        
        // Текущий индекс разрешения
        private int currentResolutionIndex = 0;

        private void Start()
        {
            // Создаём копию текущих настроек для редактирования
            tempSettings = new SettingsData();
            
            if (SettingsManager.Instance != null)
            {
                tempSettings.CopyFrom(SettingsManager.Instance.CurrentSettings);
                currentResolutionIndex = tempSettings.resolutionIndex;
            }
            
            // Инициализируем UI
            InitializeUI();
            
            // Подписываемся на события UI
            SubscribeToUIEvents();
            
            // Обновляем отображение
            UpdateUI();
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            UnsubscribeFromUIEvents();
        }

        /// <summary>
        /// Инициализирует UI значениями из настроек
        /// </summary>
        private void InitializeUI()
        {
            // Audio
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = tempSettings.masterVolume;
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = tempSettings.musicVolume;
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = tempSettings.sfxVolume;

            // Graphics
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = tempSettings.isFullscreen;

            // Gameplay
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.value = tempSettings.mouseSensitivity;
        }

        /// <summary>
        /// Подписываемся на события UI элементов
        /// </summary>
        private void SubscribeToUIEvents()
        {
            // Audio sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            // Graphics
            if (resolutionLeftButton != null)
                resolutionLeftButton.onClick.AddListener(OnResolutionLeft);
            if (resolutionRightButton != null)
                resolutionRightButton.onClick.AddListener(OnResolutionRight);
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

            // Gameplay
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

            // Buttons
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void UnsubscribeFromUIEvents()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            
            if (resolutionLeftButton != null)
                resolutionLeftButton.onClick.RemoveListener(OnResolutionLeft);
            if (resolutionRightButton != null)
                resolutionRightButton.onClick.RemoveListener(OnResolutionRight);
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
            
            if (applyButton != null)
                applyButton.onClick.RemoveListener(OnApplyClicked);
            if (resetButton != null)
                resetButton.onClick.RemoveListener(OnResetClicked);
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);
        }

        // ============================================
        // ОБРАБОТЧИКИ ИЗМЕНЕНИЙ UI
        // ============================================

        private void OnMasterVolumeChanged(float value)
        {
            tempSettings.masterVolume = value;
            UpdateUI();
            
            // Применяем громкость сразу для предпросмотра
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetMasterVolume(value);
        }

        private void OnMusicVolumeChanged(float value)
        {
            tempSettings.musicVolume = value;
            UpdateUI();
            
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetMusicVolume(value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            tempSettings.sfxVolume = value;
            UpdateUI();
            
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetSFXVolume(value);
        }

        private void OnResolutionLeft()
        {
            if (SettingsManager.Instance == null) return;
            
            currentResolutionIndex--;
            if (currentResolutionIndex < 0)
                currentResolutionIndex = SettingsManager.Instance.AvailableResolutions.Length - 1;
            
            tempSettings.resolutionIndex = currentResolutionIndex;
            UpdateUI();
        }

        private void OnResolutionRight()
        {
            if (SettingsManager.Instance == null) return;
            
            currentResolutionIndex++;
            if (currentResolutionIndex >= SettingsManager.Instance.AvailableResolutions.Length)
                currentResolutionIndex = 0;
            
            tempSettings.resolutionIndex = currentResolutionIndex;
            UpdateUI();
        }

        private void OnFullscreenChanged(bool isOn)
        {
            tempSettings.isFullscreen = isOn;
        }

        private void OnMouseSensitivityChanged(float value)
        {
            tempSettings.mouseSensitivity = value;
            UpdateUI();
        }

        // ============================================
        // ОБРАБОТЧИКИ КНОПОК
        // ============================================

        private void OnApplyClicked()
        {
            Debug.Log("Applying settings...");
            
            if (SettingsManager.Instance != null)
            {
                // Копируем временные настройки в реальные
                SettingsManager.Instance.CurrentSettings.CopyFrom(tempSettings);
                
                // Применяем и сохраняем
                SettingsManager.Instance.ApplySettings();
                SettingsManager.Instance.SaveSettings();
                
                Debug.Log("Settings applied and saved!");
            }
        }

        private void OnResetClicked()
        {
            Debug.Log("Resetting settings to default...");
            
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ResetToDefault();
                
                // Обновляем временные настройки
                tempSettings.CopyFrom(SettingsManager.Instance.CurrentSettings);
                currentResolutionIndex = tempSettings.resolutionIndex;
                
                // Обновляем UI
                InitializeUI();
                UpdateUI();
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("Going back to Main Menu...");
            
            // Можно показать диалог "Применить изменения?" если настройки изменены
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(1); // MainMenu
            }
        }

        // ============================================
        // ОБНОВЛЕНИЕ UI
        // ============================================

        /// <summary>
        /// Обновляет отображение всех значений в UI
        /// </summary>
        private void UpdateUI()
        {
            // Audio labels
            if (masterVolumeLabel != null)
                masterVolumeLabel.text = $"Master Volume: {Mathf.RoundToInt(tempSettings.masterVolume * 100)}%";
            
            if (musicVolumeLabel != null)
                musicVolumeLabel.text = $"Music Volume: {Mathf.RoundToInt(tempSettings.musicVolume * 100)}%";
            
            if (sfxVolumeLabel != null)
                sfxVolumeLabel.text = $"SFX Volume: {Mathf.RoundToInt(tempSettings.sfxVolume * 100)}%";

            // Resolution
            if (resolutionText != null && SettingsManager.Instance != null)
            {
                resolutionText.text = SettingsManager.Instance.GetResolutionString(currentResolutionIndex);
            }

            // Mouse sensitivity
            if (mouseSensitivityLabel != null)
                mouseSensitivityLabel.text = $"Mouse Sensitivity: {tempSettings.mouseSensitivity:F1}";
        }
    }
}