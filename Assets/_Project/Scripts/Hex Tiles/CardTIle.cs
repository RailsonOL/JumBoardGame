using UnityEngine;
using System.Collections.Generic;
using TMPro; // Necessário para TextMeshPro

public class CardTile : HexTile
{
    [SerializeField] private List<Card> cards; // Lista de cartas

    private TextMeshPro tileLabel; // Referência ao texto 3D

    private void Start()
    {
        CreateTileLabel(); // Cria o texto ao inicializar o tile
    }

    //TODO: Criar prefabs com o labels
    private void CreateTileLabel()
    {
        // Cria um novo GameObject para o texto 3D
        GameObject textObject = new GameObject("CardTileLabel");
        textObject.transform.SetParent(transform, false); // Faz o texto ser filho do CardTile

        // Adiciona o componente TextMeshPro (versão 3D)
        tileLabel = textObject.AddComponent<TextMeshPro>();
        tileLabel.text = "Card Tile"; // Define o texto
        tileLabel.fontSize = 2f; // Tamanho da fonte no mundo 3D
        tileLabel.alignment = TextAlignmentOptions.Center; // Centraliza o texto
        tileLabel.color = Color.black; // Define a cor como verde

        // Posiciona o texto acima do tile no espaço 3D
        textObject.transform.localPosition = Vector3.up * 0.2f; // 2 unidades acima do tile

        // Define a rotação como (90, 0, 0) em graus
        textObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Ajusta a escala para ser visível no mundo
        textObject.transform.localScale = Vector3.one; // Escala padrão
    }

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

    private void OnDestroy()
    {
        // Garante que o texto seja destruído junto com o tile
        if (tileLabel != null)
        {
            Destroy(tileLabel.gameObject);
        }
    }
}