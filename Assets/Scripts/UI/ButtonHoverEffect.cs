using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace mash2.UI
{
    /// <summary>
    /// Hover эффект для кнопок: масштаб + стрелочки
    /// </summary>
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Scale Effect")]
        [SerializeField] private bool enableScale = true;
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float animationDuration = 0.15f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Arrow Indicators")]
        [SerializeField] private bool showArrows = true;
        [SerializeField] private GameObject leftArrow;
        [SerializeField] private GameObject rightArrow;
        [SerializeField] private float arrowOffset = 20f;
        [SerializeField] private string leftArrowSymbol = "►"; // ТВОЙ СИМВОЛ
        [SerializeField] private string rightArrowSymbol = "◄"; // ТВОЙ СИМВОЛ
        [SerializeField] private int arrowFontSize = 36;

        [Header("Glow Effect")]
        [SerializeField] private bool enableGlow = false;
        [SerializeField] private Image glowImage;
        [SerializeField] private Color glowColor = new Color(1f, 1f, 1f, 0.3f);

        [Header("Sound")]
        [SerializeField] private bool playSoundOnHover = true;

        private Vector3 originalScale;
        private Button button;
        private Coroutine scaleCoroutine;
        private Coroutine glowCoroutine;
        private bool isHovered = false;

        private void Awake()
        {
            originalScale = transform.localScale;
            button = GetComponent<Button>();

            // Создаём стрелочки автоматически, если не назначены
            if (showArrows && leftArrow == null)
            {
                CreateArrows();
            }

            // Скрываем стрелочки по умолчанию
            if (leftArrow != null)
                leftArrow.SetActive(false);
            if (rightArrow != null)
                rightArrow.SetActive(false);

            // Скрываем glow
            if (glowImage != null)
            {
                Color c = glowColor;
                c.a = 0f;
                glowImage.color = c;
            }
        }

        /// <summary>
        /// Создаёт стрелочки автоматически
        /// </summary>
        private void CreateArrows()
        {
            // Левая стрелочка
            leftArrow = new GameObject("LeftArrow");
            leftArrow.transform.SetParent(transform, false);
    
            var leftText = leftArrow.AddComponent<TMPro.TextMeshProUGUI>();
            leftText.text = leftArrowSymbol; // ← ИСПОЛЬЗУЕМ НАСТРОЙКУ
            leftText.fontSize = arrowFontSize;
            leftText.color = Color.white;
            leftText.alignment = TMPro.TextAlignmentOptions.Center;
    
            RectTransform leftRect = leftArrow.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0.5f);
            leftRect.anchorMax = new Vector2(0, 0.5f);
            leftRect.pivot = new Vector2(1, 0.5f);
            leftRect.anchoredPosition = new Vector2(-arrowOffset, 0);
            leftRect.sizeDelta = new Vector2(40, 40);

            // Правая стрелочка
            rightArrow = new GameObject("RightArrow");
            rightArrow.transform.SetParent(transform, false);
    
            var rightText = rightArrow.AddComponent<TMPro.TextMeshProUGUI>();
            rightText.text = rightArrowSymbol; // ← ИСПОЛЬЗУЕМ НАСТРОЙКУ
            rightText.fontSize = arrowFontSize;
            rightText.color = Color.white;
            rightText.alignment = TMPro.TextAlignmentOptions.Center;
    
            RectTransform rightRect = rightArrow.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(1, 0.5f);
            rightRect.anchorMax = new Vector2(1, 0.5f);
            rightRect.pivot = new Vector2(0, 0.5f);
            rightRect.anchoredPosition = new Vector2(arrowOffset, 0);
            rightRect.sizeDelta = new Vector2(40, 40);
        }

        // Вызывается при наведении мыши
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && !button.interactable)
                return;

            OnHoverStart();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverEnd();
        }

        // Вызывается при выборе кнопки (навигация клавиатурой/геймпадом)
        public void OnSelect(BaseEventData eventData)
        {
            OnHoverStart();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            OnHoverEnd();
        }

        private void OnHoverStart()
        {
            if (isHovered)
                return;

            isHovered = true;

            // Звук
            if (playSoundOnHover)
                PlayHoverSound();

            // Масштаб
            if (enableScale)
            {
                if (scaleCoroutine != null)
                    StopCoroutine(scaleCoroutine);
                scaleCoroutine = StartCoroutine(AnimateScale(originalScale * hoverScale));
            }

            // Стрелочки
            if (showArrows)
            {
                if (leftArrow != null)
                {
                    leftArrow.SetActive(true);
                    StartCoroutine(AnimateArrowScale(leftArrow, true));
                }

                if (rightArrow != null)
                {
                    rightArrow.SetActive(true);
                    StartCoroutine(AnimateArrowScale(rightArrow, true));
                }
            }

            // Glow
            if (enableGlow && glowImage != null)
            {
                if (glowCoroutine != null)
                    StopCoroutine(glowCoroutine);
                glowCoroutine = StartCoroutine(AnimateGlow(glowColor));
            }
        }

        private void OnHoverEnd()
        {
            if (!isHovered)
                return;

            isHovered = false;

            // Масштаб
            if (enableScale)
            {
                if (scaleCoroutine != null)
                    StopCoroutine(scaleCoroutine);
                scaleCoroutine = StartCoroutine(AnimateScale(originalScale));
            }

            // Стрелочки
            if (showArrows)
            {
                if (leftArrow != null)
                    StartCoroutine(AnimateArrowScale(leftArrow, false));

                if (rightArrow != null)
                    StartCoroutine(AnimateArrowScale(rightArrow, false));
            }

            // Glow
            if (enableGlow && glowImage != null)
            {
                Color transparent = glowColor;
                transparent.a = 0f;
                
                if (glowCoroutine != null)
                    StopCoroutine(glowCoroutine);
                glowCoroutine = StartCoroutine(AnimateGlow(transparent));
            }
        }

        private IEnumerator AnimateScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Работает даже на паузе
                float t = elapsed / animationDuration;
                float curveValue = scaleCurve.Evaluate(t);
                
                transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        private IEnumerator AnimateArrowScale(GameObject arrow, bool show)
        {
            Vector3 startScale = arrow.transform.localScale;
            Vector3 targetScale = show ? Vector3.one : Vector3.zero;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / animationDuration;
                
                arrow.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            arrow.transform.localScale = targetScale;
            
            if (!show)
                arrow.SetActive(false);
        }

        private IEnumerator AnimateGlow(Color targetColor)
        {
            Color startColor = glowImage.color;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / animationDuration;
                
                glowImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            glowImage.color = targetColor;
        }

        private void PlayHoverSound()
        {
            if (mash2.Audio.AudioManager.Instance != null)
            {
                // AudioManager.Instance.PlayUIHoverSound();
                // Добавим позже
            }
        }

        private void OnDisable()
        {
            // Сбрасываем при отключении
            transform.localScale = originalScale;
            
            if (leftArrow != null)
                leftArrow.SetActive(false);
            if (rightArrow != null)
                rightArrow.SetActive(false);

            if (glowImage != null)
            {
                Color c = glowColor;
                c.a = 0f;
                glowImage.color = c;
            }

            isHovered = false;
        }
    }
}