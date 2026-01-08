using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Управляет геймплеем: инициализация, паузы, Game Over.
    /// Связывает все игровые системы.
    /// </summary>
    public class GameplayManager : MonoBehaviour
    {
        public static GameplayManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        private bool isPaused = false;

        private void Awake()
        {
            // Singleton для доступа из других скриптов
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Устанавливаем состояние игры
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ChangeGameState(Core.GameState.Playing);
            }

            // Скрываем UI панели
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            Debug.Log("[GameplayManager] Gameplay started!");
        }

        private void Update()
        {
            // Обработка паузы по Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Проверяем что не идёт выбор перков
                if (UI.PerkSelectionUI.Instance != null && UI.PerkSelectionUI.Instance.IsSelectionActive)
                {
                    Debug.Log("[GameplayManager] Cannot pause during perk selection");
                    return;
                }

                if (!isPaused)
                    Pause();
                else
                    Resume();
            }
        }

        /// <summary>
        /// Поставить игру на паузу
        /// </summary>
        public void Pause()
        {
            if (isPaused) return;

            isPaused = true;

            // Меняем состояние через GameManager
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ChangeGameState(Core.GameState.Paused);
            }

            // Показываем панель паузы
            if (pausePanel != null)
                pausePanel.SetActive(true);

            Debug.Log("[GameplayManager] Game paused");
        }

        /// <summary>
        /// Продолжить игру
        /// </summary>
        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;

            // Меняем состояние обратно
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ChangeGameState(Core.GameState.Playing);
            }

            // Скрываем панель паузы
            if (pausePanel != null)
                pausePanel.SetActive(false);

            Debug.Log("[GameplayManager] Game resumed");
        }

        /// <summary>
        /// Показать экран Game Over
        /// </summary>
        public void GameOver()
        {
            // Меняем состояние
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ChangeGameState(Core.GameState.GameOver);
            }

            // Показываем экран Game Over
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Debug.Log("[GameplayManager] Game Over!");
        }

        /// <summary>
        /// Перезапустить уровень
        /// </summary>
        public void RestartGame()
        {
            // Сбрасываем данные
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ResetGameData();
            }

            // Быстрая перезагрузка через SceneManager напрямую
            UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
        }

        /// <summary>
        /// Вернуться в главное меню
        /// </summary>
        public void ReturnToMainMenu()
        {
            // Быстрая загрузка без LoadingScreen
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}