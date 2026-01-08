using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace RhythmHell.Core
{
    /// <summary>
    /// Менеджер загрузки сцен. Поддерживает асинхронную загрузку с экраном загрузки.
    /// Singleton - доступен из любой точки игры.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float minimumLoadTime = 1f; // Минимальное время показа экрана загрузки

        // События для обновления UI загрузки
        public event Action<float> OnLoadProgress; // 0.0 - 1.0
        public event Action OnLoadComplete;

        private bool isLoading = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Загрузить сцену по имени
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isLoading)
            {
                Debug.LogWarning("[SceneLoader] Already loading a scene!");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        /// <summary>
        /// Загрузить сцену по индексу в Build Settings
        /// </summary>
        public void LoadScene(int sceneIndex)
        {
            if (isLoading)
            {
                Debug.LogWarning("[SceneLoader] Already loading a scene!");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        /// <summary>
        /// Быстрая загрузка без экрана загрузки (для меню)
        /// </summary>
        public void LoadSceneImmediate(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            isLoading = true;
            float startTime = Time.time;

            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");

            // Меняем состояние игры
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeGameState(GameState.Loading);
            }

            // ИСПРАВЛЕНИЕ: Загружаем LoadingScreen только если мы НЕ в LoadingScreen и целевая сцена НЕ LoadingScreen
            string currentScene = SceneManager.GetActiveScene().name;
            bool needLoadingScreen = (currentScene != "LoadingScreen" && sceneName != "LoadingScreen" && sceneName != "MainMenu");

            if (needLoadingScreen)
            {
                SceneManager.LoadScene("LoadingScreen");
                yield return new WaitForSeconds(0.1f);
            }

            // Начинаем асинхронную загрузку целевой сцены
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Ждём загрузки
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnLoadProgress?.Invoke(progress);

                if (asyncLoad.progress >= 0.9f)
                {
                    // Только если показывали экран загрузки
                    if (needLoadingScreen)
                    {
                        float elapsedTime = Time.time - startTime;
                        if (elapsedTime < minimumLoadTime)
                        {
                            yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
                        }
                    }

                    OnLoadProgress?.Invoke(1f);
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            OnLoadComplete?.Invoke();
            isLoading = false;

            Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
        }

        private IEnumerator LoadSceneAsync(int sceneIndex)
        {
            string sceneName = SceneManager.GetSceneByBuildIndex(sceneIndex).name;
            yield return LoadSceneAsync(sceneName);
        }

        /// <summary>
        /// Перезагрузить текущую сцену
        /// </summary>
        public void ReloadCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }
    }
}