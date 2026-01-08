using UnityEngine;
using System;

namespace RhythmHell.Core
{
    /// <summary>
    /// Состояния игры
    /// </summary>
    public enum GameState
    {
        MainMenu,    // Главное меню
        Playing,     // Игровой процесс
        Paused,      // Пауза
        GameOver,    // Конец игры
        Loading      // Загрузка
    }

    /// <summary>
    /// Главный менеджер игры. Управляет состояниями и глобальными данными.
    /// Singleton - существует в единственном экземпляре и не уничтожается между сценами.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton паттерн
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Player Stats")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int collectedSouls = 0; // Валюта для казино
        [SerializeField] private int currentCircle = 1; // Текущий круг ада (1-9)

        // События для подписки других систем
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnSoulsChanged;

        // Публичные свойства (только для чтения)
        public GameState CurrentState => currentState;
        public int Score => currentScore;
        public int Souls => collectedSouls;
        public int CurrentCircle => currentCircle;

        private void Awake()
        {
            // Реализация Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать при загрузке новых сцен
            
            Debug.Log("[GameManager] Initialized");
        }

        /// <summary>
        /// Изменить состояние игры
        /// </summary>
        public void ChangeGameState(GameState newState)
        {
            if (currentState == newState) return;

            Debug.Log($"[GameManager] State changed: {currentState} -> {newState}");
            
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            // Обработка смены состояния
            HandleStateChange(newState);
        }

        private void HandleStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Playing:
                    Time.timeScale = 1f; // Нормальная скорость
                    Cursor.visible = false; // Скрываем курсор
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f; // Останавливаем время
                    Cursor.visible = true; // Показываем курсор
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    Cursor.visible = true;
                    break;

                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    Cursor.visible = true;
                    break;
            }
        }

        /// <summary>
        /// Добавить очки
        /// </summary>
        public void AddScore(int amount)
        {
            currentScore += amount;
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Добавить души (валюту)
        /// </summary>
        public void AddSouls(int amount)
        {
            collectedSouls += amount;
            OnSoulsChanged?.Invoke(collectedSouls);
        }

        /// <summary>
        /// Потратить души
        /// </summary>
        public bool SpendSouls(int amount)
        {
            if (collectedSouls >= amount)
            {
                collectedSouls -= amount;
                OnSoulsChanged?.Invoke(collectedSouls);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Сбросить данные для новой игры
        /// </summary>
        public void ResetGameData()
        {
            currentScore = 0;
            currentCircle = 1;
            // Души НЕ сбрасываем - они копятся между попытками для казино
            
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Перейти на следующий круг ада
        /// </summary>
        public void NextCircle()
        {
            currentCircle++;
            Debug.Log($"[GameManager] Entering circle {currentCircle} of Hell");
        }
    }
}