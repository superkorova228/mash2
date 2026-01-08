using UnityEngine;
using System;

namespace RhythmHell.Core
{
    /// <summary>
    /// Данные настроек игры (сериализуемый класс для сохранения)
    /// </summary>
    [System.Serializable]
    public class GameSettings
    {
        // Audio
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;

        // Graphics
        public bool fullscreen = true;
        public int resolutionIndex = 0; // Индекс разрешения из списка доступных
        public int qualityLevel = 2; // 0-5 (Very Low - Very High)

        // Gameplay
        public float brightness = 1f;

        public GameSettings()
        {
            // Значения по умолчанию уже установлены выше
        }
    }

    /// <summary>
    /// Менеджер настроек игры. Сохраняет и загружает настройки через PlayerPrefs.
    /// Singleton - доступен из любой точки игры.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        private const string SETTINGS_KEY = "GameSettings"; // Ключ для PlayerPrefs

        [Header("Current Settings")]
        [SerializeField] private GameSettings currentSettings;

        // События для обновления UI и других систем
        public event Action<GameSettings> OnSettingsChanged;

        public GameSettings CurrentSettings => currentSettings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Загружаем настройки при старте
            LoadSettings();
        }

        /// <summary>
        /// Загрузить настройки из PlayerPrefs
        /// </summary>
        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                currentSettings = JsonUtility.FromJson<GameSettings>(json);
                Debug.Log("[SettingsManager] Settings loaded from PlayerPrefs");
            }
            else
            {
                // Первый запуск - создаём настройки по умолчанию
                currentSettings = new GameSettings();
                SaveSettings();
                Debug.Log("[SettingsManager] Created default settings");
            }

            // Применяем загруженные настройки
            ApplySettings();
        }

        /// <summary>
        /// Сохранить настройки в PlayerPrefs
        /// </summary>
        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("[SettingsManager] Settings saved to PlayerPrefs");

            // Уведомляем подписчиков
            OnSettingsChanged?.Invoke(currentSettings);
        }

        /// <summary>
        /// Применить все настройки
        /// </summary>
        private void ApplySettings()
        {
            ApplyAudioSettings();
            ApplyGraphicsSettings();

            OnSettingsChanged?.Invoke(currentSettings);
        }

        /// <summary>
        /// Применить аудио настройки
        /// </summary>
        private void ApplyAudioSettings()
        {
            // Здесь будем обращаться к AudioManager когда создадим его
            AudioListener.volume = currentSettings.masterVolume;
            
            Debug.Log($"[SettingsManager] Audio applied - Master: {currentSettings.masterVolume}");
        }

        /// <summary>
        /// Применить графические настройки
        /// </summary>
        private void ApplyGraphicsSettings()
        {
            // Fullscreen
            Screen.fullScreen = currentSettings.fullscreen;

            // Resolution
            if (currentSettings.resolutionIndex >= 0 && 
                currentSettings.resolutionIndex < Screen.resolutions.Length)
            {
                Resolution res = Screen.resolutions[currentSettings.resolutionIndex];
                Screen.SetResolution(res.width, res.height, currentSettings.fullscreen);
            }

            // Quality
            QualitySettings.SetQualityLevel(currentSettings.qualityLevel);

            Debug.Log($"[SettingsManager] Graphics applied - Fullscreen: {currentSettings.fullscreen}, Quality: {currentSettings.qualityLevel}");
        }

        // === PUBLIC МЕТОДЫ ДЛЯ ИЗМЕНЕНИЯ НАСТРОЕК ===

        public void SetMasterVolume(float volume)
        {
            currentSettings.masterVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
        }

        public void SetMusicVolume(float volume)
        {
            currentSettings.musicVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
        }

        public void SetSFXVolume(float volume)
        {
            currentSettings.sfxVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
        }

        public void SetFullscreen(bool isFullscreen)
        {
            currentSettings.fullscreen = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        public void SetResolution(int resolutionIndex)
        {
            currentSettings.resolutionIndex = resolutionIndex;
            ApplyGraphicsSettings();
        }

        public void SetQuality(int qualityLevel)
        {
            currentSettings.qualityLevel = Mathf.Clamp(qualityLevel, 0, 5);
            QualitySettings.SetQualityLevel(currentSettings.qualityLevel);
        }

        public void SetBrightness(float brightness)
        {
            currentSettings.brightness = Mathf.Clamp(brightness, 0.5f, 2f);
            // Применяется через post-processing или shader
        }

        /// <summary>
        /// Сбросить настройки к значениям по умолчанию
        /// </summary>
        public void ResetToDefaults()
        {
            currentSettings = new GameSettings();
            ApplySettings();
            SaveSettings();
            Debug.Log("[SettingsManager] Settings reset to defaults");
        }
    }
}