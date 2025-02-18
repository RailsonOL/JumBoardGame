using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardManager))]
public class BoardManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BoardManager boardManager = (BoardManager)target;

        if (GUILayout.Button("Gerar Conexões HexTiles"))
        {
            boardManager.GenerateHexConnections();
            EditorUtility.SetDirty(boardManager); // Marca como modificado para salvar na cena
        }
    }
}
