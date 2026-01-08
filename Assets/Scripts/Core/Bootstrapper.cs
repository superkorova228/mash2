using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythmHell.Core
{
    /// <summary>
    /// Точка входа в игру. Инициализирует все основные системы и загружает MainMenu.
    /// Эта сцена должна быть первой в Build Settings.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool skipToGameplay = false; // Для быстрого тестирования

        private void Awake()
        {
            // Инициализируем все синглтоны и системы здесь
            InitializeCoreServices();
        }

        private void Start()
        {
            // Загружаем главное меню (или Gameplay для тестирования)
            string targetScene = skipToGameplay ? "Gameplay" : "MainMenu";
            SceneManager.LoadScene(targetScene);
        }

        private void InitializeCoreServices()
        {
            Debug.Log("[Bootstrapper] Initializing core services...");
            
            // Инициализируем GameManager
            if (GameManager.Instance == null)
            {
                GameObject gmObject = new GameObject("GameManager");
                gmObject.AddComponent<GameManager>();
            }
            
            // Инициализируем SceneLoader
            if (SceneLoader.Instance == null)
            {
                GameObject slObject = new GameObject("SceneLoader");
                slObject.AddComponent<SceneLoader>();
            }
            
            // Инициализируем SettingsManager
            if (SettingsManager.Instance == null)
            {
                GameObject smObject = new GameObject("SettingsManager");
                smObject.AddComponent<SettingsManager>();
            }
            
            // Инициализируем CursorManager
            if (CursorManager.Instance == null)
            {
                GameObject cmObject = new GameObject("CursorManager");
                cmObject.AddComponent<CursorManager>();
            }
            
            // Инициализируем AudioManager
            if (AudioManager.Instance == null)
            {
                GameObject amObject = new GameObject("AudioManager");
                amObject.AddComponent<AudioManager>();
            }
        }
    }
}