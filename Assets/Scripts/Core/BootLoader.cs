using UnityEngine;
using mash2.Core;

namespace mash2.Core
{
    public class BootLoader : MonoBehaviour
    {
        [Header("Scene to Load After Boot")]
        [SerializeField] private int mainMenuSceneIndex = 1;
        
        [Header("Boot Delay")]
        [SerializeField] private float bootDelay = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private void Awake()
        {
            if (verboseLogging)
                Debug.Log("=== BOOT: BootLoader Awake ===");
        }

        private void Start()
        {
            if (verboseLogging)
                Debug.Log("=== BOOT: Game Starting ===");
            
            CheckCriticalSystems();
            
            Invoke(nameof(LoadMainMenu), bootDelay);
        }

        private void CheckCriticalSystems()
        {
            if (SceneLoader.Instance == null)
                Debug.LogError("CRITICAL: SceneLoader.Instance is NULL!");
            else if (verboseLogging)
                Debug.Log("✓ SceneLoader found");
            
            if (GameManager.Instance == null)
                Debug.LogError("CRITICAL: GameManager.Instance is NULL!");
            else if (verboseLogging)
                Debug.Log("✓ GameManager found");
            
            if (SettingsManager.Instance == null)
                Debug.LogError("CRITICAL: SettingsManager.Instance is NULL!");
            else if (verboseLogging)
                Debug.Log("✓ SettingsManager found");
            
            if (mash2.Audio.AudioManager.Instance == null)
                Debug.LogWarning("WARNING: AudioManager.Instance is NULL!");
            else if (verboseLogging)
                Debug.Log("✓ AudioManager found");
        }

        private void LoadMainMenu()
        {
            if (verboseLogging)
                Debug.Log("BOOT: Loading Main Menu...");
            
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(mainMenuSceneIndex);
            }
            else
            {
                Debug.LogError("CRITICAL: Cannot load MainMenu - SceneLoader is null!");
            }
        }
    }
}