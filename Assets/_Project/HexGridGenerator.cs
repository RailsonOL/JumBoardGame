using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

public class HexGridGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    public int gridWidth = 8;
    public int gridHeight = 8;
    public int borderWidth = 8;

    private readonly float horizontalSpacing = 1.732f;
    private readonly float verticalSpacing = 1.5f;

    // Nomes especiais para as pontas
    private readonly string[] cornerNames = new string[]
    {
        "NEXUS_NE", // Nordeste
        "NEXUS_E",  // Leste
        "NEXUS_SE", // Sudeste
        "NEXUS_SW", // Sudoeste
        "NEXUS_W",  // Oeste
        "NEXUS_NW"  // Noroeste
    };

    void OnDrawGizmos()
    {
        for (int q = -gridWidth; q <= gridWidth; q++)
        {
            for (int r = -gridHeight; r <= gridHeight; r++)
            {
                if (IsHexInBorder(q, r))
                {
                    Vector3 position = GetHexPosition(q, r);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(position, 0.1f);
                }
            }
        }
    }

    private bool IsHexInBorder(int q, int r)
    {
        int distance = GetHexDistance(q, r);
        bool isInOuterBorder = distance == borderWidth;
        bool isInGrid = Mathf.Abs(q + r) <= gridWidth;
        return isInOuterBorder && isInGrid;
    }

    private bool IsCorner(int q, int r)
    {
        // Identifica se a posição é uma das 6 pontas
        Vector3 pos = GetHexPosition(q, r);
        float angle = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        int distance = GetHexDistance(q, r);
        return distance == borderWidth && Mathf.Abs(angle % 60) < 5;
    }

    private int GetFaceNumber(int q, int r)
    {
        Vector3 pos = GetHexPosition(q, r);
        float angle = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        // Divide o hexágono em 6 faces (60 graus cada)
        return (int)(angle / 60) + 1;
    }

    private int GetHexDistance(int q, int r)
    {
        return (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(-q - r)) / 2;
    }

    private string GetHexName(int q, int r)
    {
        if (IsCorner(q, r))
        {
            Vector3 pos = GetHexPosition(q, r);
            float angle = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;
            int cornerIndex = ((int)(angle / 60)) % 6;
            return cornerNames[cornerIndex];
        }

        int face = GetFaceNumber(q, r);
        int subNumber = GetSubNumber(q, r, face);
        return $"Face_{face}_{subNumber}";
    }

    private int GetSubNumber(int q, int r, int face)
    {
        // Calcula um número sequencial para cada hexágono dentro da face
        Vector3 pos = GetHexPosition(q, r);
        float angle = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        // Normaliza o ângulo para a face atual
        float normalizedAngle = angle - (face - 1) * 60;
        float distance = Vector3.Distance(Vector3.zero, pos);

        // Cria um número único baseado na posição dentro da face
        return Mathf.RoundToInt(normalizedAngle * distance / 30);
    }

    public void GenerateGrid()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        for (int q = -gridWidth; q <= gridWidth; q++)
        {
            for (int r = -gridHeight; r <= gridHeight; r++)
            {
                if (IsHexInBorder(q, r))
                {
                    Vector3 position = GetHexPosition(q, r);
                    GameObject hex = Instantiate(hexPrefab, position, Quaternion.Euler(90, 0, 0), transform);
                    hex.name = GetHexName(q, r);
                }
            }
        }
    }

    public Vector3 GetHexPosition(int q, int r)
    {
        float x = horizontalSpacing * (q + r / 2f);
        float y = verticalSpacing * r;
        return new Vector3(x, 0, y);
    }

    [MenuItem("GameObject/Sort Children Naturally", false, 0)]
    static void SortSelectedGameObject()
    {
        Transform[] selections = Selection.transforms;

        foreach (Transform selection in selections)
        {
            SortChildrenNaturally(selection);
        }
    }

    static void SortChildrenNaturally(Transform parent)
    {
        var children = parent.Cast<Transform>()
            .OrderBy(t => t.name, new NaturalComparer())
            .ToList();

        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }

        Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Sort Children Naturally");
        EditorUtility.SetDirty(parent.gameObject);
    }
}

// Comparador natural para strings
public class NaturalComparer : System.Collections.Generic.IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // Usa expressões regulares para dividir as strings em partes numéricas e não numéricas
        var regex = new Regex(@"(\d+)|(\D+)");
        var xParts = regex.Matches(x).Cast<Match>().Select(m => m.Value).ToArray();
        var yParts = regex.Matches(y).Cast<Match>().Select(m => m.Value).ToArray();

        int minLength = Mathf.Min(xParts.Length, yParts.Length);

        for (int i = 0; i < minLength; i++)
        {
            string xPart = xParts[i];
            string yPart = yParts[i];

            // Se ambas as partes forem numéricas, compara como números
            if (int.TryParse(xPart, out int xNum) && int.TryParse(yPart, out int yNum))
            {
                if (xNum != yNum) return xNum.CompareTo(yNum);
            }
            // Caso contrário, compara como strings
            else
            {
                int stringCompare = string.Compare(xPart, yPart, System.StringComparison.OrdinalIgnoreCase);
                if (stringCompare != 0) return stringCompare;
            }
        }

        // Se todas as partes forem iguais, retorna a comparação pelo comprimento
        return xParts.Length.CompareTo(yParts.Length);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(HexGridGenerator))]
public class HexGridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexGridGenerator generator = (HexGridGenerator)target;

        if (GUILayout.Button("Generate Grid"))
        {
            generator.GenerateGrid();
        }

        if (GUILayout.Button("Generate Grid"))
        {
            generator.GenerateGrid();
        }
    }
}

#endif