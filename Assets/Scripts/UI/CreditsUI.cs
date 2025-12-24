using UnityEngine;
using UnityEngine.UI;
using mash2.Core;

namespace mash2.UI
{
    public class CreditsUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackClicked);
        }

        private void OnBackClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
        }
    }
}
