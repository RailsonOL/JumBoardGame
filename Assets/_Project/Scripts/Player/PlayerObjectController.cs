using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerObjectController : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool PlayerReady;
    [SyncVar(hook = nameof(OnReadyToPlayChanged))] public bool readyToPlay = false; // Nova SyncVar

    [Header("Player Game Data")]
    public Essent SelectedEssent;
    [SyncVar] public uint SelectedEssentNetId;
    [SyncVar(hook = nameof(OnSelectedEssentIdChanged))]
    public int SelectedEssentId = -1; // Store the selected essent ID (default -1 means none selected)

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

        if (isOwned)
        {
            StartCoroutine(CheckReadyToPlay());
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (isOwned)
            {
                if (Input.GetKeyDown(KeyCode.R) && isOurTurn)
                {
                    GameController.Instance.CmdRollDice();
                }

                // if (Input.GetKeyDown(KeyCode.C))
                // {
                //     if (isLocalPlayer)
                //     {
                //         Debug.Log("Request Card");
                //         RequestInitialCards();
                //     }
                // }
            }
        }
    }

    private IEnumerator CheckReadyToPlay()
    {
        // Aguarda até que a cena esteja totalmente carregada e os objetos necessários estejam prontos
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Game");

        Debug.Log($"Cena game carregada para o cliente {ConnectionID}, verificando prontidão.");

        while (!IsClientReady())
        {
            Debug.Log($"Cliente {ConnectionID} ainda não está pronto.");
            yield return new WaitForSeconds(0.3f); // Re verifica a cada 0.3s
        }

        Debug.Log($"Cliente {ConnectionID} está pronto, atualizando readyToPlay.");
        CmdSetReadyToPlay(true);
    }

    private bool IsClientReady()
    {
        bool essentReady = NetworkClient.spawned.ContainsKey(SelectedEssentNetId);
        bool hudReady = GameHudManager.Inst != null;
        bool cameraReady = CameraFollow.Instance != null;

        // if essentials are ready, check if the player is ready
        return essentReady && hudReady && cameraReady;
    }

    [Command]
    private void CmdSetReadyToPlay(bool ready)
    {
        readyToPlay = ready;
    }

    private void OnReadyToPlayChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"Cliente {ConnectionID} mudou readyToPlay para {newValue}");
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

    public Essent GetSelectedEssentByNetId()
    {
        // Verifica se o NetworkIdentity com o netId existe na lista de objetos spawnados
        if (NetworkClient.spawned.TryGetValue(SelectedEssentNetId, out NetworkIdentity networkIdentity))
        {
            // Tenta obter o componente Essent do objeto encontrado
            Essent essent = networkIdentity.GetComponent<Essent>();
            if (essent != null)
            {
                return essent;
            }
            else
            {
                Debug.LogWarning($"Objeto com NetworkIdentity {netId} encontrado, mas não possui componente Essent.");
            }
        }
        else
        {
            Debug.LogWarning($"Objeto com NetworkIdentity {netId} não encontrado na lista de objetos spawnados.");
        }

        return null;
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

    [Command]
    public void CmdSetSelectedEssentId(int essentId)
    {
        SelectedEssentId = essentId;
        Debug.Log($"Player {PlayerName} selected Essent ID: {essentId}");
    }

    private void OnSelectedEssentIdChanged(int oldId, int newId)
    {
        Debug.Log($"Selected Essent ID changed from {oldId} to {newId} for Player {PlayerName}");
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

    [TargetRpc]
    public void TargetInitializeHand(NetworkConnection target, List<int> cardIds)
    {
        PlayerHand playerHand = GetComponentInChildren<PlayerHand>();
        if (playerHand != null)
        {
            playerHand.InitializeHand(cardIds);
        }
    }

    public void ReceiveCard(Card card)
    {
        if (card != null)
        {
            Card cardFromManager = CardManager.Instance.GetCardById(card.id);
            if (cardFromManager != null)
            {
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

    public void ReciveCardByID(int cardID)
    {
        Card cardFromManager = CardManager.Instance.GetCardById(cardID);
        if (cardFromManager != null)
        {
            PlayerHand playerHand = GetComponent<PlayerHand>();
            if (playerHand != null)
            {
                playerHand.AddCardToHand(cardFromManager);
            }
            Debug.Log($"Carta recebida: {cardFromManager.cardName}");
        }
        else
        {
            Debug.LogWarning($"Carta com ID {cardID} não encontrada no CardManager!");
        }
    }

    private void GetTilePosition() { }
    private void GetCurrentEssence() { }
    private void GetEssentName() { }
}