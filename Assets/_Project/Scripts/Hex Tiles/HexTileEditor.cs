#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

public class HexTileEditor : EditorWindow
{
    private TileRegion regionToSet;
    private Material materialToSet;
    private bool includeCardTiles = true;
    private bool includeBasicTiles = true;

    [MenuItem("Tools/Hex Board/Configure Tiles")]
    public static void ShowWindow()
    {
        GetWindow<HexTileEditor>("Tile Configurator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configure múltiplos Hex Tiles", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);
        GUILayout.Label("Filtros", EditorStyles.boldLabel);
        includeCardTiles = EditorGUILayout.Toggle("Incluir Card Tiles", includeCardTiles);
        includeBasicTiles = EditorGUILayout.Toggle("Incluir Basic Tiles", includeBasicTiles);

        EditorGUILayout.Space(10);
        GUILayout.Label("Configuração de Região", EditorStyles.boldLabel);
        regionToSet = (TileRegion)EditorGUILayout.EnumPopup("Região", regionToSet);
        if (GUILayout.Button("Aplicar Região aos Selecionados"))
        {
            ApplyRegionToSelectedTiles();
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Configuração de Material", EditorStyles.boldLabel);
        materialToSet = (Material)EditorGUILayout.ObjectField("Material", materialToSet, typeof(Material), false);
        if (GUILayout.Button("Aplicar Material aos Selecionados"))
        {
            ApplyMaterialToSelectedTiles();
        }

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Encontrar Tiles por Região"))
        {
            FindTilesByRegion();
        }
    }

    private void ApplyRegionToSelectedTiles()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int count = 0;

        foreach (GameObject obj in selectedObjects)
        {
            HexTile tile = obj.GetComponent<HexTile>();
            if (tile != null)
            {
                bool isCardTile = tile is CardTile;

                if ((isCardTile && includeCardTiles) || (!isCardTile && includeBasicTiles))
                {
                    Undo.RecordObject(tile, "Change Tile Region");
                    SerializedObject serializedTile = new SerializedObject(tile);
                    SerializedProperty regionProperty = serializedTile.FindProperty("region");
                    regionProperty.enumValueIndex = (int)regionToSet;
                    serializedTile.ApplyModifiedProperties();
                    count++;
                    EditorUtility.SetDirty(tile);
                }
            }
        }

        Debug.Log($"Configurados {count} tiles para a região {regionToSet}");
    }

    private void ApplyMaterialToSelectedTiles()
    {
        if (materialToSet == null)
        {
            EditorUtility.DisplayDialog("Erro", "Selecione um material para aplicar", "OK");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;
        int count = 0;

        foreach (GameObject obj in selectedObjects)
        {
            HexTile tile = obj.GetComponent<HexTile>();
            if (tile != null)
            {
                bool isCardTile = tile is CardTile;

                if ((isCardTile && includeCardTiles) || (!isCardTile && includeBasicTiles))
                {
                    Undo.RecordObject(tile, "Change Tile Material");
                    SerializedObject serializedTile = new SerializedObject(tile);
                    SerializedProperty materialProperty = serializedTile.FindProperty("tileMaterial");
                    materialProperty.objectReferenceValue = materialToSet;
                    serializedTile.ApplyModifiedProperties();

                    // Força a atualização do material no editor
                    tile.SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);

                    count++;
                    EditorUtility.SetDirty(tile);
                }
            }
        }

        Debug.Log($"Material aplicado a {count} tiles");
    }

    private void FindTilesByRegion()
    {
        List<GameObject> tilesToSelect = new List<GameObject>();
        HexTile[] allTiles = FindObjectsByType<HexTile>(FindObjectsSortMode.None);

        foreach (HexTile tile in allTiles)
        {
            SerializedObject serializedTile = new SerializedObject(tile);
            SerializedProperty regionProperty = serializedTile.FindProperty("region");

            if (regionProperty.enumValueIndex == (int)regionToSet)
            {
                bool isCardTile = tile is CardTile;
                if ((isCardTile && includeCardTiles) || (!isCardTile && includeBasicTiles))
                {
                    tilesToSelect.Add(tile.gameObject);
                }
            }
        }

        Selection.objects = tilesToSelect.ToArray();
        Debug.Log($"Encontrados {tilesToSelect.Count} tiles na região {regionToSet}");
    }
}
#endif