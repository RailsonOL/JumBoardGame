using UnityEngine;
using System.Collections.Generic;

public class HexGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    public float hexSize = 1f;
    public float gapSize = 0.1f;
    public Color gizmoColor = Color.white;
    public GameObject hexPrefab;

    // Adicionando uma flag para mostrar gizmos durante o jogo
    public bool showGizmosInGame = true;

    private float horizontalDistance;
    private float verticalDistance;
    private Dictionary<Vector2Int, GameObject> placedHexes = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        CalculateDistances();
    }

    void CalculateDistances()
    {
        horizontalDistance = hexSize * Mathf.Sqrt(3f) + gapSize;
        verticalDistance = hexSize * 1.5f + gapSize;
    }

    void OnDrawGizmos()
    {
        DrawAllHexGizmos();
    }

    // Novo método para desenhar gizmos também durante o jogo
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && showGizmosInGame)
        {
            DrawAllHexGizmos();
        }
    }

    // Método separado para desenhar todos os gizmos
    void DrawAllHexGizmos()
    {
        if (!Application.isPlaying)
        {
            CalculateDistances();
        }

        Gizmos.color = gizmoColor;
        Vector3 basePosition = transform.position;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = CalculateHexPosition(x, y) + basePosition;
                DrawHexGizmo(position);
            }
        }
    }

    Vector3 CalculateHexPosition(int x, int y)
    {
        float xPos = x * horizontalDistance;
        float yPos = y * verticalDistance;

        if (y % 2 != 0)
        {
            xPos += horizontalDistance * 0.5f;
        }

        float centerOffsetX = (width * horizontalDistance) * 0.5f;
        float centerOffsetY = (height * verticalDistance) * 0.5f;

        return new Vector3(xPos - centerOffsetX, 0, yPos - centerOffsetY);
    }

    void DrawHexGizmo(Vector3 center)
    {
        Vector3[] vertices = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = (i * 60f + 30f) * Mathf.Deg2Rad;
            vertices[i] = center + new Vector3(
                hexSize * Mathf.Cos(angle),
                0,
                hexSize * Mathf.Sin(angle)
            );
        }

        for (int i = 0; i < 6; i++)
        {
            Gizmos.DrawLine(vertices[i], vertices[(i + 1) % 6]);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleHexClick();
        }
        // Adicionando verificação para a tecla espaço
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleHexHover();
        }
    }

    // Novo método para lidar com o hover + tecla espaço
    void HandleHexHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPoint = hit.point - transform.position;
            Vector2Int gridPosition = WorldToGridPosition(hitPoint);

            if (IsValidGridPosition(gridPosition))
            {
                PlaceHex(gridPosition);
            }
        }
    }

    void HandleHexClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPoint = hit.point - transform.position;
            Vector2Int gridPosition = WorldToGridPosition(hitPoint);

            if (IsValidGridPosition(gridPosition))
            {
                PlaceHex(gridPosition);
            }
        }
    }

    Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        float gridX = worldPosition.x + (width * horizontalDistance * 0.5f);
        float gridY = worldPosition.z + (height * verticalDistance * 0.5f);

        float x = gridX / horizontalDistance;
        float y = gridY / verticalDistance;

        if (Mathf.RoundToInt(y) % 2 != 0)
        {
            x -= 0.5f;
        }

        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < width &&
               position.y >= 0 && position.y < height;
    }

    void PlaceHex(Vector2Int gridPosition)
    {
        if (placedHexes.ContainsKey(gridPosition))
        {
            Destroy(placedHexes[gridPosition]);
            placedHexes.Remove(gridPosition);
        }
        else if (hexPrefab != null)
        {
            Vector3 worldPosition = CalculateHexPosition(gridPosition.x, gridPosition.y) + transform.position;
            GameObject hex = Instantiate(hexPrefab, worldPosition, Quaternion.Euler(90, 0, 0));
            hex.transform.parent = transform;
            placedHexes.Add(gridPosition, hex);
        }
    }

    
}