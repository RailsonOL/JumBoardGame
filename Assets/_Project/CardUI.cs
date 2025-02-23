// CardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Card Data")]
    [SerializeField] private Card cardData;

    private void OnEnable()
    {
        UpdateCardUI();
    }

    public void UpdateCardUI()
    {
        if (cardData != null)
        {
            if (iconImage != null && cardData.icon != null)
            {
                iconImage.sprite = cardData.icon;
            }

            if (nameText != null)
                nameText.text = cardData.cardName;

            if (descriptionText != null)
                descriptionText.text = cardData.description;

            if (costText != null)
                costText.text = cardData.essenceCost.ToString();
        }
    }

    public void SetCardData(Card newCardData)
    {
        cardData = newCardData;
        UpdateCardUI();
    }
}