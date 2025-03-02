using UnityEngine;
using System.Collections;
using DG.Tweening;
using Mirror;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NetworkIdentity))]
public class Essent : NetworkBehaviour
{
    public EssentData data;
    public int tileIndexPosition;
    public HexTile currentTile;
    public PlayerObjectController playerOwner;

    public int totalEssence;
    public string essentName;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float arcHeight = 0.5f;

    public bool isMoving = false;

    private GameHudManager interfaceController;

    private void Start()
    {
        interfaceController = FindFirstObjectByType<GameHudManager>();

        if (interfaceController == null)
        {
            Debug.LogError("InGameInterfaceController não encontrado na cena!");
        }

        totalEssence = data.essence;
        essentName = data.essentName;
    }

    public bool IsAlive()
    {
        return totalEssence > 0;
    }

    public void ModifyEssence(int amount)
    {
        totalEssence += amount;
        Debug.Log($"{data.essentName} agora tem {totalEssence} de essência.");
    }

    public void UseSpecialAbility()
    {
        Debug.Log($"{essentName} usou sua habilidade especial!");
    }

    public void Initialize(HexTile startTile)
    {
        currentTile = startTile;
        transform.position = startTile.transform.position + Vector3.up;
        Debug.Log($"{essentName} começou no hexágono {currentTile.GetTileIndex()}.");
    }

    public List<int> GetInitialCardsIDs()
    {
        // Verifica se o EssentData e a lista de cartas iniciais existem
        if (data != null && data.initialCards != null)
        {
            // Retorna uma lista de IDs das cartas iniciais
            return data.initialCards.Select(card => card.id).ToList();
        }
        else
        {
            Debug.LogWarning("EssentData ou initialCards não encontrados!");
            return new List<int>(); // Retorna uma lista vazia se não houver cartas
        }
    }



    #region Board Moviment

    public void MoveNext(int numMoves)
    {
        if (isMoving || numMoves <= 0) return;

        StartCoroutine(MoveAlongRoute(numMoves));
    }

    public void MoveBack(int numMoves)
    {
        if (isMoving || numMoves <= 0) return;

        StartCoroutine(MoveBackAlongRoute(numMoves));
    }

    private IEnumerator MoveBackAlongRoute(int numMoves)
    {
        isMoving = true;
        int movesDone = 0;

        while (movesDone < numMoves)
        {
            HexTile previousTile = currentTile.GetPreviousHex();
            if (previousTile == null)
            {
                Debug.Log($"{data.essentName} atingiu o início da rota, ignorando {numMoves - movesDone} movimentos restantes.");
                break;
            }

            // Move o ídolo para o tile anterior
            yield return StartCoroutine(MoveSmoothly(previousTile, false));

            // Atualiza o tile atual
            currentTile = previousTile;
            movesDone++;
        }

        // Aplica o efeito do tile final após o movimento ser concluído
        if (currentTile != null)
        {
            currentTile.ExecuteTileEffect(this);
        }

        isMoving = false;
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
                Debug.Log($"{data.essentName} atingiu o final da rota, ignorando {numMoves - movesDone} movimentos restantes.");
                break;
            }

            // Move o ídolo para o próximo tile
            yield return StartCoroutine(MoveSmoothly(nextTile, true));

            // Atualiza o tile atual
            currentTile = nextTile;
            movesDone++;
        }

        // Aplica o efeito do tile final após o movimento ser concluído
        if (currentTile != null)
        {
            currentTile.ExecuteTileEffect(this);
        }

        isMoving = false;
    }

    private IEnumerator MoveSmoothly(HexTile targetTile, bool isForward)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = targetTile.transform.position + Vector3.up;
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

    #endregion
}