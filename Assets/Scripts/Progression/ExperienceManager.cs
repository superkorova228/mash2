using UnityEngine;
using System;

namespace RhythmHell.Progression
{
    /// <summary>
    /// Менеджер опыта и уровней. Отслеживает XP игрока и выдаёт перки при левел-апе.
    /// </summary>
    public class ExperienceManager : MonoBehaviour
    {
        public static ExperienceManager Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentXP = 0;
        [SerializeField] private int xpToNextLevel = 100;

        [Header("Level Curve")]
        [SerializeField] private int baseXPRequired = 100; // XP для 2 уровня
        [SerializeField] private float xpScaling = 1.2f; // Множитель каждый уровень (1.2 = +20%)

        // События
        public event Action<int> OnXPChanged; // Текущий XP
        public event Action<int, int> OnLevelUp; // Новый уровень, XP к следующему
        public event Action<float> OnXPProgressChanged; // Прогресс 0.0 - 1.0

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Вычисляем XP для первого уровня
            CalculateXPForNextLevel();
            
            // Уведомляем UI о начальном состоянии
            OnXPChanged?.Invoke(currentXP);
            OnXPProgressChanged?.Invoke(0f);

            Debug.Log($"[ExperienceManager] Initialized - Level: {currentLevel}, XP to next: {xpToNextLevel}");
        }

        /// <summary>
        /// Добавить опыт
        /// </summary>
        public void AddExperience(int amount)
        {
            currentXP += amount;
            
            // Уведомляем об изменении XP
            OnXPChanged?.Invoke(currentXP);
            
            // Вычисляем прогресс (0.0 - 1.0)
            float progress = (float)currentXP / xpToNextLevel;
            OnXPProgressChanged?.Invoke(progress);

            Debug.Log($"[ExperienceManager] +{amount} XP. Total: {currentXP}/{xpToNextLevel}");

            // Проверяем левел-ап
            if (currentXP >= xpToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Повышение уровня
        /// </summary>
        private void LevelUp()
        {
            currentLevel++;
            
            // Вычитаем использованный XP (остаток переходит на следующий уровень)
            currentXP -= xpToNextLevel;

            // Вычисляем XP для следующего уровня
            CalculateXPForNextLevel();

            Debug.Log($"[ExperienceManager] LEVEL UP! Now level {currentLevel}. Next level needs: {xpToNextLevel}");

            // ЗВУК левел-апа
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySound2D(Core.SoundType.LevelUp);
            }

            // Уведомляем о левел-апе
            OnLevelUp?.Invoke(currentLevel, xpToNextLevel);

            // Показываем выбор перков
            if (UI.PerkSelectionUI.Instance != null)
            {
                UI.PerkSelectionUI.Instance.ShowPerkSelection();
            }

            // Обновляем прогресс
            float progress = (float)currentXP / xpToNextLevel;
            OnXPProgressChanged?.Invoke(progress);

            // Если XP хватает ещё на уровень - повышаем снова (рекурсия)
            if (currentXP >= xpToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Вычислить XP необходимый для следующего уровня
        /// </summary>
        private void CalculateXPForNextLevel()
        {
            // Формула: baseXP * (scaling ^ (level - 1))
            // Например: 100 * (1.2 ^ 1) = 120 для уровня 3
            xpToNextLevel = Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpScaling, currentLevel - 1));
        }

        /// <summary>
        /// Сбросить прогресс (новая игра)
        /// </summary>
        public void ResetProgress()
        {
            currentLevel = 1;
            currentXP = 0;
            CalculateXPForNextLevel();
            
            OnXPChanged?.Invoke(currentXP);
            OnXPProgressChanged?.Invoke(0f);

            Debug.Log("[ExperienceManager] Progress reset");
        }

        // Публичные свойства
        public int CurrentLevel => currentLevel;
        public int CurrentXP => currentXP;
        public int XPToNextLevel => xpToNextLevel;
        public float ProgressToNextLevel => (float)currentXP / xpToNextLevel;
    }
}