using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace RhythmHell.UI
{
    /// <summary>
    /// UI выбора перков. Показывает 3 случайных перка для выбора.
    /// </summary>
    public class PerkSelectionUI : MonoBehaviour
    {
        public static PerkSelectionUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel; // Панель с перками
        [SerializeField] private PerkCardUI[] perkCards = new PerkCardUI[3]; // 3 карточки перков
        [SerializeField] private TextMeshProUGUI timerText; // Текст таймера (опционально)

        [Header("Perk Pool")]
        [SerializeField] private List<Progression.PerkData> allPerks = new List<Progression.PerkData>();

        [Header("Settings")]
        [SerializeField] private bool slowTimeOnSelection = true; // Замедлять время при выборе
        [SerializeField] private bool autoCloseEnabled = true; // Автозакрытие
        [SerializeField] private float autoCloseDelay = 3f; // Через сколько секунд закрывать

        private bool isSelectionActive = false;
        private float selectionStartTime; // Время когда открылся выбор

        // Публичное свойство для проверки
        public bool IsSelectionActive => isSelectionActive;

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
            // Скрываем панель при старте
            HideSelection();

            // Проверяем что перки назначены
            if (allPerks.Count == 0)
            {
                Debug.LogWarning("[PerkSelection] No perks in pool! Add PerkData assets to the list.");
            }
        }

        private void Update()
        {
            // Закрытие выбора по Escape (опционально)
            if (isSelectionActive && Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[PerkSelection] Selection cancelled by Escape");
                HideSelection(); // Закрывает без выбора перка
            }

            // Автозакрытие через заданное время
            if (isSelectionActive && autoCloseEnabled)
            {
                float elapsed = GetElapsedTime();
                float remaining = autoCloseDelay - elapsed;

                // Обновляем текст таймера
                if (timerText != null)
                {
                    timerText.text = Mathf.Ceil(remaining).ToString();
                    
                    // Красный цвет когда мало времени
                    if (remaining <= 1f)
                        timerText.color = Color.red;
                    else
                        timerText.color = Color.white;
                }
                
                if (elapsed >= autoCloseDelay)
                {
                    Debug.Log("[PerkSelection] Auto-closing (time expired)");
                    HideSelection(); // Закрывает без выбора
                }
            }
        }

        /// <summary>
        /// Получить время прошедшее с открытия (с учётом timeScale)
        /// </summary>
        private float GetElapsedTime()
        {
            // Используем unscaledTime чтобы таймер работал даже при замедлении
            return Time.unscaledTime - selectionStartTime;
        }

        /// <summary>
        /// Показать выбор перков
        /// </summary>
        public void ShowPerkSelection()
        {
            Debug.Log("[PerkSelection] ShowPerkSelection called!");

            if (isSelectionActive)
            {
                Debug.LogWarning("[PerkSelection] Already active!");
                return;
            }

            if (allPerks.Count < 3)
            {
                Debug.LogWarning($"[PerkSelection] Not enough perks! Count: {allPerks.Count}");
                return;
            }

            Debug.Log($"[PerkSelection] Perks in pool: {allPerks.Count}");

            isSelectionActive = true;
            selectionStartTime = Time.unscaledTime; // Запоминаем время открытия

            // Замедляем время (или оставляем нормальным)
            if (slowTimeOnSelection)
            {
                Time.timeScale = 0.5f; // 50% скорости (было 0.2f = 20%)
                Debug.Log("[PerkSelection] Time slowed to 0.5");

                // ОПЦИОНАЛЬНО: Замедляем музыку
                if (Rhythm.BeatManager.Instance != null)
                {
                    AudioSource audioSource = Rhythm.BeatManager.Instance.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.pitch = 0.5f; // Замедляем музыку
                    }
                }
            }

            // Выбираем 3 случайных перка
            List<Progression.PerkData> selectedPerks = GetRandomPerks(3);
            Debug.Log($"[PerkSelection] Selected {selectedPerks.Count} perks");

            // Показываем карточки
            for (int i = 0; i < perkCards.Length && i < selectedPerks.Count; i++)
            {
                if (perkCards[i] != null)
                {
                    perkCards[i].Setup(selectedPerks[i], this);
                    Debug.Log($"[PerkSelection] Setup card {i}: {selectedPerks[i].perkName}");
                }
                else
                {
                    Debug.LogError($"[PerkSelection] PerkCard {i} is NULL!");
                }
            }

            // Показываем панель
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
                Debug.Log("[PerkSelection] Panel shown");
            }
            else
            {
                Debug.LogError("[PerkSelection] Selection Panel is NULL!");
            }
        }

        /// <summary>
        /// Скрыть выбор перков
        /// </summary>
        public void HideSelection()
        {
            isSelectionActive = false;

            // Возвращаем нормальную скорость
            if (slowTimeOnSelection)
            {
                Time.timeScale = 1f;

                // Возвращаем pitch музыки
                if (Rhythm.BeatManager.Instance != null)
                {
                    AudioSource audioSource = Rhythm.BeatManager.Instance.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        audioSource.pitch = 1f;
                    }
                }
            }

            // Скрываем панель
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Выбран перк (вызывается из PerkCardUI)
        /// </summary>
        public void OnPerkSelected(Progression.PerkData perk)
        {
            Debug.Log($"[PerkSelection] Selected: {perk.perkName}");

            // Применяем перк к игроку
            if (Progression.PlayerStats.Instance != null)
            {
                Progression.PlayerStats.Instance.ApplyPerk(perk);
            }

            // Скрываем выбор
            HideSelection();
        }

        /// <summary>
        /// Получить N случайных перков из пула
        /// </summary>
        private List<Progression.PerkData> GetRandomPerks(int count)
        {
            // Фильтруем перки которые нельзя брать повторно
            List<Progression.PerkData> availablePerks = allPerks.Where(p => 
                p.isStackable || !Progression.PlayerStats.Instance.HasPerk(p)
            ).ToList();

            if (availablePerks.Count < count)
            {
                Debug.LogWarning($"[PerkSelection] Not enough available perks! Using all available.");
                count = availablePerks.Count;
            }

            // Выбираем случайные с учётом весов
            List<Progression.PerkData> selected = new List<Progression.PerkData>();
            
            for (int i = 0; i < count; i++)
            {
                Progression.PerkData randomPerk = GetWeightedRandomPerk(availablePerks);
                selected.Add(randomPerk);
                availablePerks.Remove(randomPerk); // Убираем чтобы не выбрать дважды
            }

            return selected;
        }

        /// <summary>
        /// Получить случайный перк с учётом весов
        /// </summary>
        private Progression.PerkData GetWeightedRandomPerk(List<Progression.PerkData> perks)
        {
            // Считаем общий вес
            float totalWeight = perks.Sum(p => p.rarityWeight);
            float random = Random.Range(0f, totalWeight);

            // Выбираем по весу
            float currentWeight = 0f;
            foreach (var perk in perks)
            {
                currentWeight += perk.rarityWeight;
                if (random <= currentWeight)
                {
                    return perk;
                }
            }

            // Fallback
            return perks[Random.Range(0, perks.Count)];
        }

        /// <summary>
        /// Добавить перк в пул (опционально, для динамического добавления)
        /// </summary>
        public void AddPerkToPool(Progression.PerkData perk)
        {
            if (perk != null && !allPerks.Contains(perk))
            {
                allPerks.Add(perk);
            }
        }
    }
}