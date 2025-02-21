using UnityEditor;
using UnityEngine;

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