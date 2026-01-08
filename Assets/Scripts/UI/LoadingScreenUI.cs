using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RhythmHell.UI
{
    /// <summary>
    /// Управление UI экрана загрузки. Отображает прогресс загрузки сцены.
    /// </summary>
    public class LoadingScreenUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Settings")]
        [SerializeField] private bool animateText = true;
        [SerializeField] private float textAnimSpeed = 0.5f;

        private string baseLoadingText = "LOADING";
        private float textAnimTimer = 0f;
        private int dotCount = 0;

        private void OnEnable()
        {
            // Подписываемся на события SceneLoader
            if (Core.SceneLoader.Instance != null)
            {
                Core.SceneLoader.Instance.OnLoadProgress += UpdateProgress;
                Core.SceneLoader.Instance.OnLoadComplete += OnLoadComplete;
            }

            // Сброс значений
            if (progressBar != null)
                progressBar.value = 0f;
        }

        private void OnDisable()
        {
            // Отписываемся от событий
            if (Core.SceneLoader.Instance != null)
            {
                Core.SceneLoader.Instance.OnLoadProgress -= UpdateProgress;
                Core.SceneLoader.Instance.OnLoadComplete -= OnLoadComplete;
            }
        }

        private void Update()
        {
            // Анимация текста "LOADING..."
            if (animateText && loadingText != null)
            {
                textAnimTimer += Time.deltaTime;
                
                if (textAnimTimer >= textAnimSpeed)
                {
                    textAnimTimer = 0f;
                    dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3
                    loadingText.text = baseLoadingText + new string('.', dotCount);
                }
            }
        }

        /// <summary>
        /// Обновить прогресс-бар (вызывается SceneLoader)
        /// </summary>
        private void UpdateProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.value = progress;
            }
        }

        /// <summary>
        /// Загрузка завершена (вызывается SceneLoader)
        /// </summary>
        private void OnLoadComplete()
        {
            Debug.Log("[LoadingScreen] Load complete!");
        }
    }
}