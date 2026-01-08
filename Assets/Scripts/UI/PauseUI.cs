using UnityEngine;
using UnityEngine.UI;

namespace RhythmHell.UI
{
    /// <summary>
    /// Управление UI панели паузы
    /// </summary>
    public class PausePanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Panels")]
        [SerializeField] private GameObject pauseMenuPanel; // Сама панель с кнопками

        private void OnEnable()
        {
            // Подписываемся на кнопки
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            ShowPauseMenu();
        }

        private void OnDisable()
        {
            // Отписываемся
            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void OnResumeClicked()
        {
            var gameplay = Gameplay.GameplayManager.Instance;
            if (gameplay != null)
            {
                gameplay.Resume();
            }
        }

        private void OnMainMenuClicked()
        {
            var gameplay = Gameplay.GameplayManager.Instance;
            if (gameplay != null)
            {
                gameplay.ReturnToMainMenu();
            }
        }

        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }
    }
}