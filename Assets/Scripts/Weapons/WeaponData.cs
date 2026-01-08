using UnityEngine;

namespace RhythmHell.Weapons
{
    /// <summary>
    /// Типы оружия
    /// </summary>
    public enum WeaponType
    {
        Pistol,      // Стандартный пистолет
        Shotgun,     // Дробовик (разброс)
        LaserCannon  // Лазерная пушка (пробивающий луч)
    }

    /// <summary>
    /// Паттерн стрельбы
    /// </summary>
    public enum FirePattern
    {
        Single,   // Одна пуля
        Spread,   // Разброс (дробовик)
        Laser     // Луч
    }

    /// <summary>
    /// ScriptableObject с данными оружия.
    /// Создаётся через Create → RhythmHell → Weapon Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "RhythmHell/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        public string weaponName = "New Weapon";
        [TextArea(2, 4)]
        public string description = "Weapon description";
        public Sprite weaponSprite; // Спрайт оружия
        public Sprite iconSprite; // Иконка для UI
        public WeaponType weaponType;

        [Header("Fire Pattern")]
        public FirePattern firePattern = FirePattern.Single;
        
        [Header("Damage")]
        public int baseDamage = 25; // Базовый урон одной пули
        
        [Header("Spread Settings (for Shotgun)")]
        public int pelletsCount = 4; // Количество дробинок
        [Range(0f, 30f)] public float spreadAngle = 15f; // Угол разброса (градусы)
        
        [Header("Laser Settings")]
        public float laserRange = 15f; // Дальность луча
        public float laserWidth = 0.2f; // Толщина луча
        public float laserDuration = 0.1f; // Длительность визуализации
        
        [Header("Bullet")]
        public GameObject bulletPrefab; // Префаб пули (для Single и Spread)

        [Header("Visual")]
        public Vector3 weaponScale = Vector3.one; // Масштаб спрайта оружия
        public Vector2 weaponOffset = Vector2.zero; // Смещение от игрока

        [Header("Rarity (for drops)")]
        public int rarityWeight = 100; // Вес для случайного выпадения
        public Color rarityColor = Color.white;
    }
}