using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using TMPro;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby Instance;

    // Callbacks
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    // Lobbies Callbacks
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;

    public List<CSteamID> lobbyIDs = new List<CSteamID>();

    // Vars
    private NetworkManager manager;
    private const string HostAddressKey = "HostAddress";
    public ulong CurrentLobbyID;

    void Start()
    {
        if (!SteamManager.Initialized) return;
        if (Instance == null) Instance = this;

        manager = GetComponent<NetworkManager>();
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) return;

        Debug.Log("Lobby created");

        manager.StartHost();

        // Gerar um código curto com até 5 caracteres
        string shortCode = GenerateShortLobbyCode();

        // Armazenar o código curto
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName() + "'s Lobby");
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "shortCode", shortCode);

        Debug.Log("Lobby Code Generated: " + shortCode);
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Joining lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Everyone
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        Debug.Log("Lobby entered");

        // Client
        if (NetworkServer.active) return;
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        manager.networkAddress = hostAddress;
        manager.StartClient();
    }

    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    public static string GenerateShortLobbyCode()
    {
        // Gerar um valor aleatório único usando Guid e timestamp
        ulong uniqueValue = (ulong)System.Guid.NewGuid().GetHashCode() ^ (ulong)System.DateTime.Now.Ticks;

        // Codificar o valor único em base36
        string shortCode = Base36Encode(uniqueValue);

        // Pegar os últimos 5 caracteres do código
        string finalCode = shortCode.Length >= 5 ? shortCode.Substring(shortCode.Length - 5) : shortCode.PadLeft(5, 'X');
        Debug.Log("Final Lobby Code: " + finalCode);

        return finalCode;
    }

    private static string Base36Encode(ulong value)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string result = "";

        while (value > 0)
        {
            result = chars[(int)(value % 36)] + result;
            value /= 36;
        }

        return result;
    }
}