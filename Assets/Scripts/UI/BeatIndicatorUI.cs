using UnityEngine;
using UnityEngine.UI;

namespace RhythmHell.UI
{
    /// <summary>
    /// Визуальный индикатор ритма. Пульсирует в такт музыке.
    /// Помогает игроку "почувствовать" бит.
    /// </summary>
    public class BeatIndicatorUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image indicatorImage;
        
        [Header("Animation")]
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float beatScale = 1.8f; // Увеличено!
        [SerializeField] private float animationSpeed = 15f; // Быстрее возврат

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f); // Полупрозрачный белый
        [SerializeField] private Color beatColor = Color.yellow; // Яркий жёлтый

        private float targetScale = 1f;
        private Color targetColor;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            targetColor = normalColor;
        }

        private void OnEnable()
        {
            // Подписываемся на биты
            if (Rhythm.BeatManager.Instance != null)
            {
                Rhythm.BeatManager.Instance.OnBeat += OnBeat;
            }
        }

        private void OnDisable()
        {
            // Отписываемся
            if (Rhythm.BeatManager.Instance != null)
            {
                Rhythm.BeatManager.Instance.OnBeat -= OnBeat;
            }
        }

        private void Update()
        {
            // Плавная анимация к целевому масштабу
            float currentScale = rectTransform.localScale.x;
            float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * animationSpeed);
            rectTransform.localScale = Vector3.one * newScale;

            // Плавная анимация цвета
            if (indicatorImage != null)
            {
                indicatorImage.color = Color.Lerp(indicatorImage.color, targetColor, Time.deltaTime * animationSpeed);
            }

            // Возвращаемся к нормальному состоянию
            if (targetScale > normalScale)
            {
                targetScale = normalScale;
                targetColor = normalColor;
            }
        }

        /// <summary>
        /// Реакция на бит
        /// </summary>
        private void OnBeat()
        {
            // Резко увеличиваемся
            targetScale = beatScale;
            targetColor = beatColor;
        }
    }
}