using System;
using System.IO;
using UnityEngine;
using mash2.Data;

namespace mash2.Core
{
    /// <summary>
    /// Управляет настройками игры: сохранение, загрузка, применение
    /// Singleton - доступен из любого места
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private SettingsData currentSettings;
        
        // Публичный доступ к настройкам (только чтение)
        public SettingsData CurrentSettings => currentSettings;
        
        // События для уведомления других систем
        public event Action<SettingsData> OnSettingsChanged;
        public event Action<SettingsData> OnSettingsLoaded;
        public event Action<SettingsData> OnSettingsSaved;
        
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "settings.json";
        [SerializeField] private bool usePersistentDataPath = true; // true = сохранение в папку игры
        
        private string SaveFilePath => usePersistentDataPath 
            ? Path.Combine(Application.persistentDataPath, saveFileName)
            : Path.Combine(Application.dataPath, saveFileName);
        
        // Доступные разрешения экрана
        private Resolution[] availableResolutions;
        public Resolution[] AvailableResolutions => availableResolutions;

        private void Awake()
        {
            // Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Получаем список доступных разрешений
            InitializeResolutions();
            
            Debug.Log($"SettingsManager initialized. Save path: {SaveFilePath}");
        }

        private void Start()
        {
            // Загружаем настройки при старте
            LoadSettings();
        }

        /// <summary>
        /// Инициализирует список доступных разрешений экрана
        /// </summary>
        private void InitializeResolutions()
        {
            availableResolutions = Screen.resolutions;
            
            // Фильтруем дубликаты (разные refresh rate)
            System.Collections.Generic.List<Resolution> uniqueResolutions = new System.Collections.Generic.List<Resolution>();
            
            foreach (Resolution res in availableResolutions)
            {
                // Добавляем только если такого разрешения ещё нет
                bool isDuplicate = false;
                foreach (Resolution unique in uniqueResolutions)
                {
                    if (res.width == unique.width && res.height == unique.height)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                
                if (!isDuplicate)
                    uniqueResolutions.Add(res);
            }
            
            availableResolutions = uniqueResolutions.ToArray();
            
            Debug.Log($"Found {availableResolutions.Length} unique resolutions");
        }

        // ============================================
        // СОХРАНЕНИЕ И ЗАГРУЗКА
        // ============================================

        /// <summary>
        /// Загружает настройки из файла (или создаёт новые)
        /// </summary>
        public void LoadSettings()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    currentSettings = JsonUtility.FromJson<SettingsData>(json);
                    
                    Debug.Log($"Settings loaded from: {SaveFilePath}");
                    
                    // Применяем загруженные настройки
                    ApplySettings();
                    
                    OnSettingsLoaded?.Invoke(currentSettings);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load settings: {e.Message}");
                    CreateDefaultSettings();
                }
            }
            else
            {
                Debug.Log("Settings file not found. Creating default settings.");
                CreateDefaultSettings();
            }
        }

        /// <summary>
        /// Сохраняет текущие настройки в файл
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(currentSettings, true); // true = pretty print
                
                // Создаём папку, если её нет
                string directory = Path.GetDirectoryName(SaveFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(SaveFilePath, json);
                
                Debug.Log($"Settings saved to: {SaveFilePath}");
                
                OnSettingsSaved?.Invoke(currentSettings);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save settings: {e.Message}");
            }
        }

        /// <summary>
        /// Создаёт и сохраняет настройки по умолчанию
        /// </summary>
        public void CreateDefaultSettings()
        {
            currentSettings = SettingsData.CreateDefault();
            
            // Устанавливаем разрешение на текущее
            currentSettings.resolutionIndex = GetCurrentResolutionIndex();
            
            ApplySettings();
            SaveSettings();
            
            Debug.Log("Default settings created and applied.");
        }

        /// <summary>
        /// Сбрасывает настройки к значениям по умолчанию
        /// </summary>
        public void ResetToDefault()
        {
            Debug.Log("Resetting settings to default...");
            CreateDefaultSettings();
            OnSettingsChanged?.Invoke(currentSettings);
        }

        // ============================================
        // ПРИМЕНЕНИЕ НАСТРОЕК
        // ============================================

        /// <summary>
        /// Применяет все настройки к игре
        /// </summary>
        public void ApplySettings()
        {
            ApplyAudioSettings();
            ApplyGraphicsSettings();
            ApplyGameplaySettings();
            
            Debug.Log("All settings applied.");
            OnSettingsChanged?.Invoke(currentSettings);
        }

        /// <summary>
        /// Применяет аудио настройки
        /// </summary>
        private void ApplyAudioSettings()
        {
            // TODO: Когда создадим AudioManager, будет так:
            // AudioManager.Instance.SetMasterVolume(currentSettings.masterVolume);
            // AudioManager.Instance.SetMusicVolume(currentSettings.musicVolume);
            // AudioManager.Instance.SetSFXVolume(currentSettings.sfxVolume);
            
            AudioListener.volume = currentSettings.masterVolume;
            
            Debug.Log($"Audio applied: Master={currentSettings.masterVolume:F2}");
        }

        /// <summary>
        /// Применяет графические настройки
        /// </summary>
        private void ApplyGraphicsSettings()
        {
            // Применяем разрешение
            if (currentSettings.resolutionIndex >= 0 && currentSettings.resolutionIndex < availableResolutions.Length)
            {
                Resolution res = availableResolutions[currentSettings.resolutionIndex];
                Screen.SetResolution(res.width, res.height, currentSettings.isFullscreen);
                
                Debug.Log($"Resolution applied: {res.width}x{res.height}, Fullscreen={currentSettings.isFullscreen}");
            }
            
            // Применяем качество графики
            QualitySettings.SetQualityLevel(currentSettings.qualityLevel);
            
            Debug.Log($"Quality level set to: {currentSettings.qualityLevel}");
        }

        /// <summary>
        /// Применяет игровые настройки
        /// </summary>
        private void ApplyGameplaySettings()
        {
            // Mouse sensitivity будет использоваться в PlayerController
            // ShowFPS будет использоваться в FPSCounter (если создадим)
            
            Debug.Log($"Gameplay settings: MouseSens={currentSettings.mouseSensitivity:F2}");
        }

        // ============================================
        // ИЗМЕНЕНИЕ ОТДЕЛЬНЫХ НАСТРОЕК
        // ============================================

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

        public void SetResolution(int resolutionIndex)
        {
            if (resolutionIndex >= 0 && resolutionIndex < availableResolutions.Length)
            {
                currentSettings.resolutionIndex = resolutionIndex;
                ApplyGraphicsSettings();
            }
        }

        public void SetFullscreen(bool isFullscreen)
        {
            currentSettings.isFullscreen = isFullscreen;
            ApplyGraphicsSettings();
        }

        public void SetQualityLevel(int level)
        {
            currentSettings.qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            ApplyGraphicsSettings();
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            currentSettings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 2f);
            ApplyGameplaySettings();
        }

        public void SetShowFPS(bool show)
        {
            currentSettings.showFPS = show;
        }

        // ============================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ============================================

        /// <summary>
        /// Получает индекс текущего разрешения в списке доступных
        /// </summary>
        private int GetCurrentResolutionIndex()
        {
            Resolution current = Screen.currentResolution;
            
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                if (availableResolutions[i].width == current.width &&
                    availableResolutions[i].height == current.height)
                {
                    return i;
                }
            }
            
            return 0; // По умолчанию первое в списке
        }

        /// <summary>
        /// Возвращает строковое представление разрешения
        /// </summary>
        public string GetResolutionString(int index)
        {
            if (index >= 0 && index < availableResolutions.Length)
            {
                Resolution res = availableResolutions[index];
                return $"{res.width} x {res.height}";
            }
            
            return "Unknown";
        }

        /// <summary>
        /// Возвращает текущее разрешение как строку
        /// </summary>
        public string GetCurrentResolutionString()
        {
            return GetResolutionString(currentSettings.resolutionIndex);
        }
    }
}