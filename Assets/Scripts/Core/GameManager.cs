using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace mash2.Core
{
    /// <summary>
    /// Главный менеджер игры. Управляет состояниями, паузой, игровыми данными.
    /// Singleton - существует во всех сценах.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.Boot;
        
        // Публичное свойство для доступа к текущему состоянию (только чтение)
        public GameState CurrentState => currentState;
        
        // События для подписки других систем
        public event Action<GameState, GameState> OnStateChanged; // (предыдущее, новое)
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        
        [Header("Gameplay Data")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int currentWave = 0;
        [SerializeField] private float gameplayTime = 0f;
        
        // Публичные свойства для доступа к данным
        public int CurrentScore => currentScore;
        public int CurrentWave => currentWave;
        public float GameplayTime => gameplayTime;
        
        // Флаг паузы
        private bool isPaused = false;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("GameManager initialized.");
        }

        private void Start()
        {
            // Подписываемся на события смены сцены
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            // Обновляем время геймплея только во время игры
            if (currentState == GameState.Gameplay && !isPaused)
            {
                gameplayTime += Time.deltaTime;
            }
            
            // Горячая клавиша для паузы (ESC)
            if (currentState == GameState.Gameplay && Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    ResumeGame();
                else
                    PauseGame();
            }
        }

        /// <summary>
        /// Изменяет состояние игры
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (currentState == newState)
                return; // Уже в этом состоянии
            
            GameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"Game State changed: {previousState} → {newState}");
            
            // Уведомляем подписчиков
            OnStateChanged?.Invoke(previousState, newState);
            
            // Выполняем действия в зависимости от нового состояния
            HandleStateChange(newState);
        }

        /// <summary>
        /// Обработка входа в новое состояние
        /// </summary>
        private void HandleStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.Boot:
                    // Инициализация
                    break;
                
                case GameState.MainMenu:
                    // Сброс данных геймплея
                    ResetGameplayData();
                    Time.timeScale = 1f; // Убедимся, что время идёт нормально
                    isPaused = false;
                    break;
                
                case GameState.Gameplay:
                    // Начало новой игры
                    Time.timeScale = 1f;
                    isPaused = false;
                    break;
                
                case GameState.Paused:
                    // Пауза обрабатывается через PauseGame()
                    break;
                
                case GameState.GameOver:
                    Time.timeScale = 1f; // На экране Game Over время идёт нормально
                    isPaused = false;
                    // Здесь можно сохранить рекорды, показать статистику
                    break;
                
                case GameState.Settings:
                case GameState.Credits:
                    // Ничего особенного
                    break;
                
                case GameState.Loading:
                    // Идёт загрузка
                    break;
            }
        }

        /// <summary>
        /// Вызывается когда загружается новая сцена
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Scene loaded: {scene.name}");
            
            // Автоматически меняем состояние в зависимости от сцены
            switch (scene.name)
            {
                case "Boot":
                    ChangeState(GameState.Boot);
                    break;
                case "MainMenu":
                    ChangeState(GameState.MainMenu);
                    break;
                case "Settings":
                    ChangeState(GameState.Settings);
                    break;
                case "Gameplay":
                    ChangeState(GameState.Gameplay);
                    break;
                case "Pause":
                    ChangeState(GameState.Paused);
                    break;
                case "GameOver":
                    ChangeState(GameState.GameOver);
                    break;
                case "Credits":
                    ChangeState(GameState.Credits);
                    break;
            }
        }

        // ============================================
        // ПАУЗА
        // ============================================
        
        /// <summary>
        /// Ставит игру на паузу
        /// </summary>
        public void PauseGame()
        {
            if (currentState != GameState.Gameplay)
            {
                Debug.LogWarning("Cannot pause - not in gameplay!");
                return;
            }
            
            if (isPaused)
                return; // Уже на паузе
            
            isPaused = true;
            Time.timeScale = 0f; // Останавливаем время
            
            Debug.Log("Game PAUSED");
            OnGamePaused?.Invoke();
            
            // Можно загрузить Pause сцену как overlay (дополнительная сцена поверх текущей)
            // Или показать UI паузы
        }

        /// <summary>
        /// Снимает игру с паузы
        /// </summary>
        public void ResumeGame()
        {
            if (!isPaused)
                return;
            
            isPaused = false;
            Time.timeScale = 1f; // Возобновляем время
            
            Debug.Log("Game RESUMED");
            OnGameResumed?.Invoke();
        }

        // ============================================
        // GAMEPLAY DATA
        // ============================================
        
        /// <summary>
        /// Добавляет очки к счёту
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            Debug.Log($"Score: {currentScore} (+{points})");
        }

        /// <summary>
        /// Переход на следующую волну врагов
        /// </summary>
        public void NextWave()
        {
            currentWave++;
            Debug.Log($"Wave {currentWave} started!");
        }

        /// <summary>
        /// Сброс данных геймплея (при возврате в меню или начале новой игры)
        /// </summary>
        public void ResetGameplayData()
        {
            currentScore = 0;
            currentWave = 0;
            gameplayTime = 0f;
            Debug.Log("Gameplay data reset.");
        }

        /// <summary>
        /// Вызывается когда игрок проигрывает
        /// </summary>
        public void TriggerGameOver()
        {
            if (currentState != GameState.Gameplay)
                return;
            
            Debug.Log("GAME OVER!");
            
            // Сохраняем рекорд (реализуем позже)
            // SettingsManager.Instance.SaveHighScore(currentScore);
            
            // Загружаем сцену GameOver
            SceneLoader.Instance.LoadScene(5); // GameOver = индекс 5
        }

        // ============================================
        // NAVIGATION (для удобства)
        // ============================================
        
        public void LoadMainMenu()
        {
            ResetGameplayData();
            SceneLoader.Instance.LoadScene(1); // MainMenu
        }

        public void LoadGameplay()
        {
            ResetGameplayData();
            SceneLoader.Instance.LoadScene(3); // Gameplay
        }

        public void LoadSettings()
        {
            SceneLoader.Instance.LoadScene(2); // Settings
        }

        public void RestartGameplay()
        {
            ResetGameplayData();
            SceneLoader.Instance.LoadScene(3);
        }

        public void QuitGame()
        {
            SceneLoader.Instance.QuitGame();
        }
    }
}