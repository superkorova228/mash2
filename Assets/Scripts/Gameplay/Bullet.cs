using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Пуля игрока. Летит прямо, наносит урон врагам, уничтожается при столкновении.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifetime = 3f; // Время жизни пули (секунды)

        private Rigidbody2D rb;
        private float spawnTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            spawnTime = Time.time;

            // ИСПРАВЛЕНО: Летим в направлении RIGHT (оружие смотрит вправо по оси X)
            // transform.right = направление вправо от объекта
            if (rb != null)
            {
                rb.linearVelocity = transform.right * speed;
            }
        }

        private void Update()
        {
            // Самоуничтожение через lifetime секунд
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Столкновение с врагом
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log($"[Bullet] Hit something: {collision.gameObject.name}, Tag: {collision.tag}");

            // Проверяем что это враг
            if (collision.CompareTag("Enemy"))
            {
                Debug.Log($"[Bullet] Hit enemy!");

                // Пробуем найти любой компонент врага
                EnemyBase enemy = collision.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"[Bullet] Dealt {damage} damage to {enemy.Type}");
                }
                else
                {
                    Debug.LogWarning($"[Bullet] Enemy has no EnemyBase component!");
                }

                // Уничтожаем пулю
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Установить урон (вызывается при спавне для разного урона)
        /// </summary>
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// Установить скорость (опционально)
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            if (rb != null)
            {
                rb.linearVelocity = transform.right * speed; // ИСПРАВЛЕНО
            }
        }
    }
}