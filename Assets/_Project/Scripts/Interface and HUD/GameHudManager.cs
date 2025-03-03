using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GameHudManager : NetworkBehaviour
{
    public static GameHudManager Inst { get; private set; }

    [Header("UI Infos")]
    [SerializeField] private TextMeshProUGUI currentTurnText;
    [SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private TextMeshProUGUI diceResultText;
    [SerializeField] private TextMeshProUGUI genericMessage;

    [Header("Players Essent Status")]
    [SerializeField] private TextMeshProUGUI[] playerEssentStatus; // Array para os status dos jogadores

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
        for (int i = 0; i < playerEssentStatus.Length; i++)
        {
            playerEssentStatus[i].gameObject.SetActive(i < quantity);
        }
    }

    [ClientRpc]
    public void RpcUpdatePlayerEssentStatus(int playerIndex, string essentName, string currentEssence)
    {
        if (playerIndex < 1 || playerIndex > playerEssentStatus.Length)
        {
            Debug.LogWarning("Índice de jogador inválido: " + playerIndex);
            return;
        }

        // Template padrão no Inspector
        string template = "{essentName}: {currentEssence}";

        // Substituir placeholders pelos valores reais
        string formattedText = template.Replace("{essentName}", essentName)
                                       .Replace("{currentEssence}", currentEssence);

        // Atualizar o texto do jogador correspondente
        playerEssentStatus[playerIndex - 1].SetText(formattedText);
    }
}