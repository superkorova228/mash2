using System;
using UnityEngine;

namespace mash2.Data
{
    /// <summary>
    /// Данные настроек игры.
    /// [Serializable] позволяет сохранить этот класс в JSON
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 0.8f;        // Общая громкость (0-1)
        
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;         // Громкость музыки
        
        [Range(0f, 1f)]
        public float sfxVolume = 1.0f;           // Громкость звуковых эффектов
        
        [Header("Graphics Settings")]
        public int resolutionIndex = 0;          // Индекс разрешения из списка доступных
        public bool isFullscreen = true;         // Полноэкранный режим
        public int qualityLevel = 2;             // Уровень качества графики (0-5)
        
        [Header("Gameplay Settings")]
        [Range(0.1f, 2f)]
        public float mouseSensitivity = 1.0f;    // Чувствительность мыши
        
        public bool showFPS = false;             // Показывать FPS счётчик
        
        [Header("Language")]
        public string language = "en";           // Язык игры (en, ru, и т.д.)

        /// <summary>
        /// Создаёт настройки по умолчанию
        /// </summary>
        public static SettingsData CreateDefault()
        {
            return new SettingsData
            {
                masterVolume = 0.8f,
                musicVolume = 0.7f,
                sfxVolume = 1.0f,
                resolutionIndex = 0,
                isFullscreen = true,
                qualityLevel = 2,
                mouseSensitivity = 1.0f,
                showFPS = false,
                language = "en"
            };
        }

        /// <summary>
        /// Копирует значения из другого объекта SettingsData
        /// </summary>
        public void CopyFrom(SettingsData other)
        {
            masterVolume = other.masterVolume;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            resolutionIndex = other.resolutionIndex;
            isFullscreen = other.isFullscreen;
            qualityLevel = other.qualityLevel;
            mouseSensitivity = other.mouseSensitivity;
            showFPS = other.showFPS;
            language = other.language;
        }
    }
}