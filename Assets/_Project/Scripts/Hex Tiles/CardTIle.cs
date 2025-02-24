using UnityEngine;

public class CardTile : HexTile
{
    [SerializeField] private Card card;

    public override void ExecuteTileEffect(Idol idol)
    {
        if (card != null && idol.playerOwner != null)
        {
            //idol.playerOwner.ReceiveCard(card);
            Debug.Log($"Carta dada ao jogador: {card.cardName}");
        }
    }
}