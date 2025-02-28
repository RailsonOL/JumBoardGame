using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    // Evento para notificar mudanças no jogador atual
    public event Action OnCurrentPlayerChanged;

    [SyncVar(hook = nameof(OnCurrentPlayerChanged_Hook))]
    public int currentPlayer;
    [SyncVar] int numberOfPlayers;
    [SyncVar] bool gameEnd = false;

    [Header("Game Objects")]
    [SerializeField] PlayerObjectController[] players;
    [SerializeField] DiceController dice;
    public GameObject essentPrefab;

    [Header("Player Hand Panel")]
    [SerializeField] private HexTile startingTile;

    private CustomNetworkManager manager;
    private CustomNetworkManager Manager => manager ??= NetworkManager.singleton as CustomNetworkManager;

    // Hook para quando o currentPlayer mudar na network
    void OnCurrentPlayerChanged_Hook(int oldValue, int newValue)
    {
        Debug.Log($"Current player changed from {oldValue} to {newValue}");
        OnCurrentPlayerChanged?.Invoke();

        // Notifica os clientes sobre o novo Essent a ser seguido
        UpdateCameraTargetForClients();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Garante que exista apenas uma instância
        }
        else
        {
            Instance = this;
            if (isServer)
            {
                // Garantir que o GameController seja spawnado no servidor
                NetworkServer.Spawn(gameObject);
            }
        }
    }

    void Start()
    {
        numberOfPlayers = Manager.GamePlayers.Count;

        // Obtém todos os objetos de jogador na cena
        players = Manager.GamePlayers.ToArray();
        InitializeGame();
    }

    private void InitializeGame()
    {
        numberOfPlayers = Manager.GamePlayers.Count;
        players = Manager.GamePlayers.ToArray();

        if (startingTile == null)
        {
            var allTiles = FindObjectsByType<HexTile>(FindObjectsSortMode.None);
            startingTile = allTiles.FirstOrDefault(tile => tile.isStartTile);

            if (startingTile == null)
            {
                Debug.LogError("No HexTile marked as start tile found in the scene!");
                return;
            }
        }

        if (isServer)
        {
            SpawnEssents();
        }
    }

    // Método para obter o PlayerController atual
    public PlayerObjectController GetCurrentPlayerController()
    {
        if (players != null && currentPlayer >= 0 && currentPlayer < players.Length)
        {
            return players[currentPlayer];
        }
        return null;
    }

    [Command(requiresAuthority = false)]
    public void CmdRollDice(NetworkConnectionToClient sender = null)
    {
        // Verifica se o jogador que solicitou é o jogador atual
        if (sender == players[currentPlayer].connectionToClient)
        {
            if (!players[currentPlayer].SelectedEssent.isMoving)
            {
                StartCoroutine(RollAndMove());
            }
        }
    }

    // Executa o efeito de uma carta
    [Command(requiresAuthority = false)]
    public void CmdExecuteCardEffectByID(int cardID, NetworkConnectionToClient sender = null)
    {
        // Verifica se o jogador que solicitou é o jogador atual
        if (sender == players[currentPlayer].connectionToClient)
        {
            if (players[currentPlayer] != null)
            {
                Card cardFromManager = CardManager.Instance.GetCardById(cardID);
                if (cardFromManager != null)
                {
                    cardFromManager.Execute(players[currentPlayer].SelectedEssent);
                }
                else
                {
                    Debug.LogWarning($"Carta com ID {cardID} não encontrada.");
                }
            }
            else
            {
                Debug.LogWarning("Não foi possível executar o efeito da carta, porque SelectedEssent não está disponível.");
            }
        }
    }

    IEnumerator RollAndMove()
    {
        // Jogador rola o dado
        dice.RollDice();
        GameHudManager.Inst.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} throw the dice...");

        // Aguarda até que o resultado dos dados esteja disponível
        yield return new WaitUntil(() => dice.allDiceResult != 0);
        int moveAmount = dice.allDiceResult;

        // Atualiza a interface com o movimento do jogador
        GameHudManager.Inst.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is moving {moveAmount} tiles");

        // Move o Essent do jogador atual
        var currentEssent = players[currentPlayer].SelectedEssent;
        if (currentEssent != null)
        {
            currentEssent.MoveNext(moveAmount);
        }

        // Aguarda até que o movimento termine
        yield return new WaitUntil(() => !players[currentPlayer].SelectedEssent.isMoving);

        // Atualiza status e processa o resultado do turno
        GameHudManager.Inst.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is stopped");
        yield return new WaitForSeconds(1f);

        TurnResult();
    }

    public void SpawnEssents()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            PlayerObjectController player = players[i].GetComponent<PlayerObjectController>();
            Essent essentSpawned = Instantiate(essentPrefab, player.transform.position, essentPrefab.transform.rotation).GetComponent<Essent>();
            essentSpawned.playerOwner = player;
            player.SelectedEssent = essentSpawned;

            essentSpawned.Initialize(startingTile);
            NetworkServer.Spawn(essentSpawned.gameObject);

            player.SelectedEssentNetId = essentSpawned.netId;

            // Obtenha a lista de IDs de cartas do Essent
            List<int> initialCardIds = essentSpawned.GetInitialCardsIDs();

            Debug.Log($"Enviando cartas iniciais para o jogador {player.PlayerName}...");
            // Envie a lista de IDs de cartas para o cliente
            player.TargetInitializeHand(player.connectionToClient, initialCardIds);

            if (player.TryGetComponent<PlayerHand>(out var playerHand))
            {
                Debug.Log("PlayerHand encontrado");

            }
            else
            {
                Debug.LogWarning("PlayerHand não está configurado corretamente.");
            }
        }

        RefreshStat();

        currentPlayer = 0;
        players[currentPlayer].isOurTurn = true;

        StartCoroutine(WaitForAllPlayersReady());
    }

    private IEnumerator WaitForAllPlayersReady()
    {
        Debug.Log($"Aguardando todos os jogadores. Total: {numberOfPlayers}");
        yield return new WaitUntil(() => AreAllPlayersReady());

        Debug.Log("Todos os jogadores estão prontos, atualizando UI e câmera...");
        GameHudManager.Inst.RpcUpdateCurrentTurn($"Player {players[currentPlayer].PlayerName}'s turn");
        UpdateCameraTargetForClients();
    }

    private bool AreAllPlayersReady()
    {
        int readyCount = players.Count(p => p.readyToPlay);
        Debug.Log($"Jogadores prontos: {readyCount}/{numberOfPlayers}");
        return readyCount == numberOfPlayers;
    }

    // check info about standing way point and recording stat
    public void TurnResult()
    {
        players[currentPlayer].numberOfTurns++;

        if (gameEnd)
        {
            //ShowStat();
        }

        NextTurn(false);
    }

    // prepare for the next turn
    public void NextTurn(bool repeat)
    {
        RefreshStat();
        players[currentPlayer].isOurTurn = false;
        if (repeat)
        {
            // we simply dont change current player
        }
        else
        {
            int iter = 0;

            do
            {
                currentPlayer++;

                if ((currentPlayer + 1) > numberOfPlayers)
                {
                    currentPlayer = 0;
                }

                // If one of the player ending the game
                if (players[currentPlayer].isEnd)
                {
                    iter++;
                    // If all of the players ended the game
                    if (iter >= numberOfPlayers)
                    {
                        gameEnd = true;
                        break;
                    }
                }
                else
                {
                    break;
                }

            } while (true);
        }
        players[currentPlayer].isOurTurn = true;

        // Set new info about next turn player
        Debug.Log($"Player {players[currentPlayer].PlayerName} turn");
        GameHudManager.Inst.RpcUpdateCurrentTurn($"Player {players[currentPlayer].PlayerName}'s turn");

        // Notifica os clientes sobre o novo Essent a ser seguido
        UpdateCameraTargetForClients();
    }

    public Essent GetCurrentPlayerEssent()
    {
        var playerController = GetCurrentPlayerController();
        if (playerController != null)
        {
            return playerController.SelectedEssent;
        }
        return null;
    }

    // Notifica os clientes sobre o Essent a ser seguido
    [ClientRpc]
    private void RpcUpdateCameraTarget(uint essentNetId)
    {
        if (NetworkClient.spawned.TryGetValue(essentNetId, out NetworkIdentity identity))
        {
            Essent essent = identity.GetComponent<Essent>();
            if (essent != null)
            {
                CameraFollow.Instance?.UpdateTarget(essent.gameObject);
            }
        }
    }

    // Método para chamar o RpcUpdateCameraTarget quando o turno muda
    private void UpdateCameraTargetForClients()
    {
        var currentEssent = GetCurrentPlayerEssent();
        if (currentEssent != null)
        {
            RpcUpdateCameraTarget(currentEssent.netId);
        }
    }

    //Statistics table update 
    public void RefreshStat()
    {
        // Implementação existente...
    }
}