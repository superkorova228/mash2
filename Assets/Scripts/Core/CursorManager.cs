using UnityEngine;
using UnityEngine.UI;

namespace RhythmHell.Core
{
    /// <summary>
    /// Управление курсором. Блокирует системный курсор и показывает кастомный прицел.
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Cursor Settings")]
        [SerializeField] private bool hideSystemCursor = false; // Изменено на false
        [SerializeField] private bool confineCursor = true; // Запирает курсор в окне
        [SerializeField] private Texture2D customCursorTexture; // Текстура кастомного курсора
        [SerializeField] private Vector2 cursorHotspot = new Vector2(16, 16); // Центр курсора

        [Header("Custom Crosshair")]
        [SerializeField] private GameObject crosshairUI; // Кастомный прицел
        [SerializeField] private RectTransform crosshairRect;
        [SerializeField] private Canvas uiCanvas;

        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            mainCamera = Camera.main;
        }

        private void Start()
        {
            // Проверяем что текстура курсора подключена (если используем кастомный курсор)
            if (customCursorTexture != null)
            {
                Debug.Log($"[CursorManager] ✅ Custom cursor texture loaded: {customCursorTexture.name}");
            }
            else if (crosshairUI != null)
            {
                Debug.Log("[CursorManager] ✅ Using UI crosshair");
            }
            else
            {
                Debug.LogWarning("[CursorManager] ⚠️ No cursor/crosshair configured! Will use system cursor.");
            }

            // Настраиваем курсор в зависимости от состояния игры
            UpdateCursorState();
        }

        private void OnEnable()
        {
            // Подписываемся на изменения состояния игры
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void Update()
        {
            // Обновляем позицию кастомного прицела
            if (crosshairUI != null && crosshairUI.activeSelf)
            {
                UpdateCrosshairPosition();
            }
        }

        /// <summary>
        /// Обновить позицию прицела под мышью
        /// </summary>
        private void UpdateCrosshairPosition()
        {
            if (crosshairRect == null) return;

            // УПРОЩЁННЫЙ МЕТОД: просто ставим прицел на позицию мыши
            crosshairRect.position = Input.mousePosition;
        }

        /// <summary>
        /// Изменение состояния игры
        /// </summary>
        private void OnGameStateChanged(GameState newState)
        {
            UpdateCursorState();
        }

        /// <summary>
        /// Обновить состояние курсора в зависимости от GameState
        /// </summary>
        private void UpdateCursorState()
        {
            var gameState = GameManager.Instance != null ? GameManager.Instance.CurrentState : GameState.MainMenu;

            switch (gameState)
            {
                case GameState.Playing:
                    // В игре: устанавливаем кастомный курсор или прицел
                    if (customCursorTexture != null && !hideSystemCursor)
                    {
                        // Используем системный курсор с кастомной текстурой
                        Cursor.SetCursor(customCursorTexture, cursorHotspot, CursorMode.Auto);
                        Cursor.visible = true;
                        SetCrosshairVisibility(false);
                    }
                    else
                    {
                        // Используем UI прицел
                        SetCursorVisibility(false);
                        SetCrosshairVisibility(true);
                    }
                    
                    if (confineCursor)
                        Cursor.lockState = CursorLockMode.Confined; // Запираем в окне
                    break;

                case GameState.Paused:
                case GameState.GameOver:
                case GameState.MainMenu:
                    // В меню: показываем обычный курсор
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); // Сбрасываем кастомный курсор
                    SetCursorVisibility(true);
                    SetCrosshairVisibility(false);
                    Cursor.lockState = CursorLockMode.None;
                    break;
            }
        }

        /// <summary>
        /// Показать/скрыть системный курсор
        /// </summary>
        private void SetCursorVisibility(bool visible)
        {
            if (hideSystemCursor)
            {
                Cursor.visible = visible;
            }
        }

        /// <summary>
        /// Показать/скрыть кастомный прицел
        /// </summary>
        private void SetCrosshairVisibility(bool visible)
        {
            if (crosshairUI != null)
            {
                crosshairUI.SetActive(visible);
            }
        }

        /// <summary>
        /// Установить спрайт прицела
        /// </summary>
        public void SetCrosshairSprite(Sprite sprite)
        {
            if (crosshairUI != null)
            {
                Image img = crosshairUI.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = sprite;
                }
            }
        }
    }
}