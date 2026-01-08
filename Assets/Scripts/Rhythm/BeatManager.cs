using UnityEngine;
using System;

namespace RhythmHell.Rhythm
{
    /// <summary>
    /// Менеджер ритма. Использует AudioSettings.dspTime для точной синхронизации.
    /// Генерирует события "бита" для геймплейных систем.
    /// </summary>
    public class BeatManager : MonoBehaviour
    {
        public static BeatManager Instance { get; private set; }

        [Header("Track Settings")]
        [SerializeField] private AudioClip track;
        [SerializeField] private float bpm = 120f; // Beats Per Minute трека
        [SerializeField] private float trackOffset = 0f; // Смещение в секундах если трек не начинается с бита

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        [Header("Beat Detection")]
        [SerializeField] private float beatInterval; // Вычисляется из BPM
        [SerializeField] private int currentBeat = 0;

        // dspTime когда началась музыка
        private double songStartDspTime;
        private double nextBeatDspTime;
        private bool isPlaying = false;

        // События для подписки других систем
        public event Action OnBeat; // Каждый бит
        public event Action<int> OnBeatWithNumber; // Бит с номером
        public event Action OnTrackEnded; // Трек закончился

        // Публичные свойства
        public float BPM => bpm;
        public float BeatInterval => beatInterval;
        public int CurrentBeat => currentBeat;
        public bool IsPlaying => isPlaying;
        public float TrackDuration => track != null ? track.length : 0f;

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
            // Вычисляем интервал бита из BPM
            // BPM = удары в минуту, значит beatInterval = 60 / BPM секунд
            CalculateBeatInterval();

            // Если AudioSource не назначен, ищем его
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            // Настраиваем AudioSource
            if (audioSource != null)
            {
                audioSource.clip = track;
                audioSource.playOnAwake = false;
            }

            // Автоматический старт (для теста)
            // Позже это будет вызываться когда игрок готов
            Invoke(nameof(StartTrack), 0.5f);
        }

        private void Update()
        {
            if (!isPlaying) return;

            // КРИТИЧЕСКИ ВАЖНО: Используем AudioSettings.dspTime, а НЕ Time.time!
            double currentDspTime = AudioSettings.dspTime;

            // Проверяем наступил ли следующий бит
            if (currentDspTime >= nextBeatDspTime)
            {
                TriggerBeat();
                
                // Планируем следующий бит
                nextBeatDspTime += beatInterval;
                currentBeat++;
            }

            // Проверяем закончился ли трек
            if (audioSource != null && !audioSource.isPlaying && currentBeat > 0)
            {
                StopTrack();
                OnTrackEnded?.Invoke();
            }
        }

        /// <summary>
        /// Запустить воспроизведение трека
        /// </summary>
        public void StartTrack()
        {
            if (audioSource == null || track == null)
            {
                Debug.LogError("[BeatManager] AudioSource or Track is missing!");
                return;
            }

            // Запоминаем DSP время старта
            songStartDspTime = AudioSettings.dspTime + 0.1; // Небольшая задержка для буферизации
            nextBeatDspTime = songStartDspTime + trackOffset;
            
            currentBeat = 0;
            isPlaying = true;

            // Запускаем трек в точное время
            audioSource.PlayScheduled(songStartDspTime);

            // Уведомляем HUD о длительности трека
            var hud = UI.HUDManager.FindObjectOfType<UI.HUDManager>();
            if (hud != null)
            {
                hud.SetTrackDuration(TrackDuration);
            }

            Debug.Log($"[BeatManager] Track started! BPM: {bpm}, Beat Interval: {beatInterval:F3}s");
        }

        /// <summary>
        /// Остановить трек
        /// </summary>
        public void StopTrack()
        {
            isPlaying = false;
            
            if (audioSource != null)
                audioSource.Stop();

            Debug.Log("[BeatManager] Track stopped");
        }

        /// <summary>
        /// Вызывается каждый бит
        /// </summary>
        private void TriggerBeat()
        {
            OnBeat?.Invoke();
            OnBeatWithNumber?.Invoke(currentBeat);

            Debug.Log($"[BeatManager] BEAT #{currentBeat}");
        }

        /// <summary>
        /// Вычислить интервал бита из BPM
        /// </summary>
        private void CalculateBeatInterval()
        {
            beatInterval = 60f / bpm;
        }

        /// <summary>
        /// Изменить BPM (если трек динамический)
        /// </summary>
        public void SetBPM(float newBpm)
        {
            bpm = newBpm;
            CalculateBeatInterval();
        }

        /// <summary>
        /// Получить время до следующего бита
        /// </summary>
        public float GetTimeToNextBeat()
        {
            if (!isPlaying) return 0f;
            return (float)(nextBeatDspTime - AudioSettings.dspTime);
        }

        /// <summary>
        /// Проверить попал ли игрок в окно тайминга
        /// perfectWindow - окно для "Perfect" (например 0.1 сек)
        /// goodWindow - окно для "Good" (например 0.2 сек)
        /// Возвращает: 2 = Perfect, 1 = Good, 0 = Miss
        /// </summary>
        public int CheckTiming(float perfectWindow = 0.1f, float goodWindow = 0.2f)
        {
            float timeToNext = GetTimeToNextBeat();
            float timeSinceLast = beatInterval - timeToNext;
            
            // Находим минимальное расстояние до ближайшего бита
            float distanceToBeat = Mathf.Min(timeToNext, timeSinceLast);

            if (distanceToBeat <= perfectWindow)
                return 2; // Perfect!
            else if (distanceToBeat <= goodWindow)
                return 1; // Good
            else
                return 0; // Miss
        }

        private void OnDestroy()
        {
            if (audioSource != null)
                audioSource.Stop();
        }
    }
}