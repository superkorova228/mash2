using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RhythmHell.UI
{
    /// <summary>
    /// UI карточки перка. Показывает информацию о перке и обрабатывает клик.
    /// </summary>
    public class PerkCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image cardPanel; // Сам Panel карточки
        [SerializeField] private Outline cardOutline; // Outline для рамки
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button selectButton;

        private Progression.PerkData currentPerk;
        private PerkSelectionUI selectionUI;

        private void Awake()
        {
            // Подписываемся на кнопку
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectClicked);
            }
        }

        /// <summary>
        /// Настроить карточку перка
        /// </summary>
        public void Setup(Progression.PerkData perk, PerkSelectionUI selection)
        {
            currentPerk = perk;
            selectionUI = selection;

            if (perk == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Устанавливаем данные
            if (nameText != null)
                nameText.text = perk.perkName;

            if (descriptionText != null)
                descriptionText.text = perk.GetFormattedDescription();

            if (iconImage != null && perk.icon != null)
            {
                iconImage.sprite = perk.icon;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            // Цвет рамки по редкости
            if (cardOutline != null)
            {
                cardOutline.effectColor = perk.rarityColor;
            }

            // Фон карточки (слегка затемнённый)
            if (cardPanel != null)
            {
                Color bgColor = perk.rarityColor;
                bgColor.a = 0.2f; // Полупрозрачный
                cardPanel.color = bgColor;
            }
        }

        /// <summary>
        /// Клик по кнопке выбора
        /// </summary>
        private void OnSelectClicked()
        {
            // ЗВУК выбора перка
            if (Core.AudioManager.Instance != null)
            {
                Core.AudioManager.Instance.PlaySound2D(Core.SoundType.PerkSelect);
            }

            if (currentPerk != null && selectionUI != null)
            {
                selectionUI.OnPerkSelected(currentPerk);
            }
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(OnSelectClicked);
            }
        }
    }
}