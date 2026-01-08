using UnityEngine;
using System.Collections.Generic;

namespace RhythmHell.Progression
{
    /// <summary>
    /// Статистика игрока и активные перки.
    /// Singleton - хранит все улучшения игрока за раунд.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Current Stats")]
        public float damageMultiplier = 1f;      // 1.0 = 100%
        public float fireRateMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
        public int maxHPBonus = 0;               // Абсолютное значение
        public float regenRate = 0f;             // HP в секунду
        
        [Header("Special Perks")]
        public bool hasPiercingShots = false;
        public bool hasExplosiveBullets = false;
        public bool hasDash = false;
        public float lifeStealPercent = 0f;      // 0-100%
        public float critChance = 0f;            // 0-100%
        public float critMultiplier = 2f;        // 2x урон при крите

        [Header("Active Perks")]
        public List<PerkData> activePerksList = new List<PerkData>();

        // События
        public event System.Action<PerkData> OnPerkAdded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            // НЕ используем DontDestroyOnLoad - статы должны сбрасываться между раундами
        }

        /// <summary>
        /// Применить перк
        /// </summary>
        public void ApplyPerk(PerkData perk)
        {
            if (perk == null) return;

            Debug.Log($"[PlayerStats] Applying perk: {perk.perkName}");

            // Применяем эффект в зависимости от типа
            switch (perk.perkType)
            {
                case PerkType.IncreaseDamage:
                    damageMultiplier += perk.value / 100f;
                    break;

                case PerkType.IncreaseFireRate:
                    fireRateMultiplier += perk.value / 100f;
                    break;

                case PerkType.IncreaseSpeed:
                    moveSpeedMultiplier += perk.value / 100f;
                    break;

                case PerkType.IncreaseMaxHP:
                    maxHPBonus += (int)perk.value;
                    // Применяем к игроку
                    if (Gameplay.PlayerController.Instance != null)
                    {
                        Gameplay.PlayerController.Instance.Heal((int)perk.value);
                    }
                    break;

                case PerkType.Regeneration:
                    regenRate += perk.value;
                    break;

                case PerkType.PiercingShots:
                    hasPiercingShots = true;
                    break;

                case PerkType.ExplosiveBullets:
                    hasExplosiveBullets = true;
                    break;

                case PerkType.Dash:
                    hasDash = true;
                    break;

                case PerkType.LifeSteal:
                    lifeStealPercent += perk.value;
                    break;

                case PerkType.LuckyShots:
                    critChance += perk.value;
                    break;

                case PerkType.WeaponPickup:
                    // Даём игроку новое оружие
                    if (perk.weaponToGrant != null)
                    {
                        var weaponController = Gameplay.PlayerController.Instance?.GetComponentInChildren<Weapons.WeaponController>();
                        if (weaponController != null)
                        {
                            weaponController.EquipWeapon(perk.weaponToGrant);
                            Debug.Log($"[PlayerStats] Granted weapon: {perk.weaponToGrant.weaponName}");
                        }
                    }
                    break;
            }

            // Добавляем в список активных перков
            activePerksList.Add(perk);

            // Уведомляем подписчиков
            OnPerkAdded?.Invoke(perk);
        }

        /// <summary>
        /// Проверка есть ли уже этот перк
        /// </summary>
        public bool HasPerk(PerkData perk)
        {
            return activePerksList.Contains(perk);
        }

        /// <summary>
        /// Сброс всех перков (новая игра)
        /// </summary>
        public void ResetStats()
        {
            damageMultiplier = 1f;
            fireRateMultiplier = 1f;
            moveSpeedMultiplier = 1f;
            maxHPBonus = 0;
            regenRate = 0f;
            
            hasPiercingShots = false;
            hasExplosiveBullets = false;
            hasDash = false;
            lifeStealPercent = 0f;
            critChance = 0f;
            
            activePerksList.Clear();

            Debug.Log("[PlayerStats] Stats reset");
        }

        private void Update()
        {
            // Регенерация HP
            if (regenRate > 0f && Gameplay.PlayerController.Instance != null)
            {
                float regenAmount = regenRate * Time.deltaTime;
                if (regenAmount >= 1f)
                {
                    Gameplay.PlayerController.Instance.Heal(Mathf.FloorToInt(regenAmount));
                }
            }
        }
    }
}