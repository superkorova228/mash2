using UnityEngine;
using UnityEngine.UI;

namespace RhythmHell.UI
{
    /// <summary>
    /// Управление главным меню. Обрабатывает клики по кнопкам.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button exitButton;

        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel; // Создадим позже

        private void Start()
        {
            // Устанавливаем состояние игры
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.ChangeGameState(Core.GameState.MainMenu);
            }

            // Подписываемся на клики кнопок
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);

            // Показываем главное меню, скрываем остальное
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            // Отписываемся от событий при уничтожении
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (creditsButton != null)
                creditsButton.onClick.RemoveListener(OnCreditsClicked);

            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClicked);
        }

        /// <summary>
        /// Кнопка PLAY - загрузить Gameplay
        /// </summary>
        private void OnPlayClicked()
        {
            Debug.Log("[MainMenu] Play clicked");
            
            if (Core.SceneLoader.Instance != null)
            {
                // Сбрасываем данные игры перед началом
                if (Core.GameManager.Instance != null)
                {
                    Core.GameManager.Instance.ResetGameData();
                }

                Core.SceneLoader.Instance.LoadScene("Gameplay");
            }
        }

        /// <summary>
        /// Кнопка SETTINGS - открыть панель настроек
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings clicked");
            ShowSettings();
        }

        /// <summary>
        /// Кнопка CREDITS - загрузить сцену титров
        /// </summary>
        private void OnCreditsClicked()
        {
            Debug.Log("[MainMenu] Credits clicked");
            
            if (Core.SceneLoader.Instance != null)
            {
                Core.SceneLoader.Instance.LoadSceneImmediate("Credits");
            }
        }

        /// <summary>
        /// Кнопка EXIT - выход из игры
        /// </summary>
        private void OnExitClicked()
        {
            Debug.Log("[MainMenu] Exit clicked");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Показать главное меню
        /// </summary>
        public void ShowMainMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        /// <summary>
        /// Показать настройки
        /// </summary>
        public void ShowSettings()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }
    }
}