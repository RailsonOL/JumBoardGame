using UnityEngine;

public class HexTile : MonoBehaviour
{
    [SerializeField] private HexTile nextHex; // Próximo hexágono
    [SerializeField] private HexTile previousHex; // Hexágono anterior
    [SerializeField] private int tileIndex; // Índice do hexágono (opcional)
    [SerializeField] private bool isStartTile; // É o hexágono inicial?
    [SerializeField] private bool isEndTile; // É o hexágono final?

    // Getters para acessar os hexágonos conectados
    public HexTile GetNextHex() => nextHex;
    public HexTile GetPreviousHex() => previousHex;
    public int GetTileIndex() => tileIndex;
    public bool IsStartTile() => isStartTile;
    public bool IsEndTile() => isEndTile;

    // Método para verificar se o hexágono está conectado a outro
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
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        if (isEndTile)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}