using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Типы врагов
    /// </summary>
    public enum EnemyType
    {
        Chaser,  // Стандартный преследователь
        Shooter, // Стреляет на расстоянии
        Tank     // Медленный танк с большим HP
    }

    /// <summary>
    /// Базовый класс для всех врагов.
    /// Содержит общую логику: HP, урон, движение к игроку.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Type")]
        [SerializeField] protected EnemyType enemyType;

        [Header("Stats")]
        [SerializeField] protected int maxHP = 50;
        [SerializeField] protected int currentHP;
        [SerializeField] protected int contactDamage = 10;
        [SerializeField] protected float damageInterval = 1f;

        [Header("Movement")]
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected Rigidbody2D rb;

        [Header("Rewards")]
        [SerializeField] protected int scoreReward = 10;
        [SerializeField] protected int soulReward = 1;
        [SerializeField] protected int xpReward = 10; // XP за убийство
        [SerializeField] protected GameObject xpOrbPrefab; // Префаб орба XP

        protected Transform playerTransform;
        protected float lastDamageTime = 0f;
        protected SpriteRenderer spriteRenderer;
        protected Color originalColor;

        // Публичные свойства
        public EnemyType Type => enemyType;
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;

        protected virtual void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        protected virtual void Start()
        {
            currentHP = maxHP;

            // Находим игрока
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }
        }

        protected virtual void FixedUpdate()
        {
            // Базовое поведение - движение к игроку
            // Переопределяется в наследниках для разного поведения
            MoveTowardsPlayer();
        }

        /// <summary>
        /// Базовое движение к игроку
        /// </summary>
        protected virtual void MoveTowardsPlayer()
        {
            if (playerTransform != null && rb != null)
            {
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            currentHP -= damage;

            Debug.Log($"[{enemyType}] Took {damage} damage. HP: {currentHP}/{maxHP}");

            // ЗВУК попадания
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySound2D(Core.SoundType.EnemyHit);
            }

            // Визуальный фидбэк
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashWhite());
            }

            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Смерть врага
        /// </summary>
        protected virtual void Die()
        {
            Debug.Log($"[{enemyType}] DIED!");

            // ЗВУК смерти
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySound2D(Core.SoundType.EnemyDeath);
            }

            // Награды
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.AddScore(scoreReward);
                Core.GameManager.Instance.AddSouls(soulReward);
            }

            // ДРОП XP ОРБА
            if (xpOrbPrefab != null)
            {
                GameObject orbObj = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
                Progression.XPOrb orb = orbObj.GetComponent<Progression.XPOrb>();
                if (orb != null)
                {
                    orb.SetXPValue(xpReward);
                }
            }

            // Уведомляем спавнер
            var spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.OnEnemyDied(this);
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Мигание при получении урона
        /// </summary>
        protected System.Collections.IEnumerator FlashWhite()
        {
            if (spriteRenderer == null) yield break;

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        /// <summary>
        /// Столкновение с игроком - урон
        /// </summary>
        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (Time.time >= lastDamageTime + damageInterval)
                {
                    lastDamageTime = Time.time;

                    PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.TakeDamage(contactDamage);
                    }
                }
            }
        }

        /// <summary>
        /// Получить расстояние до игрока
        /// </summary>
        protected float GetDistanceToPlayer()
        {
            if (playerTransform == null) return float.MaxValue;
            return Vector2.Distance(transform.position, playerTransform.position);
        }
    }
}