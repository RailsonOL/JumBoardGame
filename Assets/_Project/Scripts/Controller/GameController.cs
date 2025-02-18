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
    public GameObject pawnPrefab;
    public InGameInterfaceController interfaceC;

    //Manager
    private CustomNetworkManager manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (manager == null)
            {
                manager = NetworkManager.singleton as CustomNetworkManager;
            }

            return manager;
        }
    }

    private struct KeyValuePlace
    {
        public int key;
        public int value;
    }


    void Start()
    {
        numberOfPlayers = Manager.GamePlayers.Count;

        //get all player objects in scene
        players = Manager.GamePlayers.ToArray();
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (FindFirstObjectByType<Pawn>() == null)
            {
                SpawnPawns();
            }
        }

        //Rolling dice and start movement
        // if (Input.GetKeyDown(KeyCode.Mouse0))
        // {
        //     if (FindObjectOfType<Pawn>() == null)
        //     {
        //         SpawnPawns();
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.V))
        {
            dice.ViewDice();
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRollDice()
    {
        if (!players[currentPlayer].pawn.isMoving) // if dice is not thrown
        {
            StartCoroutine(RollAndMove());
        }
    }

    IEnumerator RollAndMove()
    {
        dice.RollDice();
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} throw the dice...");

        yield return new WaitUntil(() => dice.allDiceResult != 0);
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is moving {dice.allDiceResult} tiles");

        // players[currentPlayer].pawn.MoveNext(route.wayPointsSorted, dice.allDiceResult);

        yield return new WaitUntil(() => !players[currentPlayer].pawn.isMoving);
        interfaceC.RpcUpdateGameStatus($"{players[currentPlayer].PlayerName} is stopped");

        yield return new WaitForSeconds(1f);
        TurnResult();
    }

    public void SpawnPawns()
    {
        // Vector3 spawnPointPos = route.wayPointsSorted[0].GetComponent<Transform>().position;

        for (int i = 0; i < numberOfPlayers; i++)
        {
            players[i].GetComponent<PlayerMovimentNetwork>().gameController = this;
            // Pawn pawnSpawned = Instantiate(pawnPrefab, spawnPointPos, pawnPrefab.transform.rotation).GetComponent<Pawn>();
            // pawnSpawned.playerOwner = players[i];
            // players[i].pawn = pawnSpawned;

            // NetworkServer.Spawn(pawnSpawned.gameObject);
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

    // If player reached end point
    bool CheckForEnd(int pointPos)
    {
        return true;
    }
}
