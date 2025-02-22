// CardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class CardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Card Data")]
    [SerializeField] private Card cardData;

    // Função que atualiza a UI com os dados da carta
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

    // Função para definir nova carta
    public void SetCardData(Card newCardData)
    {
        cardData = newCardData;
        UpdateCardUI();
    }

    // Para permitir atualização no editor
    // private void OnValidate()
    // {
    //     UpdateCardUI();
    // }
}

#if UNITY_EDITOR
// Editor customizado para o CardUI
[CustomEditor(typeof(CardUI))]
public class CardUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CardUI cardUI = (CardUI)target;

        // Desenha o inspector padrão
        DrawDefaultInspector();

        // Adiciona um botão para atualizar a UI
        EditorGUILayout.Space();
        if (GUILayout.Button("Update Card UI"))
        {
            cardUI.UpdateCardUI();
            // Marca a cena como suja para garantir que as mudanças sejam salvas
            EditorUtility.SetDirty(cardUI.gameObject);
        }
    }
}
#endif