using UnityEngine;
using System.Collections;
using DG.Tweening;
using Mirror;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformReliable))]
public class Essent : NetworkBehaviour
{
    public EssentData data;
    public int tileIndexPosition;
    public HexTile currentTile;
    public PlayerObjectController playerOwner;
    public GameObject essentModel;

    [SyncVar] public int selectedTargetID;
    [SyncVar] public int essentID;

    Animator essentAnimator;

    public int totalEssence;
    public string essentName;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float arcHeight = 0.5f;
    public int damageToOthers = 2; // Valor do dano causado a outros Essents no mesmo tile
    public event System.Action<int> OnEssenceChanged;

    public bool isMoving = false;

    private GameHudManager interfaceController;

    private void Start()
    {
        interfaceController = FindFirstObjectByType<GameHudManager>();

        // Verifica se essentModel não é nulo antes de acessar o componente Animator
        if (essentModel != null)
        {
            essentAnimator = essentModel.GetComponent<Animator>();
        }

        if (interfaceController == null)
        {
            Debug.LogError("InGameInterfaceController não encontrado na cena!");
        }

        totalEssence = data.essence;
        essentName = data.essentName;
        essentID = data.id;
    }

    public bool IsAlive()
    {
        return totalEssence > 0;
    }

    public void ModifyEssence(int amount)
    {
        totalEssence += amount;

        // Trigger the event if it's registered
        if (OnEssenceChanged != null)
        {
            int playerIndex = FindEssentIndex();
            if (playerIndex >= 0)
            {
                OnEssenceChanged.Invoke(playerIndex + 1); // +1 because the UI uses 1-based indices
            }
        }
    }

    private int FindEssentIndex()
    {
        if (GameManager.Instance == null) return -1;

        // Find this Essent's index in the essents array
        for (int i = 0; i < GameManager.Instance.players.Length; i++)
        {
            if (GameManager.Instance.players[i].SelectedEssent == this)
                return i;
        }

        return -1;
    }

    public void SetSelectedTarget(int essentTargetID)
    {
        selectedTargetID = essentTargetID;
    }

    // Nessa logica toda estou usando o ID do essent para selecionar o alvo,
    // nao tem como passar o Essent pela rede e por isso ficou assim
    // Se tiver Essents iguais em cena, dará problema, mas o jogo nao terá isso.
    // Se for alterado, essa logica deve ser revista
    [Command(requiresAuthority = false)]
    public void CmdSetSelectedTarget(int essentTargetID)
    {
        SetSelectedTarget(essentTargetID);
    }

    public int GetSelectedTargetID()
    {
        return selectedTargetID;
    }

    public void Initialize(HexTile startTile)
    {
        // Define o tile atual como o tile inicial
        currentTile = startTile;

        if (!currentTile.TryAddEssent(this))
        {
            Debug.LogError($"Não há espaço suficiente no tile {currentTile.GetTileIndex()} para adicionar o Essent {essentName}.");
            return;
        }

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

        // Move o ídolo para o próximo tile
        if (essentAnimator != null)
        {
            // Inicia a animação de início do pulo
            essentAnimator.SetTrigger("startJump");
            essentAnimator.SetBool("idleJump", true);
        }

        while (movesDone < numMoves)
        {
            HexTile nextTile = currentTile.GetNextHex();
            if (nextTile == null)
            {
                break;
            }

            yield return StartCoroutine(MoveSmoothly(nextTile, true));

            // Atualiza o tile atual
            currentTile = nextTile;
            movesDone++;
        }

        if (essentAnimator != null)
        {
            essentAnimator.SetBool("idleJump", false);
            essentAnimator.SetTrigger("endJump");
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
    }

    #endregion
}