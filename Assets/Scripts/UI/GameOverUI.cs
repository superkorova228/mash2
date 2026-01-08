using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RhythmHell.UI
{
    /// <summary>
    /// Управление UI экрана Game Over
    /// </summary>
    public class GameOverPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void OnEnable()
        {
            // Обновляем финальный счёт
            UpdateFinalScore();

            // Подписываемся на кнопки
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDisable()
        {
            // Отписываемся
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void UpdateFinalScore()
        {
            if (finalScoreText != null && Core.GameManager.Instance != null)
            {
                int score = Core.GameManager.Instance.Score;
                finalScoreText.text = $"FINAL SCORE: {score}";
            }
        }

        private void OnRestartClicked()
        {
            var gameplay = Gameplay.GameplayManager.Instance;
            if (gameplay != null)
            {
                gameplay.RestartGame();
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
    }
}