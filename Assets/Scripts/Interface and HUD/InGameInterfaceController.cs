using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class InGameInterfaceController : NetworkBehaviour
{
    [Header("UI Infos")]
    [SerializeField] TextMeshProUGUI currentTurnText;
    [SerializeField] TextMeshProUGUI gameStatusText;
    [SerializeField] TextMeshProUGUI diceResultText;

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
}
