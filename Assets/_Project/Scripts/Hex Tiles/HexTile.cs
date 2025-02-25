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

    private MeshRenderer hexModelRenderer;

    private void Awake()
    {
        hexModelRenderer = transform.Find("HexModel").GetComponent<MeshRenderer>();
        UpdateMaterial();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            hexModelRenderer = transform.Find("HexModel").GetComponent<MeshRenderer>();
            UpdateMaterial();
        }
    }

    private void UpdateMaterial()
    {
        if (hexModelRenderer != null && tileMaterial != null)
        {
            hexModelRenderer.sharedMaterial = tileMaterial;
        }
    }

    public HexTile GetNextHex() => nextHex;
    public HexTile GetPreviousHex() => previousHex;
    public int GetTileIndex() => tileIndex;
    public bool IsStartTile() => isStartTile;
    public void SetNextHex(HexTile next) => nextHex = next;
    public void SetPreviousHex(HexTile prev) => previousHex = prev;
    public void SetTileIndex(int index) => tileIndex = index;
    public bool HasNextHex() => nextHex != null;
    public bool HasPreviousHex() => previousHex != null;

    public virtual void ExecuteTileEffect(Essent essent)
    {
        Debug.Log($"Tile {tileIndex} ({region}): Nenhum efeito aplicado.");
    }

    private void OnDrawGizmos()
    {
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

        if (isStartTile)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}