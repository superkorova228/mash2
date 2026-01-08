using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Пуля врага. Летит прямо, наносит урон игроку.
    /// </summary>
    public class EnemyBullet : MonoBehaviour
    {
        private Vector2 direction;
        private float speed;
        private int damage;
        private float lifetime = 5f;
        private float spawnTime;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spawnTime = Time.time;
        }

        /// <summary>
        /// Инициализация пули (вызывается при спавне)
        /// </summary>
        public void Initialize(Vector2 dir, float spd, int dmg)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;

            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
        }

        private void Update()
        {
            // Самоуничтожение через lifetime
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Столкновение с игроком
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Проверяем что это игрок
            if (collision.CompareTag("Player"))
            {
                PlayerController player = collision.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }

                Destroy(gameObject);
            }
        }
    }
}