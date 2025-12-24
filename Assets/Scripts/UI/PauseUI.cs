using UnityEngine;
using UnityEngine.UI;
using mash2.Core;

namespace mash2.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused += OnGamePaused;
                GameManager.Instance.OnGameResumed += OnGameResumed;
            }
            
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGamePaused -= OnGamePaused;
                GameManager.Instance.OnGameResumed -= OnGameResumed;
            }
            
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void OnGamePaused()
        {
            Debug.Log("PauseUI: Game paused, showing pause menu");
            ShowPauseMenu();
        }

        private void OnGameResumed()
        {
            Debug.Log("PauseUI: Game resumed, hiding pause menu");
            HidePauseMenu();
        }

        private void ShowPauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                
                if (resumeButton != null)
                    resumeButton.Select();
            }
        }

        private void HidePauseMenu()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnResumeClicked()
        {
            Debug.Log("Resume button clicked");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Settings button clicked from pause menu");
            
            if (GameManager.Instance != null)
            {
                // ВАЖНО: Снимаем паузу перед загрузкой сцены
                GameManager.Instance.ResumeGame();
                GameManager.Instance.LoadSettings();
            }
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("Main Menu button clicked from pause menu");
            
            if (GameManager.Instance != null)
            {
                // ВАЖНО: Снимаем паузу перед загрузкой сцены
                GameManager.Instance.ResumeGame();
                GameManager.Instance.LoadMainMenu();
            }
        }

        public void Show()
        {
            ShowPauseMenu();
        }

        public void Hide()
        {
            HidePauseMenu();
        }
    }
}