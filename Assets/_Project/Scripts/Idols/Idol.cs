using UnityEngine;
using System.Collections;
using DG.Tweening;
using Mirror;

public class Idol : NetworkBehaviour
{
    public IdolData data;
    public int position;
    public HexTile currentTile;
    public PlayerObjectController playerOwner;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float arcHeight = 0.5f;

    public bool isMoving = false;

    public bool IsAlive()
    {
        return data.essence > 0;
    }

    public void ModifyEssence(int amount)
    {
        data.essence += amount;
        Debug.Log($"{data.idolName} agora tem {data.essence} de essência.");
    }

    public void UseSpecialAbility()
    {
        Debug.Log($"{data.idolName} usou sua habilidade especial!");
    }

    public void Initialize(HexTile startTile)
    {
        currentTile = startTile;
        transform.position = startTile.transform.position + Vector3.up * 2f;
        Debug.Log($"{data.idolName} começou no hexágono {currentTile.GetTileIndex()}.");
    }

    private IEnumerator MoveSmoothly(HexTile targetTile, bool isForward)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = targetTile.transform.position + Vector3.up * 2f;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * moveSpeed;
            float percentageComplete = elapsedTime;

            // Movimento horizontal com curva de suavização
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0f, 1f, percentageComplete));

            // Efeito de pulo
            float yOffset = Mathf.Sin(percentageComplete * Mathf.PI) * jumpHeight;

            // Arco entre hexágonos
            float arcOffset = (1f - Mathf.Abs(percentageComplete - 0.5f) * 2f) * arcHeight;

            currentPosition.y += yOffset + arcOffset;

            transform.position = currentPosition;

            yield return null;
        }

        transform.position = targetPosition;
        currentTile = targetTile;
        isMoving = false;
    }

    #region esse

    public void MoveNext(int numMoves)
    {
        if (isMoving || numMoves <= 0) return;

        StartCoroutine(MoveAlongRoute(numMoves));
    }

    private IEnumerator MoveAlongRoute(int numMoves)
    {
        isMoving = true;
        int movesDone = 0;

        while (movesDone < numMoves)
        {
            HexTile nextTile = currentTile.GetNextHex();
            if (nextTile == null)
            {
                Debug.Log($"{data.idolName} atingiu o final da rota, ignorando {numMoves - movesDone} movimentos restantes.");
                break;
            }

            yield return StartCoroutine(MoveSmoothly(nextTile, true));

            currentTile = nextTile;
            movesDone++;
        }

        isMoving = false;
    }

    #endregion

    // Função para mostrar os botões no Inspector (apenas para testes)
#if UNITY_EDITOR
    void OnGUI()
    {
        if (GUILayout.Button("Mover para o próximo Hexágono"))
        {
            MoveNext(1);
        }

        if (GUILayout.Button("Mover para o hexágono anterior"))
        {
            MoveNext(-1);
        }

        if (GUILayout.Button("Mover 3 casas para frente"))
        {
            MoveNext(5);
        }

        if (GUILayout.Button("Mover 2 casas para trás"))
        {
            MoveNext(-2);
        }
    }
#endif
}