using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    [Header("Network Variables")]
    [SyncVar] public int currentPlayer;
    [SyncVar] private int numberOfPlayers;
    [SyncVar] private bool gameEnd = false;

    [Header("Game Objects")]
    [SerializeField] private PlayerObjectController[] players;
    [SerializeField] private DiceController dice;
    [SerializeField] private HexTile startingTile; // Referência para o tile inicial
    public GameObject pawnPrefab;
    public InGameInterfaceController interfaceC;

    [Header("Game Settings")]
    [SerializeField] private float turnDelay = 1f;

    // Network Manager reference
    private CustomNetworkManager manager;
    private CustomNetworkManager Manager => manager ??= NetworkManager.singleton as CustomNetworkManager;

    #region Unity Lifecycle

    private void Start()
    {
        if (!isServer) return;

        InitializeGame();
    }

    private void Update()
    {
        HandleSceneSetup();
        HandleDebugInput();
    }

    private void HandleSceneSetup()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (FindFirstObjectByType<Idol>() == null)
            {
                SpawnIdols();
            }
        }
    }

    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            dice.ViewDice();
        }
    }

    #endregion

    #region Game Initialization

    private void InitializeGame()
    {
        numberOfPlayers = Manager.GamePlayers.Count;
        players = Manager.GamePlayers.ToArray();

        if (startingTile == null)
        {
            // Usa o novo método FindObjectsByType, que é mais eficiente pois não precisa ordenar
            var allTiles = FindObjectsByType<HexTile>(FindObjectsSortMode.None);
            startingTile = allTiles.FirstOrDefault(tile => tile.isStartTile);

            if (startingTile == null)
            {
                Debug.LogError("No HexTile marked as start tile found in the scene!");
                return;
            }
        }
    }

    [Server]
    public void SpawnIdols()
    {
        if (!isServer) return;

        foreach (var player in players)
        {
            if (player == null)
            {
                Debug.LogError("Null player found during idol spawn!");
                continue;
            }

            SetupPlayerComponents(player);
            SpawnPlayerIdol(player);
        }

        RefreshStat();
        SetupFirstTurn();
    }

    private void SetupPlayerComponents(PlayerObjectController player)
    {
        var playerMovement = player.GetComponent<PlayerMovimentNetwork>();
        if (playerMovement != null)
        {
            playerMovement.gameController = this;
        }
    }

    private void SpawnPlayerIdol(PlayerObjectController player)
    {
        if (player.SelectedIdol == null)
        {
            Debug.LogError($"SelectedIdol is null for player {player.PlayerName}");
            return;
        }

        Vector3 spawnPosition = startingTile != null
            ? startingTile.transform.position + Vector3.up * 2f
            : player.transform.position;

        var spawnedIdol = Instantiate(
            player.SelectedIdol,
            spawnPosition,
            player.SelectedIdol.transform.rotation
        );

        spawnedIdol.playerOwner = player;

        if (startingTile != null)
        {
            spawnedIdol.Initialize(startingTile);
        }

        NetworkServer.Spawn(spawnedIdol.gameObject);
    }

    private void SetupFirstTurn()
    {
        currentPlayer = 0;
        players[currentPlayer].isOurTurn = true;
        UpdateTurnUI();
    }

    #endregion

    #region Turn Management

    [Command(requiresAuthority = false)]
    public void CmdRollDice()
    {
        if (CanPlayerRollDice())
        {
            StartCoroutine(RollAndMove());
        }
    }

    private bool CanPlayerRollDice()
    {
        return !players[currentPlayer].SelectedIdol.isMoving
            && !gameEnd
            && players[currentPlayer].SelectedIdol.IsAlive();
    }

    private IEnumerator RollAndMove()
    {
        // Roll dice and wait for result
        dice.RollDice();
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} throw the dice...");
        yield return new WaitUntil(() => dice.allDiceResult != 0);

        // Get result and start movement
        int moveAmount = dice.allDiceResult;
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is moving {moveAmount} tiles");

        // Move the idol
        var currentIdol = players[currentPlayer].SelectedIdol;
        if (currentIdol != null)
        {
            currentIdol.MoveHexes(moveAmount);
            yield return new WaitUntil(() => !currentIdol.isMoving);
        }

        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is stopped");
        yield return new WaitForSeconds(turnDelay);

        ProcessTurnEnd();
    }

    private void ProcessTurnEnd()
    {
        TurnResult();
        bool shouldRepeat = CheckForTurnRepeat();
        NextTurn(shouldRepeat);
    }

    public void TurnResult()
    {
        players[currentPlayer].numberOfTurns++;

        if (gameEnd)
        {
            HandleGameEnd();
        }
    }

    private bool CheckForTurnRepeat()
    {
        // Implement your logic for determining if a player should repeat their turn
        // For example, landing on a special tile or getting a specific dice result
        return true;
    }

    public void NextTurn(bool repeat)
    {
        RefreshStat();
        players[currentPlayer].isOurTurn = false;

        if (!repeat)
        {
            SelectNextValidPlayer();
        }

        players[currentPlayer].isOurTurn = true;
        UpdateTurnUI();
    }

    private void SelectNextValidPlayer()
    {
        int checkedPlayers = 0;

        do
        {
            currentPlayer = (currentPlayer + 1) % numberOfPlayers;
            checkedPlayers++;

            if (checkedPlayers >= numberOfPlayers)
            {
                gameEnd = true;
                break;
            }

        } while (players[currentPlayer].isEnd);
    }

    private void UpdateTurnUI()
    {
        interfaceC.RpcUpdateCurrentTurn($"Player {players[currentPlayer].PlayerName}'s turn");
    }

    #endregion

    #region Game State Management

    public void RefreshStat()
    {
        // Implement statistics update logic here
        // For example, update scores, positions, etc.
    }

    private void HandleGameEnd()
    {
        // Implement game end logic here
        // For example, show final scores, winner announcement, etc.
    }

    [Server]
    public void ForceGameEnd()
    {
        gameEnd = true;
        HandleGameEnd();
    }

    #endregion

    #region Utility Methods

    public PlayerObjectController GetCurrentPlayer()
    {
        return players[currentPlayer];
    }

    public bool IsGameEnded()
    {
        return gameEnd;
    }

    public int GetNumberOfPlayers()
    {
        return numberOfPlayers;
    }

    #endregion
}