using System.Collections;
using UnityEngine;
using mash2.Core;

namespace mash2.Audio
{
    /// <summary>
    /// Управляет всей аудио системой: музыка, SFX, синхронизация с ритмом
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip[] musicTracks;
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip enemyDeathSound;
        
        [Header("Settings")]
        [SerializeField] private float musicFadeDuration = 1f;
        
        // Текущий трек
        private int currentTrackIndex = 0;
        public AudioClip CurrentTrack => musicSource.clip;
        public bool IsPlaying => musicSource.isPlaying;
        
        // Ритм-данные (BPM)
        [Header("Rhythm Data")]
        [SerializeField] private float bpm = 120f; // Beats Per Minute
        public float BPM => bpm;
        public float BeatInterval => 60f / bpm; // Секунд между битами
        
        // Время следующего бита
        private float nextBeatTime = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Создаём AudioSource если нет
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            Debug.Log("AudioManager initialized");
        }

        private void Start()
        {
            // Применяем настройки громкости
            if (SettingsManager.Instance != null)
            {
                ApplyVolumeSettings();
                SettingsManager.Instance.OnSettingsChanged += OnSettingsChanged;
            }
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSettingsChanged -= OnSettingsChanged;
            }
        }

        private void Update()
        {
            // Обновляем время следующего бита
            if (musicSource.isPlaying)
            {
                float currentTime = musicSource.time;
                
                // Если прошёл бит
                if (currentTime >= nextBeatTime)
                {
                    nextBeatTime += BeatInterval;
                    // Здесь можно вызвать событие OnBeat
                }
            }
        }

        // ============================================
        // МУЗЫКА
        // ============================================

        public void PlayMusic(int trackIndex)
        {
            if (trackIndex < 0 || trackIndex >= musicTracks.Length)
            {
                Debug.LogWarning($"Track index {trackIndex} out of range!");
                return;
            }
            
            currentTrackIndex = trackIndex;
            musicSource.clip = musicTracks[trackIndex];
            musicSource.Play();
            
            nextBeatTime = BeatInterval; // Первый бит
            
            Debug.Log($"Playing track {trackIndex}: {musicTracks[trackIndex].name}");
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("Audio clip is null!");
                return;
            }
            
            musicSource.clip = clip;
            musicSource.Play();
            nextBeatTime = BeatInterval;
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }

        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        /// <summary>
        /// Плавное затухание музыки
        /// </summary>
        public void FadeOutMusic(float duration = -1f)
        {
            if (duration < 0)
                duration = musicFadeDuration;
            
            StartCoroutine(FadeOutCoroutine(duration));
        }

        private IEnumerator FadeOutCoroutine(float duration)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }
            
            musicSource.volume = 0f;
            musicSource.Stop();
            musicSource.volume = startVolume; // Восстанавливаем для следующего раза
        }

        // ============================================
        // ЗВУКОВЫЕ ЭФФЕКТЫ
        // ============================================

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
                return;
            
            sfxSource.PlayOneShot(clip);
        }

        public void PlayShootSound()
        {
            PlaySFX(shootSound);
        }

        public void PlayHitSound()
        {
            PlaySFX(hitSound);
        }

        public void PlayEnemyDeathSound()
        {
            PlaySFX(enemyDeathSound);
        }

        // ============================================
        // ГРОМКОСТЬ
        // ============================================

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }

        private void ApplyVolumeSettings()
        {
            if (SettingsManager.Instance == null)
                return;
            
            var settings = SettingsManager.Instance.CurrentSettings;
            SetMasterVolume(settings.masterVolume);
            SetMusicVolume(settings.musicVolume);
            SetSFXVolume(settings.sfxVolume);
        }

        private void OnSettingsChanged(mash2.Data.SettingsData settings)
        {
            ApplyVolumeSettings();
        }

        // ============================================
        // РИТМ СИСТЕМА
        // ============================================

        /// <summary>
        /// Проверяет, попал ли выстрел в ритм
        /// </summary>
        public bool IsOnBeat(float tolerance = 0.1f)
        {
            if (!musicSource.isPlaying)
                return true; // Если музыка не играет, всегда попадание
            
            float currentTime = musicSource.time;
            float timeSinceLastBeat = (currentTime % BeatInterval) / BeatInterval;
            
            // Проверяем близость к биту (начало или конец интервала)
            return timeSinceLastBeat <= tolerance || timeSinceLastBeat >= (1f - tolerance);
        }

        /// <summary>
        /// Возвращает точность попадания в ритм (0 = идеально, 1 = мимо)
        /// </summary>
        public float GetBeatAccuracy()
        {
            if (!musicSource.isPlaying)
                return 0f;
            
            float currentTime = musicSource.time;
            float timeSinceLastBeat = (currentTime % BeatInterval) / BeatInterval;
            
            // Расстояние до ближайшего бита
            float distanceToBeat = Mathf.Min(timeSinceLastBeat, 1f - timeSinceLastBeat);
            return distanceToBeat * 2f; // Нормализуем от 0 до 1
        }

        /// <summary>
        /// Возвращает текст оценки ритма
        /// </summary>
        public string GetRhythmRating()
        {
            float accuracy = GetBeatAccuracy();
            
            if (accuracy <= 0.1f) return "Perfect!";
            if (accuracy <= 0.3f) return "Good";
            if (accuracy <= 0.5f) return "OK";
            return "Miss";
        }
    }
}