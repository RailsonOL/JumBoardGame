using UnityEngine;
using UnityEngine.UI;  // Adicione esta linha para trabalhar com UI
using Steamworks;
using TMPro;

public class JoinLobbyController : MonoBehaviour
{
    public static JoinLobbyController Instance;

    // Adicione o campo de entrada e o botão de "Join"
    public TMP_InputField lobbyCodeInput;  // Campo de entrada do código do lobby
    public Button joinLobbyButton;     // Botão para entrar na sala

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Certifique-se de que o botão tem a função associada
        joinLobbyButton.onClick.AddListener(OnJoinLobbyButtonClicked);
    }

    private void Start()
    {
        // Também adicionamos um evento para detectar pressionamento da tecla Enter
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
        string lobbyCode = lobbyCodeInput.text.Trim();

        if (!string.IsNullOrEmpty(lobbyCode))
        {
            // Aqui você converte o código para CSteamID
            CSteamID lobbyID = GetLobbyIDFromCode(lobbyCode);
            if (lobbyID.IsValid())
            {
                // Se o lobby ID for válido, tente entrar no lobby
                SteamMatchmaking.JoinLobby(lobbyID);
            }
            else
            {
                Debug.LogError("Código de lobby inválido");
            }
        }
    }

    // Método que converte o código do lobby em CSteamID
    private CSteamID GetLobbyIDFromCode(string code)
    {
        // Converte o código base36 de volta para um ulong e cria um CSteamID
        ulong lobbyID = Base36Decode(code);
        return new CSteamID(lobbyID);
    }

    // Função para decodificar o código base36
    private ulong Base36Decode(string code)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        ulong result = 0;
        foreach (char c in code.ToUpper())
        {
            result = result * 36 + (ulong)chars.IndexOf(c);
        }
        return result;
    }

    // Detecta quando o Enter é pressionado no campo de entrada
    private void OnEnterPressed(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            JoinLobbyByCode();
        }
    }

    public void GetListOfLobbies()
    {
        SteamLobby.Instance.GetLobbiesList();
    }
}
