using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Shooter - стреляет в игрока на расстоянии.
    /// Держит дистанцию и атакует издалека.
    /// </summary>
    public class EnemyShooter : EnemyBase
    {
        [Header("Shooter Settings")]
        [SerializeField] private float attackRange = 8f; // Дистанция стрельбы
        [SerializeField] private float minDistance = 5f; // Минимальная дистанция (не подходит ближе)
        [SerializeField] private float shootInterval = 2f; // Как часто стреляет
        [SerializeField] private GameObject enemyBulletPrefab; // Префаб пули врага

        private float lastShootTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            
            // Устанавливаем тип
            enemyType = EnemyType.Shooter;
            
            // Параметры Shooter
            if (maxHP == 50) maxHP = 40; // Средний HP
            if (moveSpeed == 2f) moveSpeed = 2.5f; // Средняя скорость
            if (contactDamage == 10) contactDamage = 5; // Меньше урона при касании
            if (scoreReward == 10) scoreReward = 15;
            if (soulReward == 1) soulReward = 2;
        }

        protected override void FixedUpdate()
        {
            if (playerTransform == null) return;

            float distanceToPlayer = GetDistanceToPlayer();

            // Поведение в зависимости от дистанции
            if (distanceToPlayer > attackRange)
            {
                // Слишком далеко - подходим ближе
                MoveTowardsPlayer();
            }
            else if (distanceToPlayer < minDistance)
            {
                // Слишком близко - отходим назад
                MoveAwayFromPlayer();
            }
            else
            {
                // Идеальная дистанция - стоим и стреляем
                rb.linearVelocity = Vector2.zero;
                TryShoot();
            }
        }

        /// <summary>
        /// Движение от игрока (отступление)
        /// </summary>
        private void MoveAwayFromPlayer()
        {
            if (playerTransform != null && rb != null)
            {
                Vector2 direction = (transform.position - playerTransform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
        }

        /// <summary>
        /// Попытка выстрела
        /// </summary>
        private void TryShoot()
        {
            if (Time.time >= lastShootTime + shootInterval)
            {
                lastShootTime = Time.time;
                Shoot();
            }
        }

        /// <summary>
        /// Выстрел в игрока
        /// </summary>
        private void Shoot()
        {
            if (enemyBulletPrefab == null || playerTransform == null) return;

            // Направление к игроку
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Спавним пулю
            GameObject bulletObj = Instantiate(enemyBulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));

            // Настраиваем пулю
            EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();
            if (bullet != null)
            {
                bullet.Initialize(direction, 8f, 15); // Скорость 8, урон 15
            }

            Debug.Log("[Shooter] Pew!");
        }
    }
}