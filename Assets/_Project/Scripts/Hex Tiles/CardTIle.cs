using UnityEngine;
using System.Collections.Generic;

public class CardTile : HexTile
{
    [SerializeField] private List<Card> cards; // Lista de cartas

    public override void ExecuteTileEffect(Essent essent)
    {
        Debug.Log("ExecuteTileEffect called");
        Debug.Log($"Player Owner: {essent.playerOwner}");

        if (cards != null && cards.Count > 0 && essent.playerOwner != null)
        {
            Card randomCard = cards[Random.Range(0, cards.Count)]; // Escolhe uma carta aleatória

            // Obtém o ID do jogador
            int playerId = essent.playerOwner.PlayerIdNumber;

            // Envia a carta para o jogador usando o GameController
            GameController.Instance.SendCardToPlayer(playerId, randomCard.id);

            Debug.Log($"Carta enviada ao jogador: {randomCard.cardName}");
        }
    }
}