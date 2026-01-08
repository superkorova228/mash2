using UnityEngine;
using UnityEngine.UI;

namespace RhythmHell.UI
{
    /// <summary>
    /// Управление UI панели настроек. Синхронизируется с SettingsManager.
    /// </summary>
    public class SettingsPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button backButton;

        private Core.SettingsManager settingsManager;
        private bool isInitialized = false;

        private void OnEnable()
        {
            // Получаем ссылку на SettingsManager
            settingsManager = Core.SettingsManager.Instance;

            if (settingsManager != null)
            {
                // Загружаем текущие значения в UI
                LoadCurrentSettings();

                // Подписываемся на изменения слайдеров/тоглов
                SubscribeToUIEvents();

                isInitialized = true;
            }
        }

        private void OnDisable()
        {
            // Отписываемся от событий
            UnsubscribeFromUIEvents();
        }

        /// <summary>
        /// Загрузить текущие настройки в UI элементы
        /// </summary>
        private void LoadCurrentSettings()
        {
            var settings = settingsManager.CurrentSettings;

            // Временно отключаем события чтобы избежать зацикливания
            isInitialized = false;

            if (masterVolumeSlider != null)
                masterVolumeSlider.value = settings.masterVolume;

            if (brightnessSlider != null)
                brightnessSlider.value = settings.brightness;

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = settings.fullscreen;

            isInitialized = true;
        }

        /// <summary>
        /// Подписаться на события UI
        /// </summary>
        private void SubscribeToUIEvents()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (brightnessSlider != null)
                brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        /// <summary>
        /// Отписаться от событий UI
        /// </summary>
        private void UnsubscribeFromUIEvents()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

            if (brightnessSlider != null)
                brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);

            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);

            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);
        }

        // === ОБРАБОТЧИКИ ИЗМЕНЕНИЙ ===

        private void OnMasterVolumeChanged(float value)
        {
            if (!isInitialized) return;
            settingsManager.SetMasterVolume(value);
        }

        private void OnBrightnessChanged(float value)
        {
            if (!isInitialized) return;
            settingsManager.SetBrightness(value);
        }

        private void OnFullscreenChanged(bool isOn)
        {
            if (!isInitialized) return;
            settingsManager.SetFullscreen(isOn);
        }

        private void OnBackClicked()
        {
            // Сохраняем настройки перед выходом
            settingsManager.SaveSettings();

            // Возвращаемся в главное меню
            var mainMenu = FindObjectOfType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.ShowMainMenu();
            }
        }
    }
}