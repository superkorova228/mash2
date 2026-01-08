using UnityEngine;
using System.Collections.Generic;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Спавнер врагов. Спавнит волны врагов с увеличением сложности к концу трека.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject chaserPrefab;
        [SerializeField] private GameObject shooterPrefab;
        [SerializeField] private GameObject tankPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnInterval = 3f; // Как часто спавнить
        [SerializeField] private float spawnDistance = 12f; // Дистанция от камеры
        [SerializeField] private int maxEnemiesAlive = 20; // Максимум врагов одновременно

        [Header("Difficulty Curve")]
        [SerializeField] private AnimationCurve difficultyCurve; // Кривая сложности от 0 до 1
        [SerializeField] private int startEnemiesPerWave = 1;
        [SerializeField] private int maxEnemiesPerWave = 5;

        [Header("Enemy Type Weights (%)")]
        [SerializeField] private float chaserWeight = 60f; // 60% Chaser
        [SerializeField] private float shooterWeight = 30f; // 30% Shooter
        [SerializeField] private float tankWeight = 10f; // 10% Tank

        private List<EnemyBase> activeEnemies = new List<EnemyBase>();
        private float nextSpawnTime = 0f;
        private float trackStartTime;
        private float trackDuration;
        private Transform playerTransform;
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            mainCamera = Camera.main;
        }

        private void Start()
        {
            // Получаем длительность трека из BeatManager
            if (Rhythm.BeatManager.Instance != null)
            {
                trackDuration = Rhythm.BeatManager.Instance.TrackDuration;
                trackStartTime = Time.time;
            }

            // Находим игрока
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }

            // Создаём кривую сложности по умолчанию если не задана
            if (difficultyCurve == null || difficultyCurve.length == 0)
            {
                difficultyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }

            Debug.Log($"[EnemySpawner] Initialized. Track duration: {trackDuration}s");
        }

        private void Update()
        {
            // Проверяем что игра активна
            if (Core.GameManager.Instance != null && 
                Core.GameManager.Instance.CurrentState != Core.GameState.Playing)
            {
                return;
            }

            // Спавним врагов по таймеру
            if (Time.time >= nextSpawnTime)
            {
                SpawnWave();
                nextSpawnTime = Time.time + spawnInterval;
            }

            // Очищаем список от мёртвых врагов
            activeEnemies.RemoveAll(e => e == null);
        }

        /// <summary>
        /// Заспавнить волну врагов
        /// </summary>
        private void SpawnWave()
        {
            // Не спавним если лимит достигнут
            if (activeEnemies.Count >= maxEnemiesAlive)
            {
                return;
            }

            // Вычисляем прогресс трека (0.0 - 1.0)
            float trackProgress = GetTrackProgress();

            // Получаем сложность из кривой
            float difficulty = difficultyCurve.Evaluate(trackProgress);

            // Количество врагов в волне зависит от сложности
            int enemiesInWave = Mathf.RoundToInt(Mathf.Lerp(startEnemiesPerWave, maxEnemiesPerWave, difficulty));

            // Спавним
            for (int i = 0; i < enemiesInWave; i++)
            {
                if (activeEnemies.Count >= maxEnemiesAlive) break;

                SpawnRandomEnemy(difficulty);
            }

            Debug.Log($"[EnemySpawner] Wave spawned: {enemiesInWave} enemies. Progress: {trackProgress:F2}, Difficulty: {difficulty:F2}");
        }

        /// <summary>
        /// Заспавнить случайного врага
        /// </summary>
        private void SpawnRandomEnemy(float difficulty)
        {
            // Выбираем тип врага на основе весов
            EnemyType type = GetRandomEnemyType(difficulty);

            // Получаем префаб
            GameObject prefab = GetPrefabForType(type);
            if (prefab == null)
            {
                Debug.LogWarning($"[EnemySpawner] No prefab for {type}!");
                return;
            }

            // Вычисляем позицию спавна (вокруг камеры)
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // Спавним
            GameObject enemyObj = Instantiate(prefab, spawnPosition, Quaternion.identity);
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                activeEnemies.Add(enemy);
            }
        }

        /// <summary>
        /// Получить случайный тип врага на основе весов
        /// </summary>
        private EnemyType GetRandomEnemyType(float difficulty)
        {
            // Нормализуем веса
            float totalWeight = chaserWeight + shooterWeight + tankWeight;
            float chaserNorm = chaserWeight / totalWeight;
            float shooterNorm = shooterWeight / totalWeight;
            float tankNorm = tankWeight / totalWeight;

            // Увеличиваем шанс сложных врагов с ростом difficulty
            // На высокой сложности больше Shooter и Tank
            float adjustedShooterWeight = shooterNorm * (1f + difficulty * 0.5f);
            float adjustedTankWeight = tankNorm * (1f + difficulty);

            float total = chaserNorm + adjustedShooterWeight + adjustedTankWeight;
            float random = Random.Range(0f, total);

            if (random < chaserNorm)
                return EnemyType.Chaser;
            else if (random < chaserNorm + adjustedShooterWeight)
                return EnemyType.Shooter;
            else
                return EnemyType.Tank;
        }

        /// <summary>
        /// Получить префаб по типу
        /// </summary>
        private GameObject GetPrefabForType(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Chaser: return chaserPrefab;
                case EnemyType.Shooter: return shooterPrefab;
                case EnemyType.Tank: return tankPrefab;
                default: return null;
            }
        }

        /// <summary>
        /// Получить случайную позицию спавна вокруг камеры
        /// </summary>
        private Vector2 GetRandomSpawnPosition()
        {
            if (mainCamera == null) return Vector2.zero;

            // Случайный угол
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // Позиция вокруг камеры на расстоянии spawnDistance
            Vector2 cameraPos = mainCamera.transform.position;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistance;

            return cameraPos + offset;
        }

        /// <summary>
        /// Получить прогресс трека (0.0 - 1.0)
        /// </summary>
        private float GetTrackProgress()
        {
            if (trackDuration <= 0) return 0f;

            float elapsed = Time.time - trackStartTime;
            return Mathf.Clamp01(elapsed / trackDuration);
        }

        /// <summary>
        /// Вызывается когда враг умирает
        /// </summary>
        public void OnEnemyDied(EnemyBase enemy)
        {
            activeEnemies.Remove(enemy);
        }

        /// <summary>
        /// Очистить всех врагов (конец трека)
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            activeEnemies.Clear();
            Debug.Log("[EnemySpawner] All enemies cleared!");
        }

        // Для дебага в редакторе
        private void OnDrawGizmosSelected()
        {
            if (mainCamera != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(mainCamera.transform.position, spawnDistance);
            }
        }
    }
}