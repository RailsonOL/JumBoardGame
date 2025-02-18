using UnityEngine;

public class HexTile : MonoBehaviour
{
    [SerializeField] private HexTile nextHex; // Próximo hexágono
    [SerializeField] private HexTile previousHex; // Hexágono anterior
    [SerializeField] private int tileIndex; // Índice do hexágono (opcional)
    [SerializeField] private bool isStartTile; // É o hexágono inicial?

    // Getters para acessar os hexágonos conectados
    public HexTile GetNextHex() => nextHex;
    public HexTile GetPreviousHex() => previousHex;
    public int GetTileIndex() => tileIndex;
    public bool IsStartTile() => isStartTile;

    // Métodos para configurar conexões (usados pelo BoardManager)
    public void SetNextHex(HexTile next) => nextHex = next;
    public void SetPreviousHex(HexTile prev) => previousHex = prev;
    public void SetTileIndex(int index) => tileIndex = index;

    // Métodos auxiliares
    public bool HasNextHex() => nextHex != null;
    public bool HasPreviousHex() => previousHex != null;

    // Editor helper para visualizar conexões
    private void OnDrawGizmos()
    {
        // Desenha as conexões
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

        // Destaca visualmente os tiles especiais
        if (isStartTile)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
}
