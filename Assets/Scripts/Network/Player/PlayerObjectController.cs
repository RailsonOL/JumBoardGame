using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    //Player Data
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool PlayerReady;

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

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    private void PlayerReadyUpdate(bool oldReady, bool newReady)
    {
        if(isServer){
            this.PlayerReady = newReady;
        }

        if(isClient){
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
        if(isOwned){
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
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    public void PlayerNameUpdate(string oldName, string newName)
    {
        if(isServer){
            this.PlayerName = newName;
        }

        if(isClient){
            LobbyController.Instance.UpdatePlayerList();
        }
    }

    //Start Game
    public void CanStartGame(string SceneName)
    {
        if(isOwned){
            CmdCanStartGame(SceneName);
        }
    }

    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        Manager.StartGame(SceneName);
    }

}
