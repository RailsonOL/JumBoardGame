using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    [SyncVar] public int currentPlayer;
    [SyncVar] int numberOfPlayers;
    [SyncVar] bool gameEnd = false;

    [Header("Game Objects")]
    [SerializeField] PlayerObjectController[] players;
    [SerializeField] DiceController dice;
    public GameObject essentPrefab;

    [Header("Player Hand Panel")]
    public RectTransform playerHandPanel; // Referência ao painel que será usado pelo PlayerHand
    public GameObject activationPanel; // Referência ao activationPanel
    public InGameInterfaceController interfaceC;
    [SerializeField] private HexTile startingTile;

    private CustomNetworkManager manager;
    private CustomNetworkManager Manager => manager ??= NetworkManager.singleton as CustomNetworkManager;

    void Start()
    {
        numberOfPlayers = Manager.GamePlayers.Count;

        //get all player objects in scene
        players = Manager.GamePlayers.ToArray();
        InitializeGame();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (FindFirstObjectByType<Essent>() == null)
            {
                SpawnEssents();
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            dice.ViewDice();
        }
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
    }


    [Command(requiresAuthority = false)]
    public void CmdRollDice()
    {
        if (!players[currentPlayer].SelectedEssent.isMoving) // if dice is not thrown
        {
            StartCoroutine(RollAndMove());
        }
    }

    IEnumerator RollAndMove()
    {
        dice.RollDice();
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} throw the dice...");

        yield return new WaitUntil(() => dice.allDiceResult != 0);
        int moveAmount = dice.allDiceResult;
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is moving {moveAmount} tiles");

        // Obtém o Essent do jogador atual e move
        var currentEssent = players[currentPlayer].SelectedEssent;
        if (currentEssent != null)
        {
            Debug.Log("Chegou aqui");
            currentEssent.MoveNext(moveAmount);
        }

        yield return new WaitUntil(() => !players[currentPlayer].SelectedEssent.isMoving);
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is stopped");

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

        // Configura o PlayerHand do jogador
        PlayerHand playerHand = player.GetComponentInChildren<PlayerHand>();
        if (playerHand != null && playerHandPanel != null)
        {
            player.gameController = this;
            playerHand.handPanel = playerHandPanel;
            playerHand.activationPanel = activationPanel;
            playerHand.InitializeHand(); // Inicializa as cartas após o painel estar disponível
        }
        else
        {
            Debug.LogWarning("PlayerHand ou playerHandPanel não está configurado corretamente.");
        }

        essentSpawned.Initialize(startingTile);
        NetworkServer.Spawn(essentSpawned.gameObject);
    }

    RefreshStat();

    currentPlayer = 0;
    players[currentPlayer].isOurTurn = true;
    interfaceC.RpcUpdateCurrentTurn($"Player {players[currentPlayer].PlayerName}'s turn");
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
        interfaceC.RpcUpdateCurrentTurn($"Player {players[currentPlayer].PlayerName}'s turn");
    }


    //Statistics table update 
    public void RefreshStat()
    {

    }
}