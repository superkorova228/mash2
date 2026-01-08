using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Управление игроком: движение, поворот, HP.
    /// Позже добавим стрельбу привязанную к ритму.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField] private float baseMoveSpeed = 5f; // Базовая скорость
        [SerializeField] private Rigidbody2D rb;

        private float currentMoveSpeed; // Реальная скорость с учётом перков

        [Header("Weapon")]
        [SerializeField] private Transform weaponPivot; // Объект который вращается к мыши
        [SerializeField] private Transform shootPoint; // Точка откуда летят пули
        [SerializeField] private Weapons.WeaponController weaponController; // Контроллер оружия

        [Header("Health")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int currentHP = 100;

        [Header("Shooting (Placeholder)")]
        [SerializeField] private bool rhythmShooting = true; // Привязка к ритму
        [SerializeField] private float perfectWindow = 0.08f; // Окно для Perfect (строже!)
        [SerializeField] private float goodWindow = 0.15f; // Окно для Good (средне)

        private bool canShoot = true;

        // УДАЛЕНО: bulletPrefab больше не нужен здесь, используется WeaponController

        private Vector2 moveInput;
        private Vector2 mousePosition;
        private Camera mainCamera;

        // События для UI
        public event System.Action<int> OnHealthChanged;

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
            // Инициализация здоровья
            currentHP = maxHP;
            OnHealthChanged?.Invoke(currentHP);

            // Если нет Rigidbody2D, ищем его
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            // Обновляем статы с учётом перков
            UpdateStats();

            // Подписываемся на добавление перков
            if (Progression.PlayerStats.Instance != null)
            {
                Progression.PlayerStats.Instance.OnPerkAdded += OnPerkAdded;
            }
        }

        private void OnDestroy()
        {
            // Отписываемся
            if (Progression.PlayerStats.Instance != null)
            {
                Progression.PlayerStats.Instance.OnPerkAdded -= OnPerkAdded;
            }
        }

        /// <summary>
        /// Вызывается когда добавлен новый перк
        /// </summary>
        private void OnPerkAdded(Progression.PerkData perk)
        {
            Debug.Log($"[Player] New perk applied: {perk.perkName}");
            UpdateStats();
        }

        /// <summary>
        /// Обновить статы игрока с учётом перков
        /// </summary>
        private void UpdateStats()
        {
            // Скорость движения
            currentMoveSpeed = baseMoveSpeed;
            if (Progression.PlayerStats.Instance != null)
            {
                currentMoveSpeed *= Progression.PlayerStats.Instance.moveSpeedMultiplier;
            }

            Debug.Log($"[Player] Stats updated - Speed: {currentMoveSpeed} (base: {baseMoveSpeed}, mult: {Progression.PlayerStats.Instance?.moveSpeedMultiplier})");
        }

        private void Update()
        {
            // Проверяем что игра не на паузе
            if (Core.GameManager.Instance != null && 
                Core.GameManager.Instance.CurrentState != Core.GameState.Playing)
            {
                return; // Не обрабатываем ввод если не в игре
            }

            // Получение ввода движения (WASD или стрелки)
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput.Normalize(); // Чтобы диагональное движение не было быстрее

            // Получение позиции мыши для поворота
            mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // Стрельба
            if (Input.GetMouseButtonDown(0) && canShoot) // Left Click
            {
                TryShoot();
            }
        }

        private void FixedUpdate()
        {
            // Движение в FixedUpdate для физики
            if (rb != null)
            {
                rb.linearVelocity = moveInput * currentMoveSpeed;
            }

            // Поворот ОРУЖИЯ в сторону мыши (игрок сам НЕ вращается)
            RotateWeaponTowardsMouse();
        }

        /// <summary>
        /// Поворот ОРУЖИЯ в сторону курсора мыши (игрок остаётся неподвижным)
        /// </summary>
        private void RotateWeaponTowardsMouse()
        {
            if (weaponPivot == null) return;

            // Вектор от игрока к мыши
            Vector2 lookDirection = mousePosition - (Vector2)transform.position;
            
            // Вычисляем угол (без смещения -90, т.к. оружие смотрит вправо по умолчанию)
            float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            
            // Поворачиваем оружие
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);

            // Переворачиваем оружие если целимся влево (чтобы не было вверх ногами)
            if (angle > 90f || angle < -90f)
            {
                weaponPivot.localScale = new Vector3(1, -1, 1); // Отражаем по Y
            }
            else
            {
                weaponPivot.localScale = new Vector3(1, 1, 1); // Нормальная ориентация
            }
        }

        /// <summary>
        /// Попытка выстрела с проверкой ритма
        /// </summary>
        private void TryShoot()
        {
            if (!rhythmShooting)
            {
                // Режим без ритма (для тестирования)
                Shoot(2); // Perfect по умолчанию
                return;
            }

            // Проверяем ритм через BeatManager
            if (Rhythm.BeatManager.Instance != null)
            {
                int timing = Rhythm.BeatManager.Instance.CheckTiming(perfectWindow, goodWindow);

                if (timing > 0)
                {
                    // Попали в ритм!
                    Shoot(timing);
                }
                else
                {
                    // Промах по ритму - осечка
                    ShootMiss();
                }
            }
            else
            {
                // BeatManager нет - стреляем без проверки
                Shoot(2);
            }
        }

        /// <summary>
        /// Выстрел (успешный)
        /// timing: 2 = Perfect, 1 = Good
        /// </summary>
        private void Shoot(int timing)
        {
            string timingText = timing == 2 ? "PERFECT!" : "GOOD";
            Debug.Log($"[Player] {timingText} BANG!");

            // ЗВУК выстрела
            if (Core.AudioManager.Instance != null)
            {
                Core.SoundType shootSound = timing == 2 ? Core.SoundType.ShootPerfect : Core.SoundType.ShootGood;
                Core.AudioManager.Instance.PlaySound2D(shootSound);
            }

            // Уведомляем Rhythm Lane о выстреле
            var rhythmLane = FindObjectOfType<UI.RhythmLaneUI>();
            if (rhythmLane != null)
            {
                rhythmLane.OnPlayerShoot(timing);
            }

            // Добавляем очки в зависимости от точности
            if (Core.GameManager.Instance != null)
            {
                int scoreBonus = timing == 2 ? 10 : 5;
                Core.GameManager.Instance.AddScore(scoreBonus);
            }

            // СТРЕЛЯЕМ ЧЕРЕЗ WEAPON CONTROLLER
            if (weaponController != null)
            {
                weaponController.Fire(timing);
            }
            else
            {
                Debug.LogWarning("[Player] No WeaponController!");
            }
        }

        /// <summary>
        /// Промах по ритму
        /// </summary>
        private void ShootMiss()
        {
            Debug.Log("[Player] MISS! (вне ритма)");
            
            // ЗВУК промаха
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySound2D(Core.SoundType.ShootMiss);
            }
        }

        /// <summary>
        /// Получить урон
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHP -= damage;
            
            if (currentHP < 0)
                currentHP = 0;

            // ЗВУК получения урона
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySound2D(Core.SoundType.PlayerHit);
            }

            // Уведомляем UI
            OnHealthChanged?.Invoke(currentHP);

            Debug.Log($"[Player] Took {damage} damage. HP: {currentHP}/{maxHP}");

            // Проверка смерти
            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Смерть игрока
        /// </summary>
        private void Die()
        {
            Debug.Log("[Player] DIED!");

            // ЗВУК смерти
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySound2D(Core.SoundType.PlayerDeath);
            }

            // Вызываем Game Over
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.GameOver();
            }

            // Деактивируем игрока
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Лечение
        /// </summary>
        public void Heal(int amount)
        {
            currentHP += amount;
            
            if (currentHP > maxHP)
                currentHP = maxHP;

            OnHealthChanged?.Invoke(currentHP);
        }

        // Публичные свойства
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
    }
}