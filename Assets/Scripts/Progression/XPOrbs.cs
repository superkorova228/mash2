using UnityEngine;

namespace RhythmHell.Progression
{
    /// <summary>
    /// Орб опыта (душа). Дропается врагами, собирается игроком.
    /// Исчезает через время если не собран.
    /// </summary>
    public class XPOrb : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int xpValue = 10; // Сколько XP даёт
        [SerializeField] private float lifetime = 8f; // Время жизни (секунды)
        [SerializeField] private float magnetRange = 3f; // Дистанция притяжения к игроку
        [SerializeField] private float magnetSpeed = 8f; // Скорость притяжения

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float pulseSpeed = 2f; // Скорость пульсации

        private Transform playerTransform;
        private float spawnTime;
        private bool isBeingCollected = false;
        private Vector3 originalScale;

        private void Start()
        {
            spawnTime = Time.time;
            
            // Находим игрока
            if (Gameplay.PlayerController.Instance != null)
            {
                playerTransform = Gameplay.PlayerController.Instance.transform;
            }

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            originalScale = transform.localScale;
        }

        private void Update()
        {
            // Проверяем время жизни
            float age = Time.time - spawnTime;
            if (age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Мигание когда скоро исчезнет
            if (age >= lifetime - 2f)
            {
                float alpha = Mathf.PingPong(Time.time * 3f, 1f);
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = alpha;
                    spriteRenderer.color = color;
                }
            }

            // Пульсация
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.1f;
            transform.localScale = originalScale * pulse;

            // Притяжение к игроку
            if (playerTransform != null && !isBeingCollected)
            {
                float distance = Vector2.Distance(transform.position, playerTransform.position);
                
                if (distance <= magnetRange)
                {
                    // Летим к игроку
                    Vector2 direction = (playerTransform.position - transform.position).normalized;
                    transform.position += (Vector3)direction * magnetSpeed * Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// Собран игроком
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && !isBeingCollected)
            {
                isBeingCollected = true;
                Collect();
            }
        }

        /// <summary>
        /// Собрать орб
        /// </summary>
        private void Collect()
        {
            // Даём XP игроку
            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.AddExperience(xpValue);
            }

            // ЗВУК сбора (опционально)
            // if (Core.AudioManager.Instance != null)
            // {
            //     Core.AudioManager.Instance.PlaySound2D(Core.SoundType.XPCollect);
            // }

            // Уничтожаем орб
            Destroy(gameObject);
        }

        /// <summary>
        /// Установить количество XP
        /// </summary>
        public void SetXPValue(int value)
        {
            xpValue = value;
        }
    }
}