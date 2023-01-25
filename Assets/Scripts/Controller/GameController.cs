using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static WayPointData;
using Mirror;

public class GameController : NetworkBehaviour
{
    [SyncVar] public int currentPlayer;
    [SyncVar] int numberOfPlayers;
    [SyncVar] bool gameEnd = false;

    [Header("Game Objects")]
    [SerializeField] Route route;
    [SerializeField] PlayerObjectController[] players;
    [SerializeField] DiceController dice;
    public GameObject pawnPrefab;

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
        //Rolling dice and start movement
        if (Input.GetKeyDown(KeyCode.Mouse0) && (!gameEnd))
        {
            if (FindObjectOfType<Pawn>() == null)
            {
                SpawnPawns();
            }
        }

        // Stat table
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //ShowStat();
            StartCoroutine(RollAndMove());
        }
    }

    IEnumerator RollAndMove()
    {
        dice.RollDice();

        yield return new WaitUntil(() => dice.allDiceResult != 0);

        players[currentPlayer].pawn.MoveNext(route.wayPointsSorted, dice.allDiceResult);

        yield return new WaitUntil(() => !players[currentPlayer].pawn.isMoving);

        TurnResult();
    }

    public void SpawnPawns()
    {
        Vector3 spawnPointPos = route.wayPointsSorted[0].GetComponent<Transform>().position;

        for (int i = 0; i < numberOfPlayers; i++)
        {
            Pawn pawnSpawned = Instantiate(pawnPrefab, spawnPointPos, pawnPrefab.transform.rotation).GetComponent<Pawn>();
            pawnSpawned.playerOwner = players[i];
            players[i].pawn = pawnSpawned;

            NetworkServer.Spawn(pawnSpawned.gameObject);
        }

        RefreshStat();

        currentPlayer = 0;
        Debug.Log($"Player {players[currentPlayer].PlayerName} turn");
    }

    // check info about standing way point and recording stat
    public void TurnResult()
    {
        WayPointData pointData = route.wayPointsSorted[players[currentPlayer].pawn.currentTileIndex].GetComponent<WayPointData>();

        players[currentPlayer].numberOfTurns++;

        switch (pointData.bonusType)
        {
            case WayPointBonus.Default:
                {
                    NextTurn(false);
                    break;
                }

            case WayPointBonus.Fail:
                {
                    players[currentPlayer].numberOfFails++;
                    players[currentPlayer].pawn.MoveBack(route.wayPointsSorted, pointData.bonusValue);
                    break;
                }

            case WayPointBonus.Buff:
                {
                    players[currentPlayer].numberOfBuffs++;
                    players[currentPlayer].pawn.MoveNext(route.wayPointsSorted, pointData.bonusValue);
                    break;
                }

            case WayPointBonus.OneTurn:
                {
                    NextTurn(true);
                    break;
                }
        }


        if (gameEnd)
        {
            //ShowStat();
        }
    }

    // prepare for the next turn
    public void NextTurn(bool repeat)
    {
        RefreshStat();

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

        // Set new info about next turn player
        Debug.Log($"Player {players[currentPlayer].PlayerName} turn");
    }


    //Statistics table update 
    public void RefreshStat()
    {
        List<KeyValuePlace> places = new List<KeyValuePlace>();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            KeyValuePlace item;
            item.key = players[currentPlayer].PlayerIdNumber;
            item.value = players[currentPlayer].pawn.currentTileIndex;
            places.Add(item);
        }

        places = places.OrderByDescending(x => x.value).ToList();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            players[currentPlayer].place = places[i].key;
        }
    }

    // If player reached end point
    bool CheckForEnd(int pointPos)
    {
        if (route.wayPointsSorted[pointPos].GetComponent<WayPointData>().pointType == WayPointData.WayPointEnumerator.End)
        {
            return true;
        }

        return false;
    }
}
