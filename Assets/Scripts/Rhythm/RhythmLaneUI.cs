using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RhythmHell.UI
{
    /// <summary>
    /// Ритм-лента. Квадраты двигаются справа налево.
    /// НОВАЯ ЛОГИКА: квадрат меняет цвет только если по нему кликнули в нужный момент.
    /// Пропущенные квадраты просто проходят мимо.
    /// </summary>
    public class RhythmLaneUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform laneContainer;
        [SerializeField] private GameObject beatNotePrefab;
        [SerializeField] private RectTransform targetZone;

        [Header("Settings")]
        [SerializeField] private float noteSpeed = 200f; // Пикселей в секунду
        [SerializeField] private float spawnX = 400f; // Где спавнить справа
        [SerializeField] private float despawnX = -500f; // Где удалять слева
        [SerializeField] private bool testModeSpawn = true; // Спавнить по таймеру для теста

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.7f, 1f); // Голубой
        [SerializeField] private Color perfectColor = Color.green;
        [SerializeField] private Color goodColor = Color.yellow;
        [SerializeField] private Color missColor = Color.red;

        // Структура для хранения данных о ноте
        private class BeatNote
        {
            public RectTransform rectTransform;
            public Image image;
            public bool wasHit; // Была ли сбита
        }

        private List<BeatNote> activeNotes = new List<BeatNote>();
        private Queue<RectTransform> notePool = new Queue<RectTransform>();
        
        // Для тестового режима
        private float testSpawnTimer = 0f;
        private float testSpawnInterval = 0.625f; // Будет взято из BPM
        
        private bool isSubscribedToBeats = false; // Флаг подписки

        private void Start()
        {
            // Создаём пул нот
            for (int i = 0; i < 15; i++)
            {
                GameObject obj = Instantiate(beatNotePrefab, laneContainer);
                RectTransform rect = obj.GetComponent<RectTransform>();
                obj.SetActive(false);
                notePool.Enqueue(rect);
            }

            // Подписываемся на события BeatManager (ждём пока он загрузится)
            TrySubscribeToBeatManager();
        }

        /// <summary>
        /// Попытка подписаться на BeatManager (вызывается из Start и Update)
        /// </summary>
        private void TrySubscribeToBeatManager()
        {
            if (isSubscribedToBeats) return; // Уже подписаны

            if (Rhythm.BeatManager.Instance != null)
            {
                Rhythm.BeatManager.Instance.OnBeat += OnBeat;
                testSpawnInterval = Rhythm.BeatManager.Instance.BeatInterval;
                isSubscribedToBeats = true;
                
                Debug.Log($"[RhythmLane] ✅ Successfully subscribed to BeatManager! Interval: {testSpawnInterval}");
            }
            else
            {
                Debug.LogWarning("[RhythmLane] ⏳ BeatManager not ready yet, will retry...");
            }
        }

        private void OnEnable()
        {
            // НЕ подписываемся здесь, делаем это в Start
            // потому что BeatManager может быть ещё не готов
        }

        private void OnDisable()
        {
            // Отписываемся если были подписаны
            if (isSubscribedToBeats && Rhythm.BeatManager.Instance != null)
            {
                Rhythm.BeatManager.Instance.OnBeat -= OnBeat;
                isSubscribedToBeats = false;
            }
        }

        private void Update()
        {
            // Пытаемся подписаться если ещё не подписаны
            if (!isSubscribedToBeats)
            {
                TrySubscribeToBeatManager();
            }

            // ТЕСТОВЫЙ РЕЖИМ: спавн по таймеру если события не работают
            if (testModeSpawn && !isSubscribedToBeats) // Используем только если не подписаны
            {
                testSpawnTimer += Time.deltaTime;
                if (testSpawnTimer >= testSpawnInterval)
                {
                    testSpawnTimer = 0f;
                    SpawnNote();
                }
            }

            // Двигаем все ноты
            MoveNotes();

            // Удаляем ноты которые ушли за левый край
            CleanupNotes();

            // Анимация target zone
            AnimateTargetZone();
        }

        /// <summary>
        /// На каждый бит спавним ноту
        /// </summary>
        private void OnBeat()
        {
            Debug.Log("[RhythmLane] OnBeat triggered - spawning note");
            SpawnNote();

            // Пульсация target zone
            if (targetZone != null)
            {
                targetZone.localScale = Vector3.one * 1.3f;
            }
        }

        /// <summary>
        /// Заспавнить новую ноту
        /// </summary>
        private void SpawnNote()
        {
            RectTransform noteRect = GetNoteFromPool();
            Image noteImage = noteRect.GetComponent<Image>();

            // Создаём структуру
            BeatNote note = new BeatNote
            {
                rectTransform = noteRect,
                image = noteImage,
                wasHit = false
            };

            // Ставим на правый край
            noteRect.anchoredPosition = new Vector2(spawnX, 0);

            // Сбрасываем цвет на голубой
            if (noteImage != null)
                noteImage.color = normalColor;

            activeNotes.Add(note);
        }

        /// <summary>
        /// Двигать все активные ноты влево
        /// </summary>
        private void MoveNotes()
        {
            foreach (var note in activeNotes)
            {
                Vector2 pos = note.rectTransform.anchoredPosition;
                pos.x -= noteSpeed * Time.deltaTime;
                note.rectTransform.anchoredPosition = pos;
            }
        }

        /// <summary>
        /// Удалить ноты которые ушли за левый край
        /// </summary>
        private void CleanupNotes()
        {
            for (int i = activeNotes.Count - 1; i >= 0; i--)
            {
                if (activeNotes[i].rectTransform.anchoredPosition.x < despawnX)
                {
                    // Просто удаляем, без изменения цвета
                    ReturnNoteToPool(activeNotes[i].rectTransform);
                    activeNotes.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// НОВАЯ ЛОГИКА: Вызывается когда игрок стреляет.
        /// Находим ноту которая СЕЙЧАС в target zone и красим её.
        /// </summary>
        public void OnPlayerShoot(int timing)
        {
            if (targetZone == null) return;

            // Находим ноту которая БЛИЖЕ ВСЕГО к target zone и ещё не сбита
            BeatNote closestNote = GetClosestUnshotNoteToTarget();

            if (closestNote != null)
            {
                float targetX = targetZone.anchoredPosition.x;
                float noteX = closestNote.rectTransform.anchoredPosition.x;
                float distance = Mathf.Abs(noteX - targetX);

                // Если нота достаточно близко к target zone (в пределах разумного)
                // Это предотвратит окраску нот которые далеко
                if (distance < 100f) // 100 пикселей - разумное окно
                {
                    // Красим ноту в зависимости от точности
                    if (closestNote.image != null)
                    {
                        if (timing == 2)
                            closestNote.image.color = perfectColor; // Зелёный
                        else if (timing == 1)
                            closestNote.image.color = goodColor; // Жёлтый
                        else
                            closestNote.image.color = missColor; // Красный
                    }

                    // Помечаем что нота сбита (чтобы не красить её повторно)
                    closestNote.wasHit = true;
                }
            }
        }

        /// <summary>
        /// Найти ближайшую несбитую ноту к target zone
        /// </summary>
        private BeatNote GetClosestUnshotNoteToTarget()
        {
            if (activeNotes.Count == 0 || targetZone == null) return null;

            float targetX = targetZone.anchoredPosition.x;
            BeatNote closest = null;
            float minDistance = float.MaxValue;

            foreach (var note in activeNotes)
            {
                // Пропускаем уже сбитые ноты
                if (note.wasHit) continue;

                float distance = Mathf.Abs(note.rectTransform.anchoredPosition.x - targetX);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = note;
                }
            }

            return closest;
        }

        /// <summary>
        /// Получить ноту из пула
        /// </summary>
        private RectTransform GetNoteFromPool()
        {
            RectTransform note;

            if (notePool.Count > 0)
            {
                note = notePool.Dequeue();
                note.gameObject.SetActive(true);
            }
            else
            {
                GameObject obj = Instantiate(beatNotePrefab, laneContainer);
                note = obj.GetComponent<RectTransform>();
            }

            return note;
        }

        /// <summary>
        /// Вернуть ноту в пул
        /// </summary>
        private void ReturnNoteToPool(RectTransform note)
        {
            note.gameObject.SetActive(false);
            notePool.Enqueue(note);
        }

        /// <summary>
        /// Анимация target zone (плавное возвращение к нормальному размеру)
        /// </summary>
        private void AnimateTargetZone()
        {
            if (targetZone != null)
            {
                targetZone.localScale = Vector3.Lerp(targetZone.localScale, Vector3.one, Time.deltaTime * 10f);
            }
        }
    }
}