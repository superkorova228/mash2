using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RhythmHell.UI
{
    /// <summary>
    /// UI для отображения XP и уровня игрока.
    /// </summary>
    public class XPBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform xpFillRect; // Изменено на RectTransform
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;

        [Header("Animation")]
        [SerializeField] private float fillSpeed = 5f;
        [SerializeField] private bool animateFill = true;

        private float targetFillAmount = 0f;
        private float currentFillAmount = 0f;
        private float maxWidth; // Максимальная ширина бара

        private void OnEnable()
        {
            // Ждём один кадр чтобы UI успел инициализироваться
            StartCoroutine(InitializeAfterFrame());
        }

        private System.Collections.IEnumerator InitializeAfterFrame()
        {
            yield return null; // Ждём один кадр

            // Получаем максимальную ширину от родителя
            if (xpFillRect != null && xpFillRect.parent != null)
            {
                RectTransform parentRect = xpFillRect.parent.GetComponent<RectTransform>();
                if (parentRect != null)
                {
                    maxWidth = parentRect.rect.width;
                    Debug.Log($"[XPBarUI] Max width set to: {maxWidth}");
                }
                else
                {
                    Debug.LogError("[XPBarUI] Parent RectTransform not found!");
                }
            }

            // Подписываемся на события ExperienceManager
            if (Progression.ExperienceManager.Instance != null)
            {
                Progression.ExperienceManager.Instance.OnXPProgressChanged += UpdateXPBar;
                Progression.ExperienceManager.Instance.OnLevelUp += OnLevelUp;
                Progression.ExperienceManager.Instance.OnXPChanged += UpdateXPText;

                // Инициализация начальных значений
                UpdateDisplay();
            }
            else
            {
                Debug.LogError("[XPBarUI] ExperienceManager.Instance is NULL!");
            }
        }

        private void OnDisable()
        {
            // Отписываемся
            if (Progression.ExperienceManager.Instance != null)
            {
                Progression.ExperienceManager.Instance.OnXPProgressChanged -= UpdateXPBar;
                Progression.ExperienceManager.Instance.OnLevelUp -= OnLevelUp;
                Progression.ExperienceManager.Instance.OnXPChanged -= UpdateXPText;
            }
        }

        private void Update()
        {
            // Плавная анимация заполнения бара (изменяем ширину)
            if (animateFill && xpFillRect != null && maxWidth > 0)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * fillSpeed);
                
                // Устанавливаем ширину
                float newWidth = currentFillAmount * maxWidth;
                xpFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            }
        }

        /// <summary>
        /// Обновить XP бар (прогресс 0.0 - 1.0)
        /// </summary>
        private void UpdateXPBar(float progress)
        {
            targetFillAmount = progress;

            if (!animateFill && xpFillRect != null)
            {
                xpFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress * maxWidth);
            }
        }

        /// <summary>
        /// Обновить текст XP
        /// </summary>
        private void UpdateXPText(int currentXP)
        {
            if (xpText != null && Progression.ExperienceManager.Instance != null)
            {
                int xpToNext = Progression.ExperienceManager.Instance.XPToNextLevel;
                xpText.text = $"{currentXP} / {xpToNext}";
            }
        }

        /// <summary>
        /// Левел-ап произошёл
        /// </summary>
        private void OnLevelUp(int newLevel, int xpToNext)
        {
            // Обновляем текст уровня
            if (levelText != null)
            {
                levelText.text = $"LVL {newLevel}";
            }

            // Обновляем текст XP
            if (xpText != null && Progression.ExperienceManager.Instance != null)
            {
                int currentXP = Progression.ExperienceManager.Instance.CurrentXP;
                xpText.text = $"{currentXP} / {xpToNext}";
            }

            // Сбрасываем анимацию бара (новый уровень = начинаем с 0)
            currentFillAmount = 0f;
            if (xpFillRect != null)
            {
                xpFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            }

            // Визуальный эффект левел-апа
            StartCoroutine(LevelUpFlash());
        }

        /// <summary>
        /// Обновить весь дисплей
        /// </summary>
        private void UpdateDisplay()
        {
            if (Progression.ExperienceManager.Instance == null) return;

            var xpManager = Progression.ExperienceManager.Instance;

            // Уровень
            if (levelText != null)
            {
                levelText.text = $"LVL {xpManager.CurrentLevel}";
            }

            // XP текст
            if (xpText != null)
            {
                xpText.text = $"{xpManager.CurrentXP} / {xpManager.XPToNextLevel}";
            }

            // XP бар
            float progress = xpManager.ProgressToNextLevel;
            targetFillAmount = progress;
            currentFillAmount = progress;
            if (xpFillRect != null)
            {
                xpFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress * maxWidth);
            }
        }

        /// <summary>
        /// Вспышка при левел-апе
        /// </summary>
        private System.Collections.IEnumerator LevelUpFlash()
        {
            if (xpFillRect == null) yield break;

            Image fillImage = xpFillRect.GetComponent<Image>();
            if (fillImage == null) yield break;

            Color originalColor = fillImage.color;
            fillImage.color = Color.yellow;

            yield return new WaitForSeconds(0.2f);

            fillImage.color = originalColor;
        }
    }
}