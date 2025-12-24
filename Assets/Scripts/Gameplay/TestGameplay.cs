using UnityEngine;
using mash2.Core;
using mash2.UI;

namespace mash2.Gameplay
{
    public class TestGameplay : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private int scorePerSecond = 10;
        [SerializeField] private float gameOverAfterSeconds = 30f;
        
        [Header("HUD Reference")]
        [SerializeField] private GameplayHUD hud;
        
        private float timer = 0f;
        private float damageTimer = 0f;

        private void Start()
        {
            Debug.Log("=== TEST GAMEPLAY STARTED ===");
            Debug.Log("Press ESC to pause/resume");
            Debug.Log("Press G to trigger Game Over");
            Debug.Log("Press H to take damage");
            Debug.Log($"Auto Game Over in {gameOverAfterSeconds} seconds");
            
            // Найти HUD если не назначен
            if (hud == null)
                hud = FindObjectOfType<GameplayHUD>();
        }

        private void Update()
        {
            if (GameManager.Instance.IsPaused)
                return;
            
            // Добавляем очки
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0f;
                GameManager.Instance.AddScore(scorePerSecond);
            }
            
            // Автоматический урон каждые 3 секунды (для теста)
            damageTimer += Time.deltaTime;
            if (damageTimer >= 3f)
            {
                damageTimer = 0f;
                if (hud != null)
                    hud.TakeDamage(10f);
            }
            
            // Auto Game Over
            if (GameManager.Instance.GameplayTime >= gameOverAfterSeconds)
            {
                GameManager.Instance.TriggerGameOver();
            }
            
            // Ручной Game Over
            if (Input.GetKeyDown(KeyCode.G))
            {
                GameManager.Instance.TriggerGameOver();
            }
            
            // Ручной урон
            if (Input.GetKeyDown(KeyCode.H) && hud != null)
            {
                hud.TakeDamage(20f);
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 400, 30), "Press ESC - Pause/Resume");
            GUI.Label(new Rect(10, 40, 400, 30), "Press G - Trigger Game Over");
            GUI.Label(new Rect(10, 70, 400, 30), "Press H - Take Damage");
        }
    }
}