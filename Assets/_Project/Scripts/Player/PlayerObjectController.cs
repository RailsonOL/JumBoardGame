using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class PlayerObjectController : NetworkBehaviour
{
    [Header("Player Data")]

    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool PlayerReady;

    [Header("Player Game Data")]
    public Essent SelectedEssent;

    public GameController gameController;

    [SyncVar] public bool isOurTurn = false;

    public int numberOfTurns = 0;
    public int numberOfBuffs = 0;
    public int numberOfFails = 0;
    public int place = 0;
    public bool isEnd = false;

    private CustomNetworkManager manager;
    private CustomNetworkManager Manager => manager ??= NetworkManager.singleton as CustomNetworkManager;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (isOwned)
            {
                //Rolling dice and start movement
                if (Input.GetKeyDown(KeyCode.R) && isOurTurn)
                {
                    gameController.CmdRollDice();
                }
            }
        }
    }

    private void PlayerReadyUpdate(bool oldReady, bool newReady)
    {
        if (isServer)
        {
            PlayerReady = newReady;
        }

        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    [Command]
    private void CmdChangeReadyState()
    {
        this.PlayerReadyUpdate(this.PlayerReady, !this.PlayerReady);
    }

    public void ChangeReadyState()
    {
        if (isOwned)
        {
            CmdChangeReadyState();
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName());
        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();

    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();
    }

    [Command]
    public void CmdSetPlayerName(string PlayerName)
    {
        PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    public void PlayerNameUpdate(string oldName, string newName)
    {
        if (isServer)
        {
            PlayerName = newName;
        }

        if (isClient)
        {
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    //Start Game
    public void CanStartGame(string SceneName)
    {
        if (isOwned)
        {
            CmdCanStartGame(SceneName);
        }
    }

    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        Manager.StartGame(SceneName);
    }

    public void ReceiveCard(Card card)
    {
        if (card != null)
        {
            // Busca a carta pelo ID usando o CardManager
            Card cardFromManager = CardManager.Instance.GetCardById(card.id);
            if (cardFromManager != null)
            {
                // Atualiza a mão do jogador, se necessário
                PlayerHand playerHand = GetComponent<PlayerHand>();
                if (playerHand != null)
                {
                    playerHand.AddCardToHand(cardFromManager);
                }

                Debug.Log($"Carta recebida: {cardFromManager.cardName}");
            }
            else
            {
                Debug.LogWarning($"Carta com ID {card.id} não encontrada no CardManager!");
            }
        }
    }

    private void GetTilePosition()
    {

    }

    private void GetCurrentEssence()
    {

    }

    private void GetEssentName()
    {

    }
}
