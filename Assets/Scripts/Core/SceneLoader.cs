using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using mash2.Data;

namespace mash2.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        
        [Header("Loading UI References")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private UnityEngine.UI.Slider progressBar;
        [SerializeField] private TMPro.TextMeshProUGUI loadingText;
        
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        
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
            
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }

        public void LoadScene(SceneData sceneData)
        {
            if (sceneData == null)
            {
                Debug.LogError("SceneData is null!");
                return;
            }
            
            LoadSceneAsync(sceneData.sceneName, sceneData);
        }
        
        public void LoadScene(string sceneName)
        {
            LoadSceneAsync(sceneName, null);
        }
        
        public void LoadScene(int sceneIndex)
        {
            LoadSceneAsync(sceneIndex, null);
        }

        private void LoadSceneAsync(string sceneName, SceneData sceneData)
        {
            if (isLoading)
            {
                Debug.LogWarning("Scene is already loading!");
                return;
            }
            
            StartCoroutine(LoadSceneCoroutine(sceneName, sceneData));
        }
        
        private void LoadSceneAsync(int sceneIndex, SceneData sceneData)
        {
            if (isLoading)
            {
                Debug.LogWarning("Scene is already loading!");
                return;
            }
            
            StartCoroutine(LoadSceneCoroutine(sceneIndex, sceneData));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, SceneData sceneData)
        {
            isLoading = true;
            
            bool showLoading = sceneData?.showLoadingScreen ?? true;
            if (showLoading && loadingScreen != null)
                loadingScreen.SetActive(true);
            
            OnSceneLoadStarted?.Invoke(sceneName);
            
            if (sceneData != null && sceneData.fadeOutMusic)
            {
                // TODO: AudioManager fade
            }
            
            yield return new WaitForSeconds(0.1f);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            float minimumTime = sceneData?.minimumLoadTime ?? 0.5f;
            float timer = 0f;
            
            while (!asyncLoad.isDone)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (progressBar != null)
                    progressBar.value = progress;
                
                if (loadingText != null)
                    loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
                
                if (asyncLoad.progress >= 0.9f && timer >= minimumTime)
                {
                    if (progressBar != null)
                        progressBar.value = 1f;
                    if (loadingText != null)
                        loadingText.text = "Loading... 100%";
                    
                    yield return new WaitForSeconds(0.2f);
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            if (showLoading && loadingScreen != null)
                loadingScreen.SetActive(false);
            
            OnSceneLoadCompleted?.Invoke(sceneName);
            
            isLoading = false;
        }
        
        private IEnumerator LoadSceneCoroutine(int sceneIndex, SceneData sceneData)
        {
            isLoading = true;
            
            bool showLoading = sceneData?.showLoadingScreen ?? true;
            if (showLoading && loadingScreen != null)
                loadingScreen.SetActive(true);
            
            OnSceneLoadStarted?.Invoke($"Scene {sceneIndex}");
            
            if (sceneData != null && sceneData.fadeOutMusic)
            {
                // TODO: AudioManager fade
            }
            
            yield return new WaitForSeconds(0.1f);
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            asyncLoad.allowSceneActivation = false;
            
            float minimumTime = sceneData?.minimumLoadTime ?? 0.5f;
            float timer = 0f;
            
            while (!asyncLoad.isDone)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                
                if (progressBar != null)
                    progressBar.value = progress;
                if (loadingText != null)
                    loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
                
                if (asyncLoad.progress >= 0.9f && timer >= minimumTime)
                {
                    if (progressBar != null)
                        progressBar.value = 1f;
                    if (loadingText != null)
                        loadingText.text = "Loading... 100%";
                    
                    yield return new WaitForSeconds(0.2f);
                    asyncLoad.allowSceneActivation = true;
                }
                
                yield return null;
            }
            
            if (showLoading && loadingScreen != null)
                loadingScreen.SetActive(false);
            
            OnSceneLoadCompleted?.Invoke($"Scene {sceneIndex}");
            
            isLoading = false;
        }
        
        public void ReloadCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            LoadScene(currentScene.name);
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}