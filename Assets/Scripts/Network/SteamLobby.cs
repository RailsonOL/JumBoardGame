using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    protected Callback<LobbyCreated_t> lobbyCreated;

    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;

    protected Callback<LobbyEnter_t> lobbyEntered;

    private NetworkManager manager;
    private MainMenuController mainMenuController;

    private const string HostAddressKey = "HostAddress";

    public ulong CurrentLobbyID;
    public TMP_Text lobbyNameText;
    public TMP_Text debuggText;


    void Start()

    {

        manager = GetComponent<NetworkManager>();

        Logg("SteamManager.Initialized: " + SteamManager.Initialized);
        if (!SteamManager.Initialized) return;
        
        mainMenuController = GameObject.Find("MainMenuController").GetComponent<MainMenuController>();
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

    }

    public void HostLobby()

    {

        //buttons.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);

    }

    private void OnLobbyCreated(LobbyCreated_t callback)

    {

        if (callback.m_eResult != EResult.k_EResultOK) return;

        Logg("Lobby created");

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName() + "'s Lobby");

    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)

    {
        Debug.Log("Joining lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        //everyone
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");
        mainMenuController.OnEnterLobby();
        Logg("Lobby entered");

        //client
        if (NetworkServer.active) return;

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        manager.networkAddress = hostAddress;

        manager.StartClient();

    }

    public void Logg(string log){
        debuggText.text += "\n" + log;
    }
}
