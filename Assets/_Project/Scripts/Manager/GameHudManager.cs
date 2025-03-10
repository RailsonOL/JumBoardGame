using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class GameHudManager : NetworkBehaviour
{
    public static GameHudManager Instance { get; private set; }

    [Header("UI Infos")]
    [SerializeField] private TextMeshProUGUI currentTurnText;
    [SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private TextMeshProUGUI genericMessage;

    [Header("Players Essent Status")]
    [SerializeField] private TextMeshProUGUI[] essentStatus;
    [SerializeField] private Button[] essentStatusButton;

    [Header("Loading Panel")]
    [SerializeField] private GameObject loadingPanel; // Adicione esta linha

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ClientRpc]
    public void RpcUpdateGameStatus(string text)
    {
        gameStatusText.SetText(text);
    }

    [ClientRpc]
    public void RpcUpdateCurrentTurn(string text)
    {
        currentTurnText.SetText(text);
    }

    [ClientRpc]
    public void RpcUpdateDiceResult(string text)
    {
        diceResultText.SetText(text);
    }

    [ClientRpc]
    public void RpcUpdateGenericMessage(string text)
    {
        genericMessage.SetText(text);
    }

    [ClientRpc]
    public void RpcActivatePlayerEssentStatus(int quantity)
    {
        for (int i = 0; i < essentStatusButton.Length; i++)
        {
            essentStatusButton[i].gameObject.SetActive(i < quantity);
        }
    }

    [ClientRpc]
    public void RpcUpdatePlayerEssentStatus(int essentIndex, string essentName, string currentEssence)
    {
        if (essentIndex < 1 || essentIndex > essentStatus.Length)
        {
            Debug.LogWarning("Índice de Essente inválido: " + essentIndex);
            return;
        }

        string template = "{essentName}: {currentEssence}";

        string formattedText = template.Replace("{essentName}", essentName)
                                       .Replace("{currentEssence}", currentEssence);

        essentStatus[essentIndex - 1].SetText(formattedText);

        // Configurar o evento de clique no botão correspondente
        if (essentIndex - 1 < essentStatusButton.Length)
        {
            Button button = essentStatusButton[essentIndex - 1];
            button.onClick.RemoveAllListeners(); // Remove listeners anteriores para evitar duplicação
            button.onClick.AddListener(() => OnEssentButtonClicked(essentIndex - 1));
        }
    }

    [ClientRpc]
    public void RpcSetLoadingPanelVisibility(bool isVisible)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(isVisible);
        }
        else
        {
            Debug.LogWarning("LoadingPanel não está atribuído no GameHudManager.");
        }
    }

    private void OnEssentButtonClicked(int essentIndex)
    {
        Debug.Log($"Botão do Essent do jogador {essentIndex + 1} clicado");

        // Obtenha o netId do Essent com base no índice
        Essent essentLocal = GameManager.Instance.players[essentIndex].GetSelectedEssentLocal();

        // Verifique se o netId é válido
        if (essentLocal != null)
        {
            CameraManager.Instance?.ActivateEssentFollowCamera(essentLocal.gameObject);
        }
    }
}