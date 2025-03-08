using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [SerializeField] private List<Card> allCards; // Lista de todas as cartas disponíveis no jogo

    // Propriedade pública para acessar a lista de cartas
    public List<Card> AllCards => allCards;
    public int previousListCount = 0;

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

    private void OnValidate()
    {
        // Verifica se o tamanho da lista mudou
        if (AllCards != null && AllCards.Count != previousListCount)
        {
            GenerateCardIds();
            previousListCount = AllCards.Count; // Atualiza o tamanho anterior
        }
    }

    public void GenerateCardIds()
    {
        if (AllCards == null || AllCards.Count == 0)
        {
            Debug.LogWarning("A lista de cartas está vazia. Adicione cartas ao CardManager.");
            return;
        }

        // Gera IDs apenas para as novas cartas
        for (int i = previousListCount; i < AllCards.Count; i++)
        {
            if (AllCards[i].id == 0) // Verifica se o ID ainda não foi atribuído
            {
                AllCards[i].id = i + 1; // IDs começam em 1
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(AllCards[i]); // Marca o ScriptableObject como modificado
#endif
            }
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log("IDs das cartas gerados com sucesso!");
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