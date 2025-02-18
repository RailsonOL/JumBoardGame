using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Hex Tiles")]
    public List<HexTile> hexTiles; // Lista de hexágonos na ordem definida no Inspector
    public HexTile startTile; // Hexágono inicial
    public HexTile endTile; // Hexágono final

    private void Awake()
    {
        if (startTile == null || endTile == null)
        {
            Debug.LogError("StartTile ou EndTile não foram definidos no BoardManager.");
        }

        ConfigureHexConnections();
    }

    public void GenerateHexConnections() // Método que será chamado pelo botão
    {
        ConfigureHexConnections();
        Debug.Log("HexTiles configurados!");
    }

    private void ConfigureHexConnections()
    {
        for (int i = 0; i < hexTiles.Count; i++)
        {
            HexTile current = hexTiles[i];

            // Define o próximo tile se existir
            if (i < hexTiles.Count - 1)
            {
                current.SetNextHex(hexTiles[i + 1]);
            }

            // Define o tile anterior se não for o primeiro
            if (i > 0)
            {
                current.SetPreviousHex(hexTiles[i - 1]);
            }
        }
    }
}
