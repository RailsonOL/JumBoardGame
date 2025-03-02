using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GameHudManager : NetworkBehaviour
{
    public static GameHudManager Inst { get; private set; }

    [Header("UI Infos")]
    [SerializeField] TextMeshProUGUI currentTurnText;
    [SerializeField] TextMeshProUGUI gameStatusText;
    [SerializeField] TextMeshProUGUI diceResultText;
    [SerializeField] TextMeshProUGUI genericMessage;

    [Header("Players Essent Status")]
    [SerializeField] TextMeshProUGUI player1EssentStatus;
    [SerializeField] TextMeshProUGUI player2EssentStatus;
    [SerializeField] TextMeshProUGUI player3EssentStatus;
    [SerializeField] TextMeshProUGUI player4EssentStatus;

    private void Awake()
    {
        // Configura o Singleton
        if (Inst == null)
        {
            Inst = this;
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
        player1EssentStatus.gameObject.SetActive(quantity >= 1);
        player2EssentStatus.gameObject.SetActive(quantity >= 2);
        player3EssentStatus.gameObject.SetActive(quantity >= 3);
        player4EssentStatus.gameObject.SetActive(quantity >= 4);
    }

    [ClientRpc]
    public void RpcUpdatePlayerEssentStatus(int playerIndex, string essentName, string currentEssence)
    {
        // Template padrão no Inspector
        string template = "{essentName}: {currentEssence}";

        // Substituir placeholders pelos valores reais
        string formattedText = template.Replace("{essentName}", essentName)
                                       .Replace("{currentEssence}", currentEssence);

        // Atualizar o texto correto com base no playerIndex
        switch (playerIndex)
        {
            case 1:
                player1EssentStatus.SetText(formattedText);
                break;
            case 2:
                player2EssentStatus.SetText(formattedText);
                break;
            case 3:
                player3EssentStatus.SetText(formattedText);
                break;
            case 4:
                player4EssentStatus.SetText(formattedText);
                break;
            default:
                Debug.LogWarning("Índice de jogador inválido: " + playerIndex);
                break;
        }
    }


}