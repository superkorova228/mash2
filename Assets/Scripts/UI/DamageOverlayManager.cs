using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace RhythmHell.UI
{
    /// <summary>
    /// Менеджер оверлея урона. Показывает случайную смешную картинку при получении урона.
    /// Картинка плавно появляется и исчезает.
    /// </summary>
    public class DamageOverlayManager : MonoBehaviour
    {
        public static DamageOverlayManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image overlayImage;
        [SerializeField] private CanvasGroup overlayCanvasGroup;

        [Header("Damage Images")]
        [SerializeField] private List<Sprite> damageSprites = new List<Sprite>();

        [Header("Settings")]
        [SerializeField] private float displayDuration = 0.5f; // Как долго показывать
        [SerializeField] private float fadeInSpeed = 10f; // Скорость появления
        [SerializeField] private float fadeOutSpeed = 5f; // Скорость исчезновения
        [SerializeField] private float maxAlpha = 0.3f; // Максимальная прозрачность (чтобы не закрывать весь экран)

        private Coroutine currentOverlayCoroutine;
        private bool isShowing = false;
        private int lastKnownHP = -1; // Для отслеживания урона

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Скрываем оверлей при старте
            if (overlayCanvasGroup != null)
            {
                overlayCanvasGroup.alpha = 0f;
            }

            // ИСПРАВЛЕНИЕ: Принудительно растягиваем Image на весь экран
            if (overlayImage != null)
            {
                RectTransform rect = overlayImage.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                }
            }

            // Подписываемся на получение урона игроком
            SubscribeToPlayer();

            // Подписываемся на изменение состояния игры
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }

            // Проверяем что есть картинки
            if (damageSprites.Count == 0)
            {
                Debug.LogWarning("[DamageOverlay] No damage sprites assigned! Add some meme images!");
            }
        }

        /// <summary>
        /// Подписаться на события игрока
        /// </summary>
        private void SubscribeToPlayer()
        {
            if (Gameplay.PlayerController.Instance != null)
            {
                Gameplay.PlayerController.Instance.OnHealthChanged += OnPlayerDamaged;
            }
        }

        /// <summary>
        /// Отписаться от событий игрока
        /// </summary>
        private void UnsubscribeFromPlayer()
        {
            if (Gameplay.PlayerController.Instance != null)
            {
                Gameplay.PlayerController.Instance.OnHealthChanged -= OnPlayerDamaged;
            }
        }

        private void OnDestroy()
        {
            // Отписываемся
            UnsubscribeFromPlayer();

            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        /// <summary>
        /// Реакция на изменение состояния игры
        /// </summary>
        private void OnGameStateChanged(Core.GameState newState)
        {
            // При Game Over или Pause - скрываем оверлей и отписываемся
            if (newState == Core.GameState.GameOver || newState == Core.GameState.Paused)
            {
                // Останавливаем текущую анимацию
                if (currentOverlayCoroutine != null)
                {
                    StopCoroutine(currentOverlayCoroutine);
                    currentOverlayCoroutine = null;
                }

                // Скрываем оверлей немедленно
                if (overlayCanvasGroup != null)
                {
                    overlayCanvasGroup.alpha = 0f;
                }

                isShowing = false;

                // При Game Over отписываемся от игрока
                if (newState == Core.GameState.GameOver)
                {
                    UnsubscribeFromPlayer();
                    Debug.Log("[DamageOverlay] Unsubscribed from player (Game Over)");
                }
            }
            // При возврате в Playing - подписываемся обратно
            else if (newState == Core.GameState.Playing)
            {
                SubscribeToPlayer();
            }
        }

        /// <summary>
        /// Вызывается когда HP игрока изменяется
        /// </summary>
        private void OnPlayerDamaged(int newHP)
        {
            // ИСПРАВЛЕНО: Показываем только если HP УМЕНЬШИЛОСЬ
            if (lastKnownHP == -1)
            {
                // Первая инициализация - просто запоминаем HP
                lastKnownHP = newHP;
                return;
            }

            if (newHP < lastKnownHP)
            {
                // HP уменьшилось = получен урон
                ShowRandomOverlay();
            }

            lastKnownHP = newHP;
        }

        /// <summary>
        /// Показать случайный оверлей
        /// </summary>
        public void ShowRandomOverlay()
        {
            if (damageSprites.Count == 0)
            {
                Debug.LogWarning("[DamageOverlay] No sprites to show!");
                return;
            }

            if (isShowing)
            {
                Debug.Log("[DamageOverlay] Already showing, skipping");
                return;
            }

            // Выбираем случайный спрайт
            Sprite randomSprite = damageSprites[Random.Range(0, damageSprites.Count)];
            Debug.Log($"[DamageOverlay] Showing sprite: {randomSprite.name}");

            ShowOverlay(randomSprite);
        }

        /// <summary>
        /// Показать конкретный оверлей
        /// </summary>
        public void ShowOverlay(Sprite sprite)
        {
            if (sprite == null) return;

            // Останавливаем предыдущую корутину если есть
            if (currentOverlayCoroutine != null)
            {
                StopCoroutine(currentOverlayCoroutine);
            }

            // Устанавливаем спрайт
            if (overlayImage != null)
            {
                overlayImage.sprite = sprite;
            }

            // Запускаем анимацию
            currentOverlayCoroutine = StartCoroutine(OverlaySequence());
        }

        /// <summary>
        /// Анимация появления и исчезновения
        /// </summary>
        private IEnumerator OverlaySequence()
        {
            isShowing = true;

            // Fade In (появление)
            float alpha = 0f;
            while (alpha < maxAlpha)
            {
                alpha += fadeInSpeed * Time.deltaTime;
                if (overlayCanvasGroup != null)
                {
                    overlayCanvasGroup.alpha = Mathf.Min(alpha, maxAlpha);
                }
                yield return null;
            }

            // Держим на экране
            yield return new WaitForSeconds(displayDuration);

            // Fade Out (исчезновение)
            alpha = maxAlpha;
            while (alpha > 0f)
            {
                alpha -= fadeOutSpeed * Time.deltaTime;
                if (overlayCanvasGroup != null)
                {
                    overlayCanvasGroup.alpha = Mathf.Max(alpha, 0f);
                }
                yield return null;
            }

            isShowing = false;
        }

        /// <summary>
        /// Добавить спрайт в список (можно вызывать из других скриптов)
        /// </summary>
        public void AddDamageSprite(Sprite sprite)
        {
            if (sprite != null && !damageSprites.Contains(sprite))
            {
                damageSprites.Add(sprite);
            }
        }

        /// <summary>
        /// Очистить все спрайты
        /// </summary>
        public void ClearDamageSprites()
        {
            damageSprites.Clear();
        }
    }
}