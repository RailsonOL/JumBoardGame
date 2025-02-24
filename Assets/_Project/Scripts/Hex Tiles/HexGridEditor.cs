// using UnityEngine;
// using UnityEditor;

// public class HexGridEditor : EditorWindow
// {
//     private float hexRadius = 1f;
//     private float hexSpacing = 0.1f;
//     private float hexHeight = 0.1f;
//     private GameObject hexPrefab;
//     private GameObject centerObject;
//     private Color gridColor = new Color(0.5f, 0.5f, 1f, 0.2f);
//     private Color hoverColor = new Color(1f, 1f, 0f, 0.5f);
//     private bool showGrid = true;
//     private KeyCode placementKey = KeyCode.H;
//     private Vector2Int gridDimensions = new Vector2Int(10, 10);

//     [MenuItem("Window/Hex Grid Editor")]
//     public static void ShowWindow()
//     {
//         GetWindow<HexGridEditor>("Hex Grid Editor");
//     }

//     void OnGUI()
//     {
//         GUILayout.Label("Hex Grid Settings", EditorStyles.boldLabel);

//         hexRadius = EditorGUILayout.FloatField("Hex Radius", hexRadius);
//         hexSpacing = EditorGUILayout.FloatField("Hex Spacing", hexSpacing);
//         hexHeight = EditorGUILayout.FloatField("Hex Height", hexHeight);
//         hexPrefab = (GameObject)EditorGUILayout.ObjectField("Hex Prefab", hexPrefab, typeof(GameObject), false);
//         centerObject = (GameObject)EditorGUILayout.ObjectField("Center Object", centerObject, typeof(GameObject), true);

//         EditorGUILayout.Space();
//         GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
//         gridDimensions = EditorGUILayout.Vector2IntField("Grid Size (Width x Height)", gridDimensions);
//         gridColor = EditorGUILayout.ColorField("Grid Color", gridColor);
//         hoverColor = EditorGUILayout.ColorField("Hover Color", hoverColor);

//         EditorGUILayout.Space();
//         showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);
//         placementKey = (KeyCode)EditorGUILayout.EnumPopup("Placement Key", placementKey);

//         if (GUILayout.Button("Clear All Hexes"))
//         {
//             ClearAllHexes();
//         }

//         if (centerObject == null)
//         {
//             EditorGUILayout.HelpBox("Coloque um objeto na cena e selecione-o como 'Center Object' para definir o centro da grade.", MessageType.Info);
//         }
//     }

//     void OnSceneGUI(SceneView sceneView)
//     {
//         if (!showGrid || centerObject == null) return;

//         Vector3 centerPosition = centerObject.transform.position;
//         DrawGlobalGrid(centerPosition);

//         Event e = Event.current;
//         Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
//         RaycastHit hit;
//         Plane gridPlane = new Plane(Vector3.up, new Vector3(centerPosition.x, hexHeight + centerPosition.y, centerPosition.z));
//         float distance;

//         if (gridPlane.Raycast(ray, out distance))
//         {
//             Vector3 hitPoint = ray.GetPoint(distance);
//             Vector3 snapPosition = SnapToHexGrid(hitPoint, centerPosition);

//             if (IsWithinGridBounds(snapPosition, centerPosition))
//             {
//                 DrawHexPreview(snapPosition, hoverColor);

//                 if (e.type == EventType.KeyDown && e.keyCode == placementKey)
//                 {
//                     PlaceHex(snapPosition);
//                     e.Use();
//                 }
//             }
//         }

//         SceneView.RepaintAll();
//     }

//     private bool IsWithinGridBounds(Vector3 position, Vector3 centerPosition)
//     {
//         Vector3 relativePos = position - centerPosition;
//         float effectiveWidth = (hexRadius * 1.732f + hexSpacing) * gridDimensions.x;
//         float effectiveHeight = (hexRadius * 2 + hexSpacing) * gridDimensions.y;

//         return relativePos.x >= -effectiveWidth / 2 && relativePos.x <= effectiveWidth / 2 &&
//                relativePos.z >= -effectiveHeight / 2 && relativePos.z <= effectiveHeight / 2;
//     }

//     private void DrawGlobalGrid(Vector3 centerPosition)
//     {
//         // Distâncias ajustadas para o padrão de colmeia
//         float horizontalDistance = hexRadius * Mathf.Sqrt(3f) + hexSpacing;
//         float verticalDistance = hexRadius * 1.5f + hexSpacing;

//         int halfWidth = gridDimensions.x / 2;
//         int halfHeight = gridDimensions.y / 2;

//         for (int row = -halfHeight; row <= halfHeight; row++)
//         {
//             for (int col = -halfWidth; col <= halfWidth; col++)
//             {
//                 float xPos = col * horizontalDistance + ((row % 2) * horizontalDistance * 0.5f);
//                 float zPos = row * verticalDistance;

//                 Vector3 hexPos = centerPosition + new Vector3(xPos, 0, zPos);
//                 DrawHexPreview(hexPos, gridColor);
//             }
//         }
//     }

//     private Vector3 SnapToHexGrid(Vector3 worldPosition, Vector3 centerPosition)
//     {
//         Vector3 relativePos = worldPosition - centerPosition;

//         float horizontalDistance = hexRadius * Mathf.Sqrt(3f) + hexSpacing;
//         float verticalDistance = hexRadius * 1.5f + hexSpacing;

//         int row = Mathf.RoundToInt(relativePos.z / verticalDistance);
//         float offset = (row % 2) * horizontalDistance * 0.5f;
//         int col = Mathf.RoundToInt((relativePos.x - offset) / horizontalDistance);

//         float xPos = col * horizontalDistance + ((row % 2) * horizontalDistance * 0.5f);
//         float zPos = row * verticalDistance;

//         return centerPosition + new Vector3(xPos, hexHeight, zPos);
//     }

//     private void DrawHexPreview(Vector3 center, Color color)
//     {
//         Color originalColor = Handles.color;
//         Handles.color = color;

//         Vector3[] vertices = new Vector3[6];
//         for (int i = 0; i < 6; i++)
//         {
//             // Ajustado para começar do lado plano (rotacionado 30 graus)
//             float angle = (i * 60f + 30f) * Mathf.Deg2Rad;
//             vertices[i] = center + new Vector3(
//                 hexRadius * Mathf.Cos(angle),
//                 0,
//                 hexRadius * Mathf.Sin(angle)
//             );
//         }

//         for (int i = 0; i < 6; i++)
//         {
//             Handles.DrawLine(vertices[i], vertices[(i + 1) % 6]);
//         }

//         Handles.color = originalColor;
//     }

//     private void PlaceHex(Vector3 position)
//     {
//         if (hexPrefab == null)
//         {
//             Debug.LogWarning("Hex Prefab não foi definido!");
//             return;
//         }

//         if (centerObject == null)
//         {
//             Debug.LogWarning("Center Object não foi definido!");
//             return;
//         }

//         // Verifica se já existe um hexágono na posição
//         foreach (Transform child in centerObject.transform)
//         {
//             if (Vector3.Distance(child.position, position) < 0.1f) // Pequena margem para precisão
//             {
//                 Undo.DestroyObjectImmediate(child.gameObject);
//                 Debug.Log($"Hex removido na posição {position}");
//                 return; // Sai da função, pois o hex foi removido
//             }
//         }

//         // Se não encontrou, coloca um novo hexágono
//         GameObject hex = PrefabUtility.InstantiatePrefab(hexPrefab, centerObject.transform) as GameObject;
//         hex.transform.position = position;

//         // Define um nome mais descritivo baseado na posição
//         Vector3 relativePos = position - centerObject.transform.position;
//         hex.name = $"Hex_{relativePos.x:F1}_{relativePos.z:F1}";

//         Undo.RegisterCreatedObjectUndo(hex, "Place Hex");
//         Debug.Log($"Hex colocado na posição {position}");
//     }


//     private void ClearAllHexes()
//     {
//         if (!EditorUtility.DisplayDialog("Confirmar Limpeza",
//             "Isso irá remover todos os hexágonos da cena. Tem certeza?",
//             "Sim", "Cancelar"))
//             return;

//         GameObject[] allHexes = GameObject.FindObjectsOfType<GameObject>();
//         foreach (GameObject obj in allHexes)
//         {
//             if (PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Regular &&
//                 PrefabUtility.GetCorrespondingObjectFromSource(obj) == hexPrefab)
//             {
//                 Undo.DestroyObjectImmediate(obj);
//             }
//         }
//     }

//     void OnEnable()
//     {
//         SceneView.duringSceneGui += OnSceneGUI;
//     }

//     void OnDisable()
//     {
//         SceneView.duringSceneGui -= OnSceneGUI;
//     }
// }