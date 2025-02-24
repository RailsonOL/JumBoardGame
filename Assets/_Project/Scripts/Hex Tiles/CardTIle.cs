using UnityEngine;
using System.Collections.Generic;

public class CardTile : HexTile
{
    [SerializeField] private List<Card> cards; // Lista de cartas

    public override void ExecuteTileEffect(Idol idol)
    {
        if (cards != null && cards.Count > 0 && idol.playerOwner != null)
        {
            Card randomCard = cards[Random.Range(0, cards.Count)]; // Escolhe uma carta aleatória
            idol.playerOwner.ReceiveCard(randomCard); // Dá a carta ao jogador
            Debug.Log($"Carta dada ao jogador: {randomCard.cardName}");
        }
    }
}