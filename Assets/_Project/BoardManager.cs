using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class BoardManager : MonoBehaviour
{
    [Header("Hex Tiles")]
    public List<HexTile> hexTiles; // Lista de hexágonos na ordem definida no Inspector
    public HexTile startTile; // Hexágono inicial

    [Header("Regiões e Materiais")]
    [Tooltip("Materiais para cada região, na ordem do enum TileRegion")]
    public Material[] regionMaterials = new Material[6]; // Um material para cada tipo de região

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

    public void ConfigureRegionsAndMaterials()
    {
        if (hexTiles == null || hexTiles.Count == 0)
        {
            Debug.LogError("Lista de HexTiles está vazia!");
            return;
        }

        if (regionMaterials.Length < 6)
        {
            Debug.LogError("Configure todos os materiais para as regiões!");
            return;
        }

        int tileCount = hexTiles.Count;

        // Garantir que há 48 tiles
        if (tileCount != 48)
        {
            Debug.LogWarning($"O número de tiles ({tileCount}) não é 48, o que pode afetar a distribuição de regiões.");
        }

        // Pattern: neutro, 7 de região 1, neutro, 7 de região 2, neutro, 7 de região 3, etc.
        // Regiões na ordem: 1-Neutral, 2-Frozen, 3-Plains, 4-PVP, 5-Volcanic, 6-Abyssal

        // Distribuição: 
        // Primeiro é neutro (canto)
        // Depois seguem 7 tiles de cada região, com um tile neutro entre elas

        for (int i = 0; i < tileCount; i++)
        {
            HexTile tile = hexTiles[i];
            SerializedObject serializedTile = new SerializedObject(tile);
            SerializedProperty regionProperty = serializedTile.FindProperty("region");

            TileRegion regionToApply;

            // Calcular a região baseada no índice
            // Os cantos (a cada 8 tiles) são sempre neutros
            if (i % 8 == 0)
            {
                // Cantos são neutros
                regionToApply = TileRegion.Neutral;
            }
            else
            {
                // Calcular qual seção estamos (depois de cada canto neutro)
                int section = i / 8;

                // Cada seção tem um tipo de região (após o tile neutro)
                switch (section)
                {
                    case 0:
                        regionToApply = TileRegion.Neutral;
                        break;
                    case 1:
                        regionToApply = TileRegion.Frozen;
                        break;
                    case 2:
                        regionToApply = TileRegion.Plains;
                        break;
                    case 3:
                        regionToApply = TileRegion.PVP;
                        break;
                    case 4:
                        regionToApply = TileRegion.Volcanic;
                        break;
                    case 5:
                    default:
                        regionToApply = TileRegion.Abyssal;
                        break;
                }
            }

            // Aplicar a região
            regionProperty.enumValueIndex = (int)regionToApply;

            // Aplicar o material correspondente
            SerializedProperty materialProperty = serializedTile.FindProperty("tileMaterial");
            materialProperty.objectReferenceValue = regionMaterials[(int)regionToApply];

            serializedTile.ApplyModifiedProperties();

            // Forçar atualização do material
#if UNITY_EDITOR
            EditorUtility.SetDirty(tile);
            tile.SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);
#endif
        }

        Debug.Log("Regiões e materiais configurados!");
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

        GUILayout.Space(10);

        if (GUILayout.Button("Gerar Conexões HexTiles"))
        {
            boardManager.GenerateHexConnections();
            EditorUtility.SetDirty(boardManager);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Configurar Regiões e Materiais"))
        {
            boardManager.ConfigureRegionsAndMaterials();
            EditorUtility.SetDirty(boardManager);
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Certifique-se de configurar todos os materiais para cada região antes de aplicar.", MessageType.Info);
    }
}
#endif