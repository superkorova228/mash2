using UnityEngine;
using System.Collections.Generic;

namespace RhythmHell.Core
{
    /// <summary>
    /// Типы звуков
    /// </summary>
    public enum SoundType
    {
        // Weapon
        ShootPerfect,
        ShootGood,
        ShootMiss,
        
        // Combat
        EnemyHit,
        EnemyDeath,
        PlayerHit,
        PlayerDeath,
        
        // UI
        PerkSelect,
        ButtonClick,
        LevelUp,        // ДОБАВЛЕНО
        
        // Special
        BeatTick,
        Portal
    }

    /// <summary>
    /// Данные звука
    /// </summary>
    [System.Serializable]
    public class SoundData
    {
        public SoundType soundType;
        public AudioClip[] clips; // Массив для вариаций звука
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
        public bool randomPitch = false; // Случайный pitch для разнообразия
        [Range(0f, 0.3f)] public float pitchVariation = 0.1f;
    }

    /// <summary>
    /// Менеджер аудио. Управляет всеми звуками игры.
    /// Использует пул AudioSource для оптимизации.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sound Library")]
        [SerializeField] private List<SoundData> sounds = new List<SoundData>();

        [Header("Audio Sources Pool")]
        [SerializeField] private int poolSize = 10;
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeAudioSources = new List<AudioSource>();

        [Header("Volume Controls")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float musicVolume = 1f;

        // Кэш для быстрого поиска звуков
        private Dictionary<SoundType, SoundData> soundCache = new Dictionary<SoundType, SoundData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Создаём пул AudioSource
            CreateAudioSourcePool();

            // Заполняем кэш
            BuildSoundCache();

            Debug.Log($"[AudioManager] Initialized with {sounds.Count} sounds, pool size: {poolSize}");
        }

        private void Start()
        {
            // Подписываемся на настройки для управления громкостью
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged += OnSettingsChanged;
                UpdateVolumeFromSettings();
            }
        }

        /// <summary>
        /// Создать пул AudioSource
        /// </summary>
        private void CreateAudioSourcePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                audioSourcePool.Enqueue(source);
            }
        }

        /// <summary>
        /// Построить кэш звуков для быстрого доступа
        /// </summary>
        private void BuildSoundCache()
        {
            soundCache.Clear();
            foreach (var sound in sounds)
            {
                if (!soundCache.ContainsKey(sound.soundType))
                {
                    soundCache.Add(sound.soundType, sound);
                }
                else
                {
                    Debug.LogWarning($"[AudioManager] Duplicate sound type: {sound.soundType}");
                }
            }
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(SoundType type, Vector3 position = default)
        {
            if (!soundCache.TryGetValue(type, out SoundData soundData))
            {
                Debug.LogWarning($"[AudioManager] Sound not found: {type}");
                return;
            }

            if (soundData.clips == null || soundData.clips.Length == 0)
            {
                Debug.LogWarning($"[AudioManager] No clips for sound: {type}");
                return;
            }

            // Выбираем случайный клип из массива
            AudioClip clip = soundData.clips[Random.Range(0, soundData.clips.Length)];

            // Получаем AudioSource из пула
            AudioSource source = GetAudioSourceFromPool();
            if (source == null)
            {
                Debug.LogWarning("[AudioManager] Audio source pool exhausted!");
                return;
            }

            // Настраиваем AudioSource
            source.clip = clip;
            source.volume = soundData.volume * sfxVolume * masterVolume;
            
            // Pitch с вариацией если включено
            if (soundData.randomPitch)
            {
                source.pitch = soundData.pitch + Random.Range(-soundData.pitchVariation, soundData.pitchVariation);
            }
            else
            {
                source.pitch = soundData.pitch;
            }

            // Воспроизводим
            source.Play();

            // Возвращаем в пул после завершения
            StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));
        }

        /// <summary>
        /// Проиграть звук в 2D (без позиции)
        /// </summary>
        public void PlaySound2D(SoundType type)
        {
            PlaySound(type, Vector3.zero);
        }

        /// <summary>
        /// Получить AudioSource из пула
        /// </summary>
        private AudioSource GetAudioSourceFromPool()
        {
            if (audioSourcePool.Count > 0)
            {
                AudioSource source = audioSourcePool.Dequeue();
                activeAudioSources.Add(source);
                return source;
            }

            // Если пул пуст - создаём новый
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            activeAudioSources.Add(newSource);
            Debug.LogWarning("[AudioManager] Pool exhausted, creating new AudioSource");
            return newSource;
        }

        /// <summary>
        /// Вернуть AudioSource в пул
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (source != null)
            {
                source.Stop();
                source.clip = null;
                activeAudioSources.Remove(source);
                audioSourcePool.Enqueue(source);
            }
        }

        /// <summary>
        /// Остановить все звуки
        /// </summary>
        public void StopAllSounds()
        {
            foreach (var source in activeAudioSources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }

            // Возвращаем все в пул
            foreach (var source in activeAudioSources)
            {
                if (source != null)
                {
                    audioSourcePool.Enqueue(source);
                }
            }

            activeAudioSources.Clear();
        }

        /// <summary>
        /// Обновить громкость из настроек
        /// </summary>
        private void UpdateVolumeFromSettings()
        {
            if (SettingsManager.Instance != null)
            {
                masterVolume = SettingsManager.Instance.CurrentSettings.masterVolume;
                sfxVolume = SettingsManager.Instance.CurrentSettings.sfxVolume;
                musicVolume = SettingsManager.Instance.CurrentSettings.musicVolume;

                Debug.Log($"[AudioManager] Volume updated - Master: {masterVolume}, SFX: {sfxVolume}, Music: {musicVolume}");
            }
        }

        /// <summary>
        /// Вызывается при изменении настроек
        /// </summary>
        private void OnSettingsChanged(GameSettings settings)
        {
            UpdateVolumeFromSettings();
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged -= OnSettingsChanged;
            }
        }

        /// <summary>
        /// Добавить звук в библиотеку во время выполнения (опционально)
        /// </summary>
        public void AddSound(SoundData soundData)
        {
            sounds.Add(soundData);
            if (!soundCache.ContainsKey(soundData.soundType))
            {
                soundCache.Add(soundData.soundType, soundData);
            }
        }
    }
}