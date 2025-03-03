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
    public int radius = 0; // Raio de exibição ao redor do mouse

    [Header("Orientation")]
    [Tooltip("Orientação do grid hexagonal. Define como os hexágonos estão alinhados.")]
    public HexOrientation orientation = HexOrientation.PointyTop;

    [Header("Prefabs")]
    public List<GameObject> hexPrefabs = new List<GameObject>();

    [Header("Prefab Settings")]
    [Tooltip("Rotação adicional aplicada aos prefabs (além da rotação base de alinhamento)")]
    public Vector3 prefabRotationOffset = Vector3.zero;
    [Tooltip("Exibe um hexágono de teste para ajudar a alinhar o grid com seus prefabs")]
    public bool showTestHex = false;

    [Header("Editor Settings")]
    public Color gridColor = Color.white;
    public float lineThickness = 0.1f;
    public bool showCoordinates = true;
    public float coordinatesSize = 0.2f;

    // Internal variables
    private int currentPrefabIndex = 0;
    private Dictionary<Vector3Int, GameObject> placedHexagons = new Dictionary<Vector3Int, GameObject>();
    private Vector3Int? hoveredCell = null;
    private GameObject testHexInstance = null;

    // Constants
    private const float ROOT_3 = 1.73205f; // Square root of 3

    public enum HexOrientation
    {
        FlatTop,    // Flat side on top (flat sides horizontal)
        PointyTop   // Pointy side on top (flat sides vertical)
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        UpdateTestHex();
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyTestHex();
    }

    void UpdateTestHex()
    {
        DestroyTestHex();

        if (showTestHex && hexPrefabs.Count > 0 && hexPrefabs[currentPrefabIndex] != null)
        {
            testHexInstance = PrefabUtility.InstantiatePrefab(hexPrefabs[currentPrefabIndex]) as GameObject;
            if (hoveredCell.HasValue)
            {
                testHexInstance.transform.position = CubeToWorld(hoveredCell.Value);
            }
            else
            {
                testHexInstance.transform.position = transform.position;
            }
            testHexInstance.transform.rotation = GetPrefabRotation(Vector3Int.zero);
            testHexInstance.name = "TEST_HEX";
        }
    }

    void DestroyTestHex()
    {
        if (testHexInstance != null)
        {
            DestroyImmediate(testHexInstance);
            testHexInstance = null;
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        // Handle keyboard input
        if (e.type == EventType.KeyDown)
        {
            // Switch prefab selection with 1 and 2 keys
            if (e.keyCode == KeyCode.Alpha1 && hexPrefabs.Count > 0)
            {
                currentPrefabIndex = (currentPrefabIndex - 1 + hexPrefabs.Count) % hexPrefabs.Count;
                UpdateTestHex();
                e.Use();
                SceneView.RepaintAll();
            }
            else if (e.keyCode == KeyCode.Alpha2 && hexPrefabs.Count > 0)
            {
                currentPrefabIndex = (currentPrefabIndex + 1) % hexPrefabs.Count;
                UpdateTestHex();
                e.Use();
                SceneView.RepaintAll();
            }
            // Place or remove hex with space key
            else if (e.keyCode == KeyCode.Space && hoveredCell.HasValue)
            {
                if (placedHexagons.ContainsKey(hoveredCell.Value))
                {
                    // Remove existing hex
                    DestroyImmediate(placedHexagons[hoveredCell.Value]);
                    placedHexagons.Remove(hoveredCell.Value);
                }
                else if (hexPrefabs.Count > 0 && hexPrefabs[currentPrefabIndex] != null)
                {
                    // Place new hex
                    Vector3 worldPos = CubeToWorld(hoveredCell.Value);
                    GameObject newHex = PrefabUtility.InstantiatePrefab(hexPrefabs[currentPrefabIndex]) as GameObject;
                    newHex.transform.position = worldPos;
                    newHex.transform.rotation = GetPrefabRotation(hoveredCell.Value);
                    newHex.transform.parent = this.transform;

                    // Store cube coordinates in a component for reference
                    HexCoordinates hexCoord = newHex.AddComponent<HexCoordinates>();
                    hexCoord.q = hoveredCell.Value.x;
                    hexCoord.r = hoveredCell.Value.y;
                    hexCoord.s = hoveredCell.Value.z;

                    placedHexagons.Add(hoveredCell.Value, newHex);

                    // Register undo
                    Undo.RegisterCreatedObjectUndo(newHex, "Place Hex");
                }
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
                // Raycast against an infinite plane at Y=0 if no collider was hit
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

            // Atualiza a posição do hexágono de teste para seguir o mouse
            if (showTestHex && testHexInstance != null && hoveredCell.HasValue)
            {
                Vector3 worldPos = CubeToWorld(hoveredCell.Value);
                testHexInstance.transform.position = worldPos;
            }
        }

        // Draw the hex grid
        DrawHexGrid();

        // Draw info box
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));

        // Mostra o nome do prefab selecionado
        GUILayout.Label($"Selected Prefab: {(hexPrefabs.Count > 0 ? hexPrefabs[currentPrefabIndex].name + " (" + (currentPrefabIndex + 1) + " / " + hexPrefabs.Count + ")" : "None")}");

        if (hoveredCell.HasValue)
        {
            GUILayout.Label($"Hover Cell (q,r,s): ({hoveredCell.Value.x}, {hoveredCell.Value.y}, {hoveredCell.Value.z})");

            // Show rotation info for debugging
            Quaternion rot = GetPrefabRotation(hoveredCell.Value);
            Vector3 eulerRot = rot.eulerAngles;
            GUILayout.Label($"Rotation: ({eulerRot.x:F1}, {eulerRot.y:F1}, {eulerRot.z:F1})");
        }
        else
        {
            GUILayout.Label("Hover Cell: None");
        }
        GUILayout.Label("Controls: 1/2 = Cycle Prefabs, Space = Place/Remove Hex");
        GUILayout.EndArea();
        Handles.EndGUI();
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

        // Update test hex when settings change
        UpdateTestHex();
    }
}

// Component to store hex coordinates
public class HexCoordinates : MonoBehaviour
{
    public int q; // x-axis
    public int r; // y-axis
    public int s; // z-axis (redundant, but stored for convenience)

    void OnValidate()
    {
        // Ensure cube constraint: q + r + s = 0
        s = -q - r;
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
        EditorGUILayout.PropertyField(prefabsProperty, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefab Settings", EditorStyles.boldLabel);
        gridEditor.prefabRotationOffset = EditorGUILayout.Vector3Field("Rotação Adicional", gridEditor.prefabRotationOffset);
        gridEditor.showTestHex = EditorGUILayout.Toggle("Mostrar Hex de Teste", gridEditor.showTestHex);

        if (gridEditor.showTestHex)
        {
            EditorGUILayout.HelpBox("Um hexágono de teste está sendo exibido na origem do grid. Use isso para ajustar a rotação até que o seu prefab se alinhe corretamente.", MessageType.Info);
        }

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