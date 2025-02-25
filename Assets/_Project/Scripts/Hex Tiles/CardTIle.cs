using UnityEngine;
using System.Collections.Generic;

public class CardTile : HexTile
{
    [SerializeField] private List<Card> cards; // Lista de cartas

    public override void ExecuteTileEffect(Essent essent)
    {
        if (cards != null && cards.Count > 0 && essent.playerOwner != null)
        {
            Card randomCard = cards[Random.Range(0, cards.Count)]; // Escolhe uma carta aleatória
            essent.playerOwner.ReceiveCard(randomCard); // Dá a carta ao jogador
            Debug.Log($"Carta dada ao jogador: {randomCard.cardName}");
        }
    }
}