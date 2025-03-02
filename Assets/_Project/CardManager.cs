using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [SerializeField] private List<Card> allCards; // Lista de todas as cartas disponíveis no jogo

    // Propriedade pública para acessar a lista de cartas
    public List<Card> AllCards => allCards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Card GetCardById(int cardId)
    {
        foreach (var card in allCards)
        {
            if (card.id == cardId)
            {
                return card;
            }
        }
        return null; // Retorna null se não encontrar a carta com o ID especificado
    }

    // Retorna o ID da carta
    public int GetCardId(Card card)
    {
        return card.id;
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
}