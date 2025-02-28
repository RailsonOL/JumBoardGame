using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image cardFrame; // Opcional: para colorir a borda/fundo da carta

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
            // Atualiza o sprite do ícone
            if (iconImage != null && cardData.icon != null)
            {
                iconImage.sprite = cardData.icon;

                // Aplica a cor de raridade à imagem do ícone
                // Como estamos usando UI.Image em vez de SpriteRenderer
                iconImage.color = cardData.GetRarityColor();
            }

            // Opcional: Aplicar a cor ao frame da carta também
            if (cardFrame != null)
            {
                // Você pode usar uma versão mais suave da cor para o frame
                Color frameColor = cardData.GetRarityColor();
                // Tornar a cor mais suave (menos saturada) para o frame
                frameColor.a = 0.3f; // Mais transparente
                cardFrame.color = frameColor;
            }

            // Atualiza os textos
            if (nameText != null)
                nameText.text = cardData.cardName;

            if (descriptionText != null)
                descriptionText.text = cardData.description;

            if (costText != null)
                costText.text = cardData.essenceCost.ToString();
        }
    }

    public Card GetCardData()
    {
        return cardData;
    }

    public int GetID()
    {
        return cardData.id;
    }

    public static int GetCardIDFromGameObject(GameObject cardGameObject)
    {
        // Verifica se o GameObject tem o componente CardController
        if (cardGameObject.TryGetComponent<CardController>(out var cardController))
        {
            // Retorna o ID da carta
            return cardController.GetID();
        }
        else
        {
            Debug.LogError("CardController não encontrado no GameObject da carta.");
            return -1; // Retorna um valor inválido para indicar erro
        }
    }

    public void SetCardData(Card newCardData)
    {
        cardData = newCardData;
        UpdateCardUI();
    }
}