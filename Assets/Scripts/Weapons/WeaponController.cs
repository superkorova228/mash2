using UnityEngine;

namespace RhythmHell.Weapons
{
    /// <summary>
    /// Контроллер оружия. Управляет текущим оружием игрока и стрельбой.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("Current Weapon")]
        [SerializeField] private WeaponData currentWeapon;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer weaponSpriteRenderer;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Transform weaponPivot; // Для поворота

        [Header("Laser Settings")]
        [SerializeField] private LineRenderer laserLineRenderer; // Для визуализации луча
        [SerializeField] private LayerMask enemyLayer; // Слой врагов

        private float laserVisibleTime = 0f;

        private void Awake()
        {
            // Создаём LineRenderer для лазера если нет
            if (laserLineRenderer == null)
            {
                GameObject laserObj = new GameObject("LaserLine");
                laserObj.transform.SetParent(transform);
                laserLineRenderer = laserObj.AddComponent<LineRenderer>();
                
                // Настройка LineRenderer
                laserLineRenderer.startWidth = 0.1f;
                laserLineRenderer.endWidth = 0.1f;
                laserLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                laserLineRenderer.startColor = Color.red;
                laserLineRenderer.endColor = Color.red;
                laserLineRenderer.enabled = false;
            }
        }

        private void Start()
        {
            // Устанавливаем стартовое оружие если назначено
            if (currentWeapon != null)
            {
                EquipWeapon(currentWeapon);
            }
        }

        private void Update()
        {
            // Скрываем лазер через время
            if (laserLineRenderer.enabled && Time.time > laserVisibleTime)
            {
                laserLineRenderer.enabled = false;
            }

            // ТЕСТИРОВАНИЕ: Смена оружия клавишами 1, 2, 3
            // Удалить это в финальной версии!
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // Загружаем оружие из Resources (для теста)
                WeaponData pistol = Resources.Load<WeaponData>("Weapons/Pistol");
                if (pistol != null) EquipWeapon(pistol);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                WeaponData shotgun = Resources.Load<WeaponData>("Weapons/Shotgun");
                if (shotgun != null) EquipWeapon(shotgun);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                WeaponData laser = Resources.Load<WeaponData>("Weapons/LaserCannon");
                if (laser != null) EquipWeapon(laser);
            }
            #endif
        }

        /// <summary>
        /// Экипировать новое оружие
        /// </summary>
        public void EquipWeapon(WeaponData weapon)
        {
            if (weapon == null) return;

            currentWeapon = weapon;

            // Обновляем визуал
            if (weaponSpriteRenderer != null && weapon.weaponSprite != null)
            {
                weaponSpriteRenderer.sprite = weapon.weaponSprite;
                weaponSpriteRenderer.transform.localScale = weapon.weaponScale;
                weaponSpriteRenderer.transform.localPosition = weapon.weaponOffset;
            }

            Debug.Log($"[WeaponController] Equipped: {weapon.weaponName}");
        }

        /// <summary>
        /// Выстрелить (вызывается из PlayerController)
        /// </summary>
        public void Fire(int timing)
        {
            if (currentWeapon == null)
            {
                Debug.LogWarning("[WeaponController] No weapon equipped!");
                return;
            }

            // Вычисляем финальный урон с учётом перков
            int finalDamage = CalculateFinalDamage(timing);

            // Стреляем в зависимости от паттерна
            switch (currentWeapon.firePattern)
            {
                case FirePattern.Single:
                    FireSingle(finalDamage);
                    break;

                case FirePattern.Spread:
                    FireSpread(finalDamage);
                    break;

                case FirePattern.Laser:
                    FireLaser(finalDamage);
                    break;
            }
        }

        /// <summary>
        /// Вычислить финальный урон с перками
        /// </summary>
        private int CalculateFinalDamage(int timing)
        {
            // Базовый урон оружия
            int damage = currentWeapon.baseDamage;

            // Бонус за точность
            float timingMultiplier = timing == 2 ? 1.5f : timing == 1 ? 1.2f : 1f;
            damage = Mathf.RoundToInt(damage * timingMultiplier);

            // Применяем перки
            if (Progression.PlayerStats.Instance != null)
            {
                damage = Mathf.RoundToInt(damage * Progression.PlayerStats.Instance.damageMultiplier);
            }

            return damage;
        }

        /// <summary>
        /// Одиночный выстрел (пистолет)
        /// </summary>
        private void FireSingle(int damage)
        {
            if (currentWeapon.bulletPrefab == null || shootPoint == null) return;

            GameObject bulletObj = Instantiate(currentWeapon.bulletPrefab, shootPoint.position, shootPoint.rotation);
            
            Gameplay.Bullet bullet = bulletObj.GetComponent<Gameplay.Bullet>();
            if (bullet != null)
            {
                bullet.SetDamage(damage);
            }
        }

        /// <summary>
        /// Разброс (дробовик)
        /// </summary>
        private void FireSpread(int damage)
        {
            if (currentWeapon.bulletPrefab == null || shootPoint == null) return;

            int pellets = currentWeapon.pelletsCount;
            float spread = currentWeapon.spreadAngle;

            for (int i = 0; i < pellets; i++)
            {
                // Вычисляем угол для каждой дробинки
                float angleOffset = Random.Range(-spread, spread);
                Quaternion rotation = shootPoint.rotation * Quaternion.Euler(0, 0, angleOffset);

                // Спавним дробинку
                GameObject bulletObj = Instantiate(currentWeapon.bulletPrefab, shootPoint.position, rotation);
                
                Gameplay.Bullet bullet = bulletObj.GetComponent<Gameplay.Bullet>();
                if (bullet != null)
                {
                    bullet.SetDamage(damage / pellets); // Урон делится на количество дробинок
                }
            }
        }

        /// <summary>
        /// Лазерный выстрел (пробивающий луч)
        /// </summary>
        private void FireLaser(int damage)
        {
            if (shootPoint == null) return;

            Vector2 startPos = shootPoint.position;
            Vector2 direction = shootPoint.right; // Направление выстрела

            // Raycast для поиска всех врагов на пути
            RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, direction, currentWeapon.laserRange, enemyLayer);

            // Наносим урон всем врагам на пути
            foreach (var hit in hits)
            {
                Gameplay.EnemyBase enemy = hit.collider.GetComponent<Gameplay.EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }

            // Визуализация луча
            ShowLaser(startPos, startPos + direction * currentWeapon.laserRange);

            Debug.Log($"[WeaponController] Laser hit {hits.Length} enemies");
        }

        /// <summary>
        /// Показать лазерный луч
        /// </summary>
        private void ShowLaser(Vector2 start, Vector2 end)
        {
            if (laserLineRenderer == null) return;

            laserLineRenderer.SetPosition(0, start);
            laserLineRenderer.SetPosition(1, end);
            laserLineRenderer.startWidth = currentWeapon.laserWidth;
            laserLineRenderer.endWidth = currentWeapon.laserWidth;
            laserLineRenderer.enabled = true;

            laserVisibleTime = Time.time + currentWeapon.laserDuration;
        }

        // Публичные свойства
        public WeaponData CurrentWeapon => currentWeapon;
    }
}