using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class BoardManager : MonoBehaviour
{
    [Header("Hex Tiles")]
    public List<HexTile> hexTiles; // Lista de hexágonos na ordem definida no Inspector
    public HexTile startTile; // Hexágono inicial

    private void Start()
    {
        Debug.Log($"Jogo iniciou! HexTiles: {hexTiles.Count}");
        ConfigureHexConnections(); // Força a configuração ao iniciar
    }

    public void GenerateHexConnections() // Método que será chamado pelo botão
    {
        ConfigureHexConnections();
        Debug.Log("HexTiles configurados!");
    }

    private void ConfigureHexConnections()
    {
        if (hexTiles == null || hexTiles.Count == 0)
        {
            Debug.LogError("Lista de HexTiles está vazia!");
            return;
        }

        for (int i = 0; i < hexTiles.Count; i++)
        {
            HexTile current = hexTiles[i];

            // Define o índice do tile
            current.SetTileIndex(i);

            // Define o próximo tile (se não for o último, aponta para o próximo na lista)
            if (i < hexTiles.Count - 1)
            {
                current.SetNextHex(hexTiles[i + 1]);
            }
            else
            {
                // Último tile aponta de volta para o primeiro (loop)
                current.SetNextHex(hexTiles[0]);
            }

            // Define o tile anterior (se não for o primeiro, aponta para o anterior na lista)
            if (i > 0)
            {
                current.SetPreviousHex(hexTiles[i - 1]);
            }
            else
            {
                // Primeiro tile aponta para o último (loop)
                current.SetPreviousHex(hexTiles[hexTiles.Count - 1]);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoardManager))]
public class BoardManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardManager boardManager = (BoardManager)target;

        if (Application.isPlaying)
        {
            return; // Evita chamadas no Play Mode
        }

        if (GUILayout.Button("Gerar Conexões HexTiles"))
        {
            boardManager.GenerateHexConnections();
            EditorUtility.SetDirty(boardManager);
        }
    }


}
#endif
