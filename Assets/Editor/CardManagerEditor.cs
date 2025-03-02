using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CardManager))]
public class CardManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenha o Inspector padrão
        DrawDefaultInspector();

        // Referência ao CardManager
        CardManager cardManager = (CardManager)target;

        // Botão para gerar IDs
        if (GUILayout.Button("Generate Card IDs"))
        {
            GenerateCardIds(cardManager);
        }
    }

    private void GenerateCardIds(CardManager cardManager)
    {
        // Verifica se a lista de cartas está vazia
        if (cardManager.AllCards == null || cardManager.AllCards.Count == 0)
        {
            Debug.LogWarning("A lista de cartas está vazia. Adicione cartas ao CardManager.");
            return;
        }

        // Atribui IDs às cartas
        for (int i = 0; i < cardManager.AllCards.Count; i++)
        {
            cardManager.AllCards[i].id = i + 1; // IDs começam em 1
            EditorUtility.SetDirty(cardManager.AllCards[i]); // Marca o ScriptableObject como modificado
        }

        // Salva as alterações no disco
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("IDs das cartas gerados com sucesso!");
    }
}