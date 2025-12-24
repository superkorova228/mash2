using UnityEngine;
using UnityEngine.UI;
using mash2.Core;

namespace mash2.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            // Подписываемся на клики
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
            
            // БЕЗОПАСНАЯ подписка на события GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null in MainMenuUI.Start()");
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от кнопок
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (creditsButton != null)
                creditsButton.onClick.RemoveListener(OnCreditsClicked);
            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
            
            // БЕЗОПАСНАЯ отписка от событий
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        // Обработчики кликов с проверкой на null
        private void OnPlayClicked()
        {
            Debug.Log("Play button clicked!");
            
            // Проверяем, существует ли GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGameplay();
            }
            else
            {
                // Fallback - загружаем напрямую через SceneLoader
                Debug.LogWarning("GameManager not found! Using SceneLoader directly.");
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadScene(3); // Gameplay
                }
                else
                {
                    Debug.LogError("SceneLoader is also null! Cannot load scene.");
                }
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Settings button clicked!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadSettings();
            }
            else
            {
                Debug.LogWarning("GameManager not found! Using SceneLoader directly.");
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadScene(2); // Settings
                }
            }
        }

        private void OnCreditsClicked()
        {
            Debug.Log("Credits button clicked!");
            
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(6); // Credits
            }
            else
            {
                Debug.LogError("SceneLoader is null!");
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quit button clicked!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.QuitGame();
            }
            else
            {
                // Последний вариант - выход напрямую
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
        }

        // Реагируем на изменение состояния игры
        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"MainMenu noticed state change: {oldState} → {newState}");
        }
    }
}