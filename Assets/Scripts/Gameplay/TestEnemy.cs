using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Простой враг для тестирования. Преследует игрока и наносит урон при столкновении.
    /// </summary>
    public class TestEnemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int maxHP = 50;
        [SerializeField] private int currentHP = 50;
        [SerializeField] private int contactDamage = 10; // Урон при касании игрока
        [SerializeField] private float damageInterval = 1f; // Как часто наносим урон

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private Rigidbody2D rb;

        private Transform playerTransform;
        private float lastDamageTime = 0f;
        
        // Для визуального фидбэка
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private void Start()
        {
            currentHP = maxHP;

            // Находим игрока
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }

            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            // Сохраняем оригинальный цвет
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void FixedUpdate()
        {
            // Движение к игроку
            if (playerTransform != null)
            {
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                // УБРАЛИ ВРАЩЕНИЕ - враг теперь не вращается, только движется
                // Если хочешь чтобы враг всё же вращался - оставь код ниже раскомментированным:
                // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                // transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHP -= damage;

            Debug.Log($"[Enemy] Took {damage} damage. HP: {currentHP}/{maxHP}");

            // Визуальный фидбэк - мигание белым
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
        /// Мигание белым цветом при получении урона
        /// </summary>
        private System.Collections.IEnumerator FlashWhite()
        {
            if (spriteRenderer == null) yield break;

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        /// <summary>
        /// Смерть врага
        /// </summary>
        private void Die()
        {
            Debug.Log("[Enemy] DIED!");

            // Добавляем очки игроку
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.AddScore(10);
                Core.GameManager.Instance.AddSouls(1);
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Столкновение с игроком
        /// </summary>
        private void OnCollisionStay2D(Collision2D collision)
        {
            // Проверяем что это игрок
            if (collision.gameObject.CompareTag("Player"))
            {
                // Наносим урон с интервалом
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
    }
}