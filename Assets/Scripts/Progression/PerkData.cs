using UnityEngine;

namespace RhythmHell.Progression
{
    /// <summary>
    /// Типы перков
    /// </summary>
    public enum PerkType
    {
        // Боевые
        IncreaseDamage,      // Увеличить урон
        IncreaseFireRate,    // Увеличить скорострельность
        PiercingShots,       // Пули пробивают врагов
        
        // Защитные
        IncreaseMaxHP,       // Увеличить максимум HP
        Regeneration,        // Регенерация HP
        
        // Мобильность
        IncreaseSpeed,       // Увеличить скорость
        Dash,                // Рывок (dash ability)
        
        // Специальные
        ExplosiveBullets,    // Взрывные пули
        LifeSteal,           // Вампиризм
        LuckyShots,          // Шанс крита
        
        // Оружие (НОВОЕ!)
        WeaponPickup         // Получить новое оружие
    }

    /// <summary>
    /// ScriptableObject с данными перка.
    /// Создаётся через Create → RhythmHell → Perk Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewPerk", menuName = "RhythmHell/Perk Data")]
    public class PerkData : ScriptableObject
    {
        [Header("Basic Info")]
        public string perkName = "New Perk";
        [TextArea(3, 5)]
        public string description = "Perk description here";
        public Sprite icon; // Иконка перка
        public PerkType perkType;

        [Header("Stats")]
        public float value = 10f; // Значение эффекта (например +10% урона)
        public bool isPercentage = true; // Проценты или абсолютное значение
        public bool isStackable = true; // Можно ли брать несколько раз

        [Header("Weapon Pickup (for WeaponPickup type)")]
        public Weapons.WeaponData weaponToGrant; // Оружие которое даётся

        [Header("Rarity")]
        public int rarityWeight = 100; // Вес для случайного выбора (больше = чаще)
        public Color rarityColor = Color.white; // Цвет рамки карточки

        /// <summary>
        /// Получить форматированное описание со значением
        /// </summary>
        public string GetFormattedDescription()
        {
            string valueStr = isPercentage ? $"+{value}%" : $"+{value}";
            return description.Replace("{value}", valueStr);
        }
    }
}