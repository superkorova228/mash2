using UnityEngine;
using UnityEngine.UI;
using TMPro;
using mash2.Core;

namespace mash2.UI
{
    /// <summary>
    /// Управляет HUD во время игры
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        [Header("Top HUD")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Bottom HUD")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI rhythmText;

        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;
        
        private float currentHealth;

        private void Start()
        {
            currentHealth = maxHealth;
            UpdateHUD();
        }

        private void Update()
        {
            UpdateHUD();
        }

        /// <summary>
        /// Обновляет все элементы HUD
        /// </summary>
        private void UpdateHUD()
        {
            if (GameManager.Instance == null)
                return;

            // Score
            if (scoreText != null)
            {
                scoreText.text = $"Score: {GameManager.Instance.CurrentScore}";
            }

            // Time
            if (timeText != null)
            {
                float time = GameManager.Instance.GameplayTime;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                timeText.text = $"Time: {minutes}:{seconds:00}";
            }

            // Health (пока статичное, позже подключим к игроку)
            UpdateHealthDisplay();
        }

        /// <summary>
        /// Обновляет отображение здоровья
        /// </summary>
        private void UpdateHealthDisplay()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth;
                healthBar.maxValue = maxHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"HP: {Mathf.RoundToInt(currentHealth)}";
            }
        }

        /// <summary>
        /// Устанавливает здоровье (вызывается из игровой логики)
        /// </summary>
        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0f, maxHealth);
            UpdateHealthDisplay();
            
            // Если здоровье закончилось
            if (currentHealth <= 0f && GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }

        /// <summary>
        /// Получает урон
        /// </summary>
        public void TakeDamage(float damage)
        {
            SetHealth(currentHealth - damage);
        }

        /// <summary>
        /// Лечится
        /// </summary>
        public void Heal(float amount)
        {
            SetHealth(currentHealth + amount);
        }

        /// <summary>
        /// Обновляет индикатор ритма (placeholder)
        /// </summary>
        public void UpdateRhythmIndicator(string status)
        {
            if (rhythmText != null)
            {
                rhythmText.text = $"♪ Rhythm: {status} ♪";
                
                // Можно добавить цветовую индикацию
                switch (status.ToLower())
                {
                    case "perfect":
                        rhythmText.color = Color.green;
                        break;
                    case "good":
                        rhythmText.color = Color.yellow;
                        break;
                    case "miss":
                        rhythmText.color = Color.red;
                        break;
                    default:
                        rhythmText.color = Color.white;
                        break;
                }
            }
        }
    }
}