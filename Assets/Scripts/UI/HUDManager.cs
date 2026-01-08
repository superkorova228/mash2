using UnityEngine;
using TMPro;
using System;

namespace RhythmHell.UI
{
    /// <summary>
    /// Управляет HUD (здоровье, счёт, волны, таймер).
    /// Подписывается на события игры и обновляет UI в реальном времени.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Timer Settings")]
        [SerializeField] private bool useTimer = true;
        [SerializeField] private float trackDuration = 180f; // 3 минуты (заменим на реальную длину трека)

        private float currentTime = 0f;
        private bool isTimerRunning = false;

        private void OnEnable()
        {
            // Подписываемся на события GameManager
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged += UpdateScore;
                Core.GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            // Отписываемся
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged -= UpdateScore;
                Core.GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void Start()
        {
            // Инициализация начальных значений
            UpdateScore(Core.GameManager.Instance != null ? Core.GameManager.Instance.Score : 0);
            UpdateWave(Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentCircle : 1);

            // Подписываемся на HP игрока
            if (Gameplay.PlayerController.Instance != null)
            {
                Gameplay.PlayerController.Instance.OnHealthChanged += UpdateHP;
                UpdateHP(Gameplay.PlayerController.Instance.CurrentHP);
            }

            // Запускаем таймер
            if (useTimer)
            {
                currentTime = trackDuration;
                isTimerRunning = true;
            }
        }

        private void Update()
        {
            // Обновление таймера
            if (isTimerRunning && useTimer)
            {
                currentTime -= Time.deltaTime;

                if (currentTime <= 0f)
                {
                    currentTime = 0f;
                    isTimerRunning = false;
                    OnTrackEnded();
                }

                UpdateTimer(currentTime);
            }
        }

        /// <summary>
        /// Обновить отображение HP
        /// </summary>
        public void UpdateHP(int hp)
        {
            if (hpText != null)
            {
                hpText.text = $"HP: {hp}";

                // Цветовое предупреждение при низком HP
                if (hp <= 30)
                    hpText.color = Color.red;
                else if (hp <= 50)
                    hpText.color = Color.yellow;
                else
                    hpText.color = Color.white;
            }
        }

        /// <summary>
        /// Обновить счёт (вызывается через событие)
        /// </summary>
        private void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {score}";
            }
        }

        /// <summary>
        /// Обновить волну/круг
        /// </summary>
        private void UpdateWave(int circle)
        {
            if (waveText != null)
            {
                waveText.text = $"CIRCLE: {circle}/9";
            }
        }

        /// <summary>
        /// Обновить таймер
        /// </summary>
        private void UpdateTimer(float timeRemaining)
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";

                // Красный цвет когда мало времени
                if (timeRemaining <= 30f)
                    timerText.color = Color.red;
                else
                    timerText.color = Color.white;
            }
        }

        /// <summary>
        /// Обработка изменения состояния игры
        /// </summary>
        private void OnGameStateChanged(Core.GameState newState)
        {
            // Останавливаем таймер при паузе/Game Over
            isTimerRunning = (newState == Core.GameState.Playing);
        }

        /// <summary>
        /// Трек закончился - все враги должны умереть
        /// </summary>
        private void OnTrackEnded()
        {
            Debug.Log("[HUD] Track ended! Portal should spawn.");
            // Здесь позже вызовем событие для уничтожения врагов и спавна портала
        }

        /// <summary>
        /// Установить длительность трека (вызывается BeatManager)
        /// </summary>
        public void SetTrackDuration(float duration)
        {
            trackDuration = duration;
            currentTime = duration;
        }
    }
}