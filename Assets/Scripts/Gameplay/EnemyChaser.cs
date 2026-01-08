using UnityEngine;

namespace RhythmHell.Gameplay
{
    /// <summary>
    /// Chaser - стандартный враг.
    /// Просто преследует игрока и наносит урон при касании.
    /// </summary>
    public class EnemyChaser : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            
            // Устанавливаем тип
            enemyType = EnemyType.Chaser;
            
            // Стандартные параметры Chaser (можно переопределить в Inspector)
            if (maxHP == 50) maxHP = 30; // Меньше HP чем по умолчанию
            if (moveSpeed == 2f) moveSpeed = 3f; // Быстрее
            if (contactDamage == 10) contactDamage = 10;
            if (scoreReward == 10) scoreReward = 10;
            if (soulReward == 1) soulReward = 1;
        }

        // Chaser использует базовое поведение - просто движется к игроку
        // Ничего переопределять не нужно!
    }
}