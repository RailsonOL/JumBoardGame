using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Hex Tiles")]
    public List<HexTile> hexTiles; // Lista de todos os hexágonos no tabuleiro
    public HexTile startTile; // Hexágono inicial
    public HexTile endTile; // Hexágono final

    private void Awake()
    {
        // Verifica se o tabuleiro está configurado corretamente
        if (startTile == null || endTile == null)
        {
            Debug.LogError("StartTile ou EndTile não foram definidos no BoardManager.");
        }
    }

    // Retorna o hexágono inicial
    public HexTile GetStartTile()
    {
        return startTile;
    }

    // Retorna o hexágono final
    public HexTile GetEndTile()
    {
        return endTile;
    }

    // Verifica se um hexágono é o final
    public bool IsEndTile(HexTile tile)
    {
        return tile == endTile;
    }
}