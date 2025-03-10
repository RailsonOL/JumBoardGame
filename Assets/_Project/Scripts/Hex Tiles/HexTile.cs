using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TileRegion
{
    Neutral,    // Zona Neutra
    Frozen,     // Zona Gélida
    Plains,     // Zona Planícies
    PVP,        // Zona de PVP
    Volcanic,   // Zona Vulcânica
    Abyssal     // Zona Abismal
}

public class HexTile : MonoBehaviour
{
    [SerializeField] private HexTile nextHex;
    [SerializeField] private HexTile previousHex;
    [SerializeField] private int tileIndex;
    [SerializeField] public bool isStartTile;
    [SerializeField] private Material tileMaterial;

    [Header("Region Settings")]
    [SerializeField] private TileRegion region = TileRegion.Neutral; // Região do tile

    [Header("Position Points")]
    [SerializeField] private List<Vector3> positionPoints = new List<Vector3>(); // Pontos de posicionamento
    [SerializeField] private bool showGizmos = false; // Mostrar ou ocultar Gizmos
    [SerializeField] private float circleRadius = 0.8f; // Raio do círculo de pontos
    [SerializeField] private int numberOfPoints = 6; // Número de pontos ao redor do centro
    [SerializeField] private float heightOffset = 1f; // Altura do topo do objeto

    private MeshRenderer hexModelRenderer;
    private List<Essent> essentsOnTile = new List<Essent>(); // Lista de Essents no tile

    private void Awake()
    {
        Transform hexModelTransform = transform.Find("HexModel");

        if (hexModelTransform != null)
        {
            hexModelRenderer = hexModelTransform.GetComponent<MeshRenderer>();

            if (hexModelRenderer != null)
            {
                UpdateMaterial();
            }
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Transform hexModelTransform = transform.Find("HexModel");

            if (hexModelTransform != null)
            {
                hexModelRenderer = hexModelTransform.GetComponent<MeshRenderer>();

                if (hexModelRenderer != null)
                {
                    UpdateMaterial();
                }
            }

            // Gera os pontos de posicionamento automaticamente
            GeneratePositionPoints();
        }
    }

    private void UpdateMaterial()
    {
        if (hexModelRenderer != null && tileMaterial != null)
        {
            hexModelRenderer.sharedMaterial = tileMaterial;
        }
    }

    /// <summary>
    /// Gera os pontos de posicionamento em um círculo ao redor do centro.
    /// </summary>
    private void GeneratePositionPoints()
    {
        positionPoints.Clear();

        // Adiciona o ponto central
        positionPoints.Add(Vector3.zero);

        // Calcula os pontos ao redor do centro
        float angleIncrement = 360f / numberOfPoints;

        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = angleIncrement * i;
            Vector3 pointPosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * circleRadius,
                heightOffset, // Usa o heightOffset para a altura
                Mathf.Sin(angle * Mathf.Deg2Rad) * circleRadius
            );
            positionPoints.Add(pointPosition);
        }
    }

    /// <summary>
    /// Tenta adicionar um Essent ao tile. Retorna true se houver espaço disponível.
    /// </summary>
    public bool TryAddEssent(Essent essent)
    {
        if (essentsOnTile.Count < positionPoints.Count)
        {
            essentsOnTile.Add(essent);
            UpdateEssentPositions();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove um Essent do tile.
    /// </summary>
    public void RemoveEssent(Essent essent)
    {
        essentsOnTile.Remove(essent);
        UpdateEssentPositions();
    }

    /// <summary>
    /// Atualiza as posições dos Essents no tile com base nos pontos de posicionamento.
    /// </summary>
    private void UpdateEssentPositions()
    {
        for (int i = 0; i < essentsOnTile.Count; i++)
        {
            Vector3 position = transform.position + positionPoints[i];
            position.y = transform.position.y + heightOffset; // Ajusta a altura com base no heightOffset
            essentsOnTile[i].transform.position = position;
        }
    }

    public virtual void ExecuteTileEffect(Essent essent)
    {
        Debug.Log($"Tile {tileIndex} ({region}): Nenhum efeito aplicado.");

        // Verifica se há outros Essents no mesmo tile e aplica dano
        DamageEssentOnTile(essent);
    }

    public void DamageEssentOnTile(Essent essent)
    {
        // Verifica se há outros Essents no mesmo tile
        Essent[] essentsOnTile = FindObjectsByType<Essent>(FindObjectsSortMode.None)
            .Where(e => e.currentTile == this && e != essent) // Exclui o próprio Essent
            .ToArray();

        if (essentsOnTile.Length > 0)
        {
            Debug.Log($"Há {essentsOnTile.Length} Essents no mesmo tile. Aplicando dano...");

            foreach (Essent otherEssent in essentsOnTile)
            {
                // Causa dano ao outro Essent
                otherEssent.ModifyEssence(-essent.damageToOthers);
                Debug.Log($"{essent.essentName} causou {essent.damageToOthers} de dano a {otherEssent.essentName}.");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Desenha os pontos de posicionamento no Editor
        Gizmos.color = Color.blue;
        foreach (Vector3 point in positionPoints)
        {
            Vector3 worldPosition = transform.position + point;
            worldPosition.y = transform.position.y + heightOffset; // Ajusta a altura no Gizmo
            Gizmos.DrawSphere(worldPosition, 0.1f);
        }

        // Desenha conexões com os tiles vizinhos
        if (nextHex != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextHex.transform.position);
        }

        if (previousHex != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, previousHex.transform.position);
        }

        // Destaca o tile inicial
        if (isStartTile)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }

    // Métodos de acesso
    public HexTile GetNextHex() => nextHex;
    public HexTile GetPreviousHex() => previousHex;
    public int GetTileIndex() => tileIndex;
    public bool IsStartTile() => isStartTile;
    public void SetNextHex(HexTile next) => nextHex = next;
    public void SetPreviousHex(HexTile prev) => previousHex = prev;
    public void SetTileIndex(int index) => tileIndex = index;
    public bool HasNextHex() => nextHex != null;
    public bool HasPreviousHex() => previousHex != null;
}