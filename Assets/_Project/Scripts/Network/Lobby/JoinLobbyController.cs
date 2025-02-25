using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class JoinLobbyController : MonoBehaviour
{
    public static JoinLobbyController Instance;

    public TMP_InputField lobbyCodeInput;  // Campo de entrada do código do lobby
    public Button joinLobbyButton;        // Botão para entrar na sala

    private Callback<LobbyMatchList_t> lobbyMatchListCallback; // Callback para receber a lista de lobbies

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Certifique-se de que o botão tem a função associada
        joinLobbyButton.onClick.AddListener(OnJoinLobbyButtonClicked);

        // Configurar o callback para receber a lista de lobbies
        lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
    }

    private void Start()
    {
        // Adicionar um evento para detectar pressionamento da tecla Enter
        lobbyCodeInput.onEndEdit.AddListener(OnEnterPressed);
    }

    // Método que será chamado quando o botão for pressionado
    private void OnJoinLobbyButtonClicked()
    {
        JoinLobbyByCode();
    }

    // Método para entrar no lobby usando o código
    private void JoinLobbyByCode()
    {
        string lobbyCode = lobbyCodeInput.text.Trim().ToUpper();

        if (!string.IsNullOrEmpty(lobbyCode))
        {
            // Configurar o filtro para pesquisar lobbies com o shortCode correspondente
            SteamMatchmaking.AddRequestLobbyListStringFilter("shortCode", lobbyCode, ELobbyComparison.k_ELobbyComparisonEqual);

            // Iniciar a pesquisa de lobbies
            SteamMatchmaking.RequestLobbyList();
        }
    }

    // Callback chamado quando a lista de lobbies é recebida
    private void OnLobbyMatchList(LobbyMatchList_t callback)
    {
        if (callback.m_nLobbiesMatching > 0)
        {
            // Pegar o primeiro lobby da lista (assumindo que há apenas um lobby com o shortCode correspondente)
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(0);

            // Tentar entrar no lobby usando o lobbyID encontrado
            SteamMatchmaking.JoinLobby(lobbyID);
        }
        else
        {
            Debug.LogError("Nenhum lobby encontrado com o código fornecido");
        }
    }

    // Detecta quando o Enter é pressionado no campo de entrada
    private void OnEnterPressed(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            JoinLobbyByCode();
        }
    }
}