using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Tank - медленный, но очень живучий враг.
    /// Много HP, медленная скорость, большой урон.
    /// </summary>
    public class EnemyTank : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            
            // Устанавливаем тип
            enemyType = EnemyType.Tank;
            
            // Параметры Tank
            if (maxHP == 50) maxHP = 150; // МНОГО HP
            if (moveSpeed == 2f) moveSpeed = 1.5f; // МЕДЛЕННЫЙ
            if (contactDamage == 10) contactDamage = 20; // БОЛЬШЕ урона
            if (scoreReward == 10) scoreReward = 30; // Больше наград
            if (soulReward == 1) soulReward = 3;
        }

        // Tank тоже просто идёт к игроку, но медленнее
    }
}