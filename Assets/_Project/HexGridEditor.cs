using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class HexGridEditor : MonoBehaviour
{
    [Header("Grid Configuration")]
    public float hexSize = 1f;
    public float spacing = 0.05f;
    public int radius = 0;

    [Header("Orientation")]
    public HexOrientation orientation = HexOrientation.PointyTop;

    [Header("Prefabs")]
    public List<GameObject> hexPrefabs = new List<GameObject>();
    public List<GameObject> decorations = new List<GameObject>(); // Decorações (casas, pontes, etc.)
    public List<GameObject> natureDecorations = new List<GameObject>(); // Decorações naturais (árvores, pedras, etc.)

    [Header("Prefab Settings")]
    public Vector3 prefabRotationOffset = Vector3.zero;
    public bool showTestHex = false;

    [Header("Editor Settings")]
    public Color gridColor = Color.white;
    public float lineThickness = 0.1f;
    public bool showCoordinates = true;
    public float coordinatesSize = 0.2f;

    // Internal variables
    private int currentPrefabIndex = 0;
    private int currentDecorationIndex = 0;
    private int currentNatureDecorationIndex = 0;
    private Dictionary<Vector3Int, GameObject> placedHexagons = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, List<GameObject>> placedDecorations = new Dictionary<Vector3Int, List<GameObject>>();
    private Vector3Int? hoveredCell = null;

    private const float ROOT_3 = 1.73205f;

    public enum HexOrientation
    {
        FlatTop,    // Flat side on top (flat sides horizontal)
        PointyTop   // Pointy side on top (flat sides vertical)
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;

        // Reconstruir o dicionário placedHexagons ao iniciar
        RebuildPlacedHexagons();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void RebuildPlacedHexagons()
    {
        placedHexagons.Clear();

        // Percorre todos os objetos filhos do HexGridEditor
        foreach (Transform child in transform)
        {
            HexCoordinates hexCoord = child.GetComponent<HexCoordinates>();
            if (hexCoord != null)
            {
                // Converte as coordenadas do hexágono para Vector3Int
                Vector3Int cubeCoord = new Vector3Int(hexCoord.q, hexCoord.r, hexCoord.s);

                // Adiciona ao dicionário placedHexagons
                if (!placedHexagons.ContainsKey(cubeCoord))
                {
                    placedHexagons.Add(cubeCoord, child.gameObject);
                }
            }
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Verifica se um prefab foi selecionado na aba Project
        GameObject selectedPrefab = null;
        if (Selection.activeObject != null && Selection.activeObject is GameObject)
        {
            selectedPrefab = Selection.activeObject as GameObject;

            // Atualiza o índice do prefab selecionado
            if (hexPrefabs.Contains(selectedPrefab))
            {
                currentPrefabIndex = hexPrefabs.IndexOf(selectedPrefab);
                currentDecorationIndex = -1; // Nenhuma decoração selecionada
                currentNatureDecorationIndex = -1; // Nenhuma decoração natural selecionada
            }
            else if (decorations.Contains(selectedPrefab))
            {
                currentDecorationIndex = decorations.IndexOf(selectedPrefab);
                currentNatureDecorationIndex = -1; // Nenhuma decoração natural selecionada
                currentPrefabIndex = -1; // Nenhum hexágono selecionado
            }
            else if (natureDecorations.Contains(selectedPrefab))
            {
                currentNatureDecorationIndex = natureDecorations.IndexOf(selectedPrefab);
                currentDecorationIndex = -1; // Nenhuma decoração selecionada
                currentPrefabIndex = -1; // Nenhum hexágono selecionado
            }
        }

        // Handle keyboard input
        if (e.type == EventType.KeyDown)
        {
            // Place or remove hex with space key
            if (e.keyCode == KeyCode.Space && !e.control && hoveredCell.HasValue)
            {
                HandleHexPlacement(hoveredCell.Value);
                e.Use();
                SceneView.RepaintAll();
            }
            // Place or remove decorations with Ctrl + Space
            else if (e.keyCode == KeyCode.Space && e.control && hoveredCell.HasValue)
            {
                HandleDecorationPlacement(hoveredCell.Value);
                e.Use();
                SceneView.RepaintAll();
            }
            // Rotate hex with Ctrl + R
            else if (e.keyCode == KeyCode.R && e.control && hoveredCell.HasValue)
            {
                RotateHex(hoveredCell.Value);
                e.Use();
                SceneView.RepaintAll();
            }
        }

        // Raycast to find hover cell
        if (e.type == EventType.MouseMove)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3Int cell = WorldToCube(hit.point);
                if (!hoveredCell.HasValue || !hoveredCell.Value.Equals(cell))
                {
                    hoveredCell = cell;
                    SceneView.RepaintAll();
                }
            }
            else
            {
                Plane plane = new Plane(Vector3.up, 0);
                float distance;
                if (plane.Raycast(ray, out distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    Vector3Int cell = WorldToCube(hitPoint);
                    if (!hoveredCell.HasValue || !hoveredCell.Value.Equals(cell))
                    {
                        hoveredCell = cell;
                        SceneView.RepaintAll();
                    }
                }
            }
        }

        // Draw the hex grid
        DrawHexGrid();

        // Draw info box
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));

        GUILayout.Label($"Selected Hex: {(hexPrefabs.Count > 0 && currentPrefabIndex >= 0 ? hexPrefabs[currentPrefabIndex].name : "None")}");
        GUILayout.Label($"Selected Decoration: {(decorations.Count > 0 && currentDecorationIndex >= 0 ? decorations[currentDecorationIndex].name : "None")}");
        GUILayout.Label($"Selected Nature Decoration: {(natureDecorations.Count > 0 && currentNatureDecorationIndex >= 0 ? natureDecorations[currentNatureDecorationIndex].name : "None")}");

        if (hoveredCell.HasValue)
        {
            GUILayout.Label($"Hover Cell (q,r,s): ({hoveredCell.Value.x}, {hoveredCell.Value.y}, {hoveredCell.Value.z})");
        }
        else
        {
            GUILayout.Label("Hover Cell: None");
        }
        GUILayout.Label("Controls: Space = Place/Remove Hex, Ctrl+Space = Place/Remove Decoration, Ctrl+R = Rotate Hex");
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    void RotateHex(Vector3Int cell)
    {
        if (placedHexagons.ContainsKey(cell))
        {
            GameObject hexToRotate = placedHexagons[cell];
            Quaternion currentRotation = hexToRotate.transform.rotation;

            // Rotaciona o hexágono em 60 graus (ou outro ângulo desejado)
            float rotationAngle = 60f; // Você pode alterar esse valor se necessário
            Quaternion newRotation = currentRotation * Quaternion.Euler(0, rotationAngle, 0);

            // Aplica a nova rotação
            hexToRotate.transform.rotation = newRotation;

            // Registra a operação no Undo
            Undo.RecordObject(hexToRotate.transform, "Rotate Hex");

            // Marca a cena como suja
            EditorUtility.SetDirty(hexToRotate);
            SceneView.RepaintAll();

            Debug.Log("Hex rotated.");
        }
        else
        {
            Debug.Log("No hex at this cell.");
        }
    }

    void HandleHexPlacement(Vector3Int cell)
    {
        if (placedHexagons.ContainsKey(cell))
        {
            // Remove hex and all its decorations
            GameObject hexToRemove = placedHexagons[cell];
            if (placedDecorations.ContainsKey(cell))
            {
                foreach (GameObject decoration in placedDecorations[cell])
                {
                    Undo.DestroyObjectImmediate(decoration);
                }
                placedDecorations.Remove(cell);
            }
            Undo.DestroyObjectImmediate(hexToRemove);
            placedHexagons.Remove(cell);
        }
        else if (hexPrefabs.Count > 0 && currentPrefabIndex >= 0 && hexPrefabs[currentPrefabIndex] != null)
        {
            // Place new hex
            Vector3 worldPos = CubeToWorld(cell);
            GameObject newHex = PrefabUtility.InstantiatePrefab(hexPrefabs[currentPrefabIndex]) as GameObject;
            newHex.transform.position = worldPos;
            newHex.transform.rotation = GetPrefabRotation(cell);
            newHex.transform.parent = this.transform;

            // Store cube coordinates in a component for reference
            HexCoordinates hexCoord = newHex.AddComponent<HexCoordinates>();
            hexCoord.q = cell.x;
            hexCoord.r = cell.y;
            hexCoord.s = cell.z;

            placedHexagons.Add(cell, newHex);
            placedDecorations[cell] = new List<GameObject>();

            // Register undo
            Undo.RegisterCreatedObjectUndo(newHex, "Place Hex");
        }
    }

    void HandleDecorationPlacement(Vector3Int cell)
    {
        if (placedHexagons.ContainsKey(cell))
        {
            if (placedDecorations.ContainsKey(cell) && placedDecorations[cell].Count > 0)
            {
                // Remove a primeira decoração, se ela ainda existir
                GameObject decorationToRemove = placedDecorations[cell][0];

                // Verifica se o objeto ainda existe antes de tentar destruí-lo
                if (decorationToRemove != null)
                {
                    Undo.DestroyObjectImmediate(decorationToRemove);
                }

                // Remove a decoração da lista
                placedDecorations[cell].RemoveAt(0);

                // Se não houver mais decorações, remove a célula do dicionário
                if (placedDecorations[cell].Count == 0)
                {
                    placedDecorations.Remove(cell);
                }

                Debug.Log("Decoration removed.");
            }
            else
            {
                // Adiciona uma nova decoração
                if (currentDecorationIndex >= 0 && decorations.Count > 0 && decorations[currentDecorationIndex] != null)
                {
                    PlaceDecoration(cell, decorations[currentDecorationIndex]);
                    Debug.Log("Decoration placed.");
                }
                else if (currentNatureDecorationIndex >= 0 && natureDecorations.Count > 0 && natureDecorations[currentNatureDecorationIndex] != null)
                {
                    PlaceDecoration(cell, natureDecorations[currentNatureDecorationIndex], true);
                    Debug.Log("Nature decoration placed.");
                }
                else
                {
                    Debug.Log("No decoration selected.");
                }
            }
        }
        else
        {
            Debug.Log("No hex at this cell.");
        }
    }

    void PlaceDecoration(Vector3Int cell, GameObject decorationPrefab, bool isNatureDecoration = false)
    {
        // Obtém a posição do hexágono
        Vector3 worldPos = CubeToWorld(cell);

        // Instancia a decoração
        GameObject newDecoration = PrefabUtility.InstantiatePrefab(decorationPrefab) as GameObject;

        // Define o pai da decoração como o hexágono correspondente
        newDecoration.transform.parent = placedHexagons[cell].transform;

        // Posiciona a decoração em cima do hexágono
        newDecoration.transform.localPosition = Vector3.zero; // Posição relativa ao hexágono (centro)
        newDecoration.transform.localPosition += Vector3.up * 1f; // Ajuste a altura conforme necessário

        // Aplica rotação aleatória para decorações naturais
        if (isNatureDecoration)
        {
            float randomRotation = Random.Range(0, 360);
            newDecoration.transform.localRotation = Quaternion.Euler(0, randomRotation, 0);
        }
        else
        {
            newDecoration.transform.localRotation = Quaternion.identity;
        }

        // Adiciona à lista de decorações
        if (!placedDecorations.ContainsKey(cell))
        {
            placedDecorations[cell] = new List<GameObject>();
        }
        placedDecorations[cell].Add(newDecoration);

        // Registra a operação no Undo
        Undo.RegisterCreatedObjectUndo(newDecoration, "Place Decoration");
    }

    void HandleSpaceKey(Vector3Int cell)
    {
        if (placedHexagons.ContainsKey(cell))
        {
            // Remove decorations first
            if (placedDecorations.ContainsKey(cell) && placedDecorations[cell].Count > 0)
            {
                GameObject decorationToRemove = placedDecorations[cell][0];
                Undo.DestroyObjectImmediate(decorationToRemove);
                placedDecorations[cell].RemoveAt(0);

                if (placedDecorations[cell].Count == 0)
                {
                    placedDecorations.Remove(cell);
                }
            }
            else
            {
                // Remove hex if no decorations left
                GameObject hexToRemove = placedHexagons[cell];
                Undo.DestroyObjectImmediate(hexToRemove);
                placedHexagons.Remove(cell);
            }
        }
        else if (hexPrefabs.Count > 0 && hexPrefabs[currentPrefabIndex] != null)
        {
            // Place new hex
            Vector3 worldPos = CubeToWorld(cell);
            GameObject newHex = PrefabUtility.InstantiatePrefab(hexPrefabs[currentPrefabIndex]) as GameObject;
            newHex.transform.position = worldPos;
            newHex.transform.rotation = GetPrefabRotation(cell);
            newHex.transform.parent = this.transform;

            // Store cube coordinates in a component for reference
            HexCoordinates hexCoord = newHex.AddComponent<HexCoordinates>();
            hexCoord.q = cell.x;
            hexCoord.r = cell.y;
            hexCoord.s = cell.z;

            placedHexagons.Add(cell, newHex);
            placedDecorations[cell] = new List<GameObject>();

            // Register undo
            Undo.RegisterCreatedObjectUndo(newHex, "Place Hex");
        }
    }

    // Get rotation for prefab based on grid orientation and offset
    Quaternion GetPrefabRotation(Vector3Int cubeCoord)
    {
        Quaternion baseRotation;

        if (orientation == HexOrientation.PointyTop)
        {
            // For pointy-top, the base rotation is 0 degrees
            baseRotation = Quaternion.Euler(0, 0, 0);
        }
        else // FlatTop
        {
            // For flat-top, rotate 30 degrees
            baseRotation = Quaternion.Euler(0, 30, 0);
        }

        // Apply additional offset
        return baseRotation * Quaternion.Euler(prefabRotationOffset);
    }

    void DrawHexGrid()
    {
        if (!hoveredCell.HasValue) return;

        // Desenha os hexágonos dentro do raio ao redor do mouse
        for (int q = -radius; q <= radius; q++)
        {
            for (int r = -radius; r <= radius; r++)
            {
                for (int s = -radius; s <= radius; s++)
                {
                    if (q + r + s == 0) // Apenas células válidas no grid hexagonal
                    {
                        Vector3Int cell = new Vector3Int(
                            hoveredCell.Value.x + q,
                            hoveredCell.Value.y + r,
                            hoveredCell.Value.z + s
                        );

                        Vector3 center = CubeToWorld(cell);
                        bool isHovered = cell.Equals(hoveredCell.Value);

                        // Desenha o contorno do hexágono
                        DrawHexOutline(center, hexSize, isHovered);

                        // Desenha as coordenadas se estiverem habilitadas
                        if (showCoordinates)
                        {
                            DrawCoordinates(center, cell);
                        }
                    }
                }
            }
        }
    }

    void DrawCoordinates(Vector3 center, Vector3Int cubeCoord)
    {
        Vector3 textPos = center + Vector3.up * 0.05f;
        string coordText = $"({cubeCoord.x},{cubeCoord.y},{cubeCoord.z})";
        Handles.Label(textPos, coordText, CreateTextStyle());
    }

    GUIStyle CreateTextStyle()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = Mathf.RoundToInt(coordinatesSize * 10);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;
        return style;
    }

    void DrawHexOutline(Vector3 center, float size, bool highlight)
    {
        List<Vector3> hexCorners = GetHexCorners(center, size);

        Color color = highlight ? Color.yellow : gridColor;
        Handles.color = color;

        // Draw each edge of the hexagon
        for (int i = 0; i < 6; i++)
        {
            int nextIndex = (i + 1) % 6;
            Handles.DrawLine(hexCorners[i], hexCorners[nextIndex], lineThickness);
        }

        // Draw a small dot at the center for clarity
        Handles.color = highlight ? Color.red : color;
        Handles.DrawSolidDisc(center, Vector3.up, size * 0.05f);
    }

    List<Vector3> GetHexCorners(Vector3 center, float size)
    {
        List<Vector3> corners = new List<Vector3>();

        // Starting angle depends on orientation
        float startAngle = orientation == HexOrientation.PointyTop ? 0 : Mathf.PI / 6f;

        for (int i = 0; i < 6; i++)
        {
            float angle = startAngle + Mathf.PI / 3f * i; // 60 degrees * i
            Vector3 corner = new Vector3(
                center.x + size * Mathf.Cos(angle),
                center.y,
                center.z + size * Mathf.Sin(angle)
            );
            corners.Add(corner);
        }
        return corners;
    }

    // Convert cube coordinates to world position
    public Vector3 CubeToWorld(Vector3Int cubeCoord)
    {
        int q = cubeCoord.x;
        int r = cubeCoord.y;

        float x, z;

        if (orientation == HexOrientation.PointyTop)
        {
            // Pointy-top layout
            x = hexSize * (3f / 2f * q);
            z = hexSize * (ROOT_3 / 2f * q + ROOT_3 * r);
        }
        else
        {
            // Flat-top layout
            x = hexSize * (ROOT_3 * q + ROOT_3 / 2f * r);
            z = hexSize * (3f / 2f * r);
        }

        // Apply spacing
        x *= (1 + spacing);
        z *= (1 + spacing);

        return new Vector3(x, 0, z) + transform.position;
    }

    // Convert world position to cube coordinates
    public Vector3Int WorldToCube(Vector3 worldPos)
    {
        // Adjust for spacing
        float adjustedHexSize = hexSize * (1 + spacing);

        // Convert to local position
        Vector3 localPos = worldPos - transform.position;

        // Convert to fraction cube coordinates
        float q, r;

        if (orientation == HexOrientation.PointyTop)
        {
            // Pointy-top layout
            q = (2f / 3f * localPos.x) / adjustedHexSize;
            r = (-1f / 3f * localPos.x + ROOT_3 / 3f * localPos.z) / adjustedHexSize;
        }
        else
        {
            // Flat-top layout
            q = (ROOT_3 / 3f * localPos.x - 1f / 3f * localPos.z) / adjustedHexSize;
            r = (2f / 3f * localPos.z) / adjustedHexSize;
        }

        float s = -q - r;

        // Round to nearest cube coordinates
        return CubeRound(new Vector3(q, r, s));
    }

    // Round to nearest hex cell using cube coordinates
    private Vector3Int CubeRound(Vector3 cube)
    {
        float rx = Mathf.Round(cube.x);
        float ry = Mathf.Round(cube.y);
        float rz = Mathf.Round(cube.z);

        float xDiff = Mathf.Abs(rx - cube.x);
        float yDiff = Mathf.Abs(ry - cube.y);
        float zDiff = Mathf.Abs(rz - cube.z);

        // If x was rounded furthest, adjust it
        if (xDiff > yDiff && xDiff > zDiff)
            rx = -ry - rz;
        // If y was rounded furthest, adjust it
        else if (yDiff > zDiff)
            ry = -rx - rz;
        // Otherwise, z was rounded furthest, adjust it
        else
            rz = -rx - ry;

        return new Vector3Int(Mathf.RoundToInt(rx), Mathf.RoundToInt(ry), Mathf.RoundToInt(rz));
    }

    // Save the data
    void OnValidate()
    {
        // Ensure grid dimensions don't go below minimum values
        hexSize = Mathf.Max(0.1f, hexSize);
        spacing = Mathf.Max(0f, spacing);
        //radius = Mathf.Max(1, radius); // Garante que o raio seja pelo menos 1
    }
}

// Custom editor to improve usability
[CustomEditor(typeof(HexGridEditor))]
public class HexGridEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        HexGridEditor gridEditor = (HexGridEditor)target;

        EditorGUILayout.LabelField("Hex Grid Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Grid Configuration", EditorStyles.boldLabel);
        gridEditor.hexSize = EditorGUILayout.FloatField("Hex Size", gridEditor.hexSize);
        gridEditor.spacing = EditorGUILayout.FloatField("Spacing", gridEditor.spacing);
        gridEditor.radius = EditorGUILayout.IntField("Radius", gridEditor.radius);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Orientation", EditorStyles.boldLabel);
        gridEditor.orientation = (HexGridEditor.HexOrientation)EditorGUILayout.EnumPopup("Hex Orientation", gridEditor.orientation);
        EditorGUILayout.HelpBox("PointyTop: hexágonos com ponta para cima\nFlatTop: hexágonos com lado plano para cima", MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Add hex tile prefabs to the list below. You can cycle through them using 1 and 2 keys.", MessageType.Info);

        SerializedProperty prefabsProperty = serializedObject.FindProperty("hexPrefabs");
        SerializedProperty decorationsProperty = serializedObject.FindProperty("decorations");
        SerializedProperty natureDecorationsProperty = serializedObject.FindProperty("natureDecorations");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        EditorGUILayout.PropertyField(decorationsProperty, true);
        EditorGUILayout.PropertyField(natureDecorationsProperty, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);
        gridEditor.prefabRotationOffset = EditorGUILayout.Vector3Field("Rotação Adicional", gridEditor.prefabRotationOffset);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);
        gridEditor.gridColor = EditorGUILayout.ColorField("Grid Color", gridEditor.gridColor);
        gridEditor.lineThickness = EditorGUILayout.Slider("Line Thickness", gridEditor.lineThickness, 0.01f, 0.5f);
        gridEditor.showCoordinates = EditorGUILayout.Toggle("Show Coordinates", gridEditor.showCoordinates);
        if (gridEditor.showCoordinates)
        {
            gridEditor.coordinatesSize = EditorGUILayout.Slider("Coordinates Size", gridEditor.coordinatesSize, 0.1f, 1f);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Controls: \n• 1/2: Cycle through prefabs\n• Space: Place/Remove hex at mouse position", MessageType.Info);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            SceneView.RepaintAll();
        }
    }
}
#endif