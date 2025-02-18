// using UnityEngine;
// using UnityEditor;

// public class CustomFlythroughMode : EditorWindow
// {
//     private bool isFlythroughActive = false;
//     private float flySpeed = 1.0f;

//     private bool isSpacePressed = false;
//     private bool isCtrlPressed = false;

//     [MenuItem("Tools/Custom Flythrough Mode")]
//     public static void ShowWindow()
//     {
//         GetWindow<CustomFlythroughMode>("Custom Flythrough");
//     }

//     private void OnGUI()
//     {
//         GUILayout.Label("Custom Flythrough Mode", EditorStyles.boldLabel);

//         if (GUILayout.Button(isFlythroughActive ? "Disable Flythrough Mode" : "Enable Flythrough Mode"))
//         {
//             ToggleFlythroughMode();
//         }

//         flySpeed = EditorGUILayout.FloatField("Fly Speed", flySpeed);
//     }

//     private void ToggleFlythroughMode()
//     {
//         isFlythroughActive = !isFlythroughActive;

//         if (isFlythroughActive)
//         {
//             SceneView.duringSceneGui += DuringSceneGUI;
//             Debug.Log("Flythrough Mode Activated");
//         }
//         else
//         {
//             SceneView.duringSceneGui -= DuringSceneGUI;
//             Debug.Log("Flythrough Mode Deactivated");
//         }
//     }

//     private void DuringSceneGUI(SceneView sceneView)
//     {
//         Event e = Event.current;

//         // Verifica se o botão direito do mouse está pressionado
//         if (e.type == EventType.MouseDown && e.button == 1)
//         {
//             isFlythroughActive = true;
//         }

//         // Verifica se as teclas estão pressionadas
//         if (e.type == EventType.KeyDown)
//         {
//             if (e.keyCode == KeyCode.Space)
//             {
//                 isSpacePressed = true;
//             }
//             else if (e.keyCode == KeyCode.LeftControl)
//             {
//                 isCtrlPressed = true;
//             }
//         }

//         // Verifica se as teclas foram soltas
//         if (e.type == EventType.KeyUp)
//         {
//             if (e.keyCode == KeyCode.Space)
//             {
//                 isSpacePressed = false;
//             }
//             else if (e.keyCode == KeyCode.LeftControl)
//             {
//                 isCtrlPressed = false;
//             }
//         }

//         // Aplica o movimento contínuo
//         if (isFlythroughActive)
//         {
//             // Desativa o atalho Shift + Espaço
//             if (e.type == EventType.KeyDown && e.shift && e.keyCode == KeyCode.Space)
//             {
//                 e.Use(); // Consome o evento para evitar o comportamento padrão
//             }

//             // Subir com Espaço (movimento contínuo)
//             if (isSpacePressed)
//             {
//                 MoveCamera(sceneView, Vector3.up);
//             }

//             // Descer com Ctrl (movimento contínuo)
//             if (isCtrlPressed)
//             {
//                 MoveCamera(sceneView, Vector3.down);
//             }
//         }
//     }

//     private void MoveCamera(SceneView sceneView, Vector3 direction)
//     {
//         // Aplica o movimento à câmera
//         sceneView.pivot += direction * flySpeed * 0.02f; // Ajuste o multiplicador para controlar a suavidade
//         sceneView.Repaint(); // Atualiza a cena
//     }
// }