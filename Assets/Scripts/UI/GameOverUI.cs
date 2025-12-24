using UnityEngine;
using UnityEngine.UI;
using TMPro;
using mash2.Core;

namespace mash2.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI waveText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            DisplayStats();
            
            if (restartButton != null)
                restartButton.Select();
        }

        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void DisplayStats()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("GameManager not found! Cannot display stats.");
                return;
            }

            if (scoreText != null)
            {
                int score = GameManager.Instance.CurrentScore;
                scoreText.text = $"Score: {score}";
            }

            if (timeText != null)
            {
                float time = GameManager.Instance.GameplayTime;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timeText.text = $"Time: {minutes}:{seconds:00}";
            }

            if (waveText != null)
            {
                int wave = GameManager.Instance.CurrentWave;
                waveText.text = $"Wave: {wave}";
            }
        }

        private void OnRestartClicked()
        {
            Debug.Log("Restart button clicked - Loading Gameplay scene");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGameplay();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(3); // Gameplay
            }
        }

        private void OnMainMenuClicked()
        {
            Debug.Log("Main Menu button clicked from Game Over");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(1); // MainMenu
            }
        }
    }
}