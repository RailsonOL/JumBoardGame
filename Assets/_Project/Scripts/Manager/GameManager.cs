using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    #region Variables
    [Header("Game State")]
    [SyncVar(hook = nameof(OnCurrentPlayerChanged_Hook))]
    public int currentPlayer;
    [SyncVar] public int numberOfPlayers;
    [SyncVar] private bool gameEnd = false;

    [Header("Player Management")]
    [SerializeField] public PlayerObjectController[] players; // clients have access to this array
    [SerializeField] private DiceController dice;
    [SerializeField] private List<GameObject> essentPrefabs; // List of Essent prefabs
    [SerializeField] private HexTile startingTile;

    [SyncVar] private int cardsUsedThisTurn = 0;
    [SyncVar] private bool hasRolledDiceThisTurn = false;
    public int maxCardsPerTurn = 1; // Maximum number of cards that can be used per turn

    private CustomNetworkManager manager;
    private CustomNetworkManager Manager => manager ??= NetworkManager.singleton as CustomNetworkManager;

    // Event to notify changes in the current player
    public event Action OnCurrentPlayerChanged;
    #endregion

    #region Singleton and Initialization
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
        else
        {
            Instance = this;
            if (isServer)
            {
                NetworkServer.Spawn(gameObject); // Ensure GameManager is spawned on the server
            }
        }
    }

    private void Start()
    {
        numberOfPlayers = Manager.GamePlayers.Count;
        players = Manager.GamePlayers.ToArray();
        InitializeGame();
    }
    #endregion

    #region Player Management
    private void OnCurrentPlayerChanged_Hook(int oldValue, int newValue)
    {
        Debug.Log($"Current player changed from {oldValue} to {newValue}");
        OnCurrentPlayerChanged?.Invoke();
        UpdateCameraTargetForClients();
    }

    public PlayerObjectController GetCurrentPlayerController()
    {
        if (players != null && currentPlayer >= 0 && currentPlayer < players.Length)
        {
            return players[currentPlayer];
        }
        return null;
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

    #endregion

    #region Game Initialization
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

    private IEnumerator WaitForAllPlayersReady()
    {
        Debug.Log($"Waiting for all players. Total: {numberOfPlayers}");
        yield return new WaitUntil(() => AreAllPlayersReady());

        Debug.Log("All players are ready, updating UI and camera...");
        GameHudManager.Instance.RpcUpdateCurrentTurn($"Player {players[currentPlayer].PlayerName}'s turn");
        UpdateCameraTargetForClients();

        GameHudManager.Instance.RpcActivatePlayerEssentStatus(numberOfPlayers);
        RefreshStat();
    }

    private bool AreAllPlayersReady()
    {
        int readyCount = players.Count(p => p.readyToPlay);
        Debug.Log($"Ready players: {readyCount}/{numberOfPlayers}");
        return readyCount == numberOfPlayers;
    }
    #endregion

    #region Essent Spawning
    public void SpawnEssents()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            PlayerObjectController player = players[i].GetComponent<PlayerObjectController>();

            int selectedEssentId = player.SelectedEssentId;
            GameObject selectedPrefab = essentPrefabs.FirstOrDefault(prefab =>
            {
                Essent essentComponent = prefab.GetComponent<Essent>();
                return essentComponent != null && essentComponent.data != null && essentComponent.data.id == selectedEssentId;
            });

            if (selectedPrefab == null)
            {
                Debug.LogError($"No Essent prefab found with ID {selectedEssentId} for player {player.PlayerName}. Using first prefab as fallback.");
                selectedPrefab = essentPrefabs[0]; // Fallback to the first prefab if not found
            }

            Essent essentSpawned = Instantiate(selectedPrefab, player.transform.position, selectedPrefab.transform.rotation).GetComponent<Essent>();
            essentSpawned.playerOwner = player;
            player.SelectedEssent = essentSpawned;

            // Pass the player index to the Initialize method
            essentSpawned.Initialize(startingTile, i);
            essentSpawned.OnEssenceChanged += OnEssentEssenceChanged;
            NetworkServer.Spawn(essentSpawned.gameObject);

            player.SelectedEssentNetId = essentSpawned.netId;

            List<int> initialCardIds = essentSpawned.GetInitialCardsIDs();
            Debug.Log($"Sending initial cards to player {player.PlayerName}...");
            player.TargetInitializeHand(player.connectionToClient, initialCardIds);

            if (player.TryGetComponent<PlayerHand>(out var playerHand))
            {
                Debug.Log("PlayerHand found");
            }
            else
            {
                Debug.LogWarning("PlayerHand is not properly configured.");
            }
        }

        RefreshStat();

        currentPlayer = 0;
        players[currentPlayer].isOurTurn = true;

        StartCoroutine(WaitForAllPlayersReady());
    }

    private void OnEssentEssenceChanged(int playerIndex)
    {
        if (GameHudManager.Instance != null && playerIndex > 0 && playerIndex <= numberOfPlayers)
        {
            Essent essent = players[playerIndex - 1].SelectedEssent;
            if (essent != null)
            {
                string essentName = essent.essentName;
                string currentEssence = essent.totalEssence.ToString();
                GameHudManager.Instance.RpcUpdatePlayerEssentStatus(playerIndex, essentName, currentEssence);
            }
        }
    }
    #endregion

    #region Turn Management
    public void OnEndTurnButtonPressed()
    {
        CmdEndTurn();
    }

    [Command(requiresAuthority = false)]
    public void CmdEndTurn(NetworkConnectionToClient sender = null)
    {
        if (sender == players[currentPlayer].connectionToClient)
        {
            EndTurn();
        }
    }

    private void EndTurn()
    {
        Debug.Log("EndTurn called");
        cardsUsedThisTurn = 0;
        hasRolledDiceThisTurn = false;
        TurnResult();
    }

    public void NextTurn(bool repeat)
    {
        players[currentPlayer].isOurTurn = false;

        if (!repeat)
        {
            int iter = 0;
            do
            {
                currentPlayer++;
                if ((currentPlayer + 1) > numberOfPlayers)
                {
                    currentPlayer = 0;
                }

                if (players[currentPlayer].isEnd)
                {
                    iter++;
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
        Debug.Log($"Player {players[currentPlayer].PlayerName} turn");
        GameHudManager.Instance.RpcUpdateCurrentTurn($"Player {players[currentPlayer].PlayerName}'s turn");
        UpdateCameraTargetForClients();
    }

    public void TurnResult()
    {
        players[currentPlayer].numberOfTurns++;

        if (gameEnd)
        {
            // ShowStat();
        }

        RefreshStat();
        NextTurn(false);
    }
    #endregion

    #region Player Actions

    public void OnRollDiceButtonPressed()
    {
        CmdRollDice();
    }

    [Command(requiresAuthority = false)]
    public void CmdRollDice(NetworkConnectionToClient sender = null)
    {
        if (sender == players[currentPlayer].connectionToClient && CanPerformAction(false))
        {
            if (!players[currentPlayer].SelectedEssent.isMoving)
            {
                StartCoroutine(RollAndMove());
                hasRolledDiceThisTurn = true;
            }
        }
    }

    private IEnumerator RollAndMove()
    {
        dice.RollDice();
        GameHudManager.Instance.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} throw the dice...");

        yield return new WaitUntil(() => dice.allDiceResult != 0);
        int moveAmount = dice.allDiceResult;

        GameHudManager.Instance.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is moving {moveAmount} tiles");

        var currentEssent = players[currentPlayer].SelectedEssent;
        if (currentEssent != null)
        {
            currentEssent.MoveNext(moveAmount);
        }

        yield return new WaitUntil(() => !players[currentPlayer].SelectedEssent.isMoving);

        GameHudManager.Instance.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is stopped");
        yield return new WaitForSeconds(1f);
    }

    [Command(requiresAuthority = false)]
    public void CmdExecuteCardEffectByID(int cardID, NetworkConnectionToClient sender = null)
    {
        if (sender == players[currentPlayer].connectionToClient && CanPerformAction(true))
        {
            if (players[currentPlayer] != null)
            {
                Card cardFromManager = CardManager.Instance.GetCardById(cardID);
                if (cardFromManager != null)
                {
                    players[currentPlayer].TargetRemoveCardFromHand(cardID);
                    cardFromManager.Execute(players[currentPlayer].SelectedEssent);
                    cardsUsedThisTurn++;
                }
                else
                {
                    Debug.LogWarning($"Card with ID {cardID} not found.");
                }
            }
            else
            {
                Debug.LogWarning("Cannot execute card effect because SelectedEssent is not available.");
            }
        }
    }

    public bool CanPerformAction(bool isCardAction)
    {
        if (isCardAction)
        {
            return cardsUsedThisTurn < maxCardsPerTurn;
        }
        else
        {
            return !hasRolledDiceThisTurn;
        }
    }
    #endregion

    #region Networking and Communication
    [TargetRpc]
    public void TargetReceiveCard(NetworkConnection target, int cardId)
    {
        PlayerObjectController player = target.identity.GetComponent<PlayerObjectController>();
        if (player != null)
        {
            player.ReciveCardByID(cardId);
        }
        else
        {
            Debug.LogWarning("PlayerObjectController not found on client.");
        }
    }

    public void SendCardToPlayer(int playerId, int cardId)
    {
        int adjustedPlayerId = playerId - 1;

        if (adjustedPlayerId >= 0 && adjustedPlayerId < players.Length)
        {
            PlayerObjectController player = players[adjustedPlayerId];
            if (player != null)
            {
                TargetReceiveCard(player.connectionToClient, cardId);
            }
            else
            {
                Debug.LogWarning($"Player with ID {playerId} not found.");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid player ID: {playerId}");
        }
    }

    [ClientRpc]
    private void RpcUpdateCameraTarget(uint essentNetId) // Run on all clients
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

    private void UpdateCameraTargetForClients()
    {
        var currentEssent = GetCurrentPlayerEssent();
        if (currentEssent != null)
        {
            RpcUpdateCameraTarget(currentEssent.netId);
        }
    }
    #endregion

    #region Statistics and UI
    public void RefreshStat()
    {
        if (players == null || GameHudManager.Instance == null)
        {
            Debug.LogWarning("Players or GameHudManager not available.");
            return;
        }

        for (int i = 0; i < players.Length; i++)
        {
            PlayerObjectController player = players[i];
            if (player != null && player.SelectedEssent != null)
            {
                string essentName = player.SelectedEssent.essentName;
                string currentEssence = player.SelectedEssent.totalEssence.ToString();
                GameHudManager.Instance.RpcUpdatePlayerEssentStatus(i + 1, essentName, currentEssence);
            }
            else
            {
                Debug.LogWarning($"Player {i + 1} or their Essent is not available.");
            }
        }
    }
    #endregion
}