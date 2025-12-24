using UnityEngine;
using mash2.Core;
using mash2.UI; // ← ДОБАВЬ ЭТУ СТРОКУ

namespace mash2.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 30f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float damageToPlayer = 10f;
        [SerializeField] private float damageInterval = 1f;
        
        [Header("Rewards")]
        [SerializeField] private int scoreReward = 100;
        
        private float currentHealth;
        private Transform player;
        private Rigidbody2D rb;
        private float lastDamageTime = 0f;
        private GameplayHUD hud; // Кешируем ссылку

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            currentHealth = maxHealth;
        }

        private void Start()
        {
            // Находим игрока
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            
            // Находим HUD один раз (новый метод Unity 6)
            hud = FindFirstObjectByType<GameplayHUD>();
        }

        private void FixedUpdate()
        {
            if (player == null)
                return;
            
            // Движемся к игроку
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            
            Debug.Log($"Enemy took {damage} damage. HP: {currentHealth}/{maxHealth}");
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Enemy died!");
    
            // Звук смерти
            if (mash2.Audio.AudioManager.Instance != null)
            {       
                mash2.Audio.AudioManager.Instance.PlayEnemyDeathSound();
            }
    
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreReward);
            }
    
            Destroy(gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (Time.time >= lastDamageTime + damageInterval)
                {
                    lastDamageTime = Time.time;
                    
                    if (hud != null)
                    {
                        hud.TakeDamage(damageToPlayer);
                        Debug.Log($"Player took {damageToPlayer} damage from enemy!");
                    }
                }
            }
        }
    }
}