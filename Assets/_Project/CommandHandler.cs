using UnityEngine;
using Mirror;
using System.Linq;

public class CommandHandler : NetworkBehaviour
{
    public static CommandHandler Instance { get; private set; }

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

    public void ProcessCommand(string command)
    {
        if (!command.StartsWith("/"))
        {
            return; // Não é um comando, ignora
        }

        string[] parts = command.Split(' ');
        string cmd = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : new string[0];

        switch (cmd)
        {
            case "/move":
                if (args.Length == 1 && int.TryParse(args[0], out int moveAmount))
                {
                    MovePlayer(moveAmount);
                }
                else
                {
                    ChatManager.Instance.SendSystemMessage("Uso incorreto do comando /move. Exemplo: /move 3");
                }
                break;

            case "/sendcard":
                if (args.Length == 2 && int.TryParse(args[0], out int playerId) && int.TryParse(args[1], out int cardId))
                {
                    SendCardToPlayer(playerId, cardId);
                }
                else
                {
                    ChatManager.Instance.SendSystemMessage("Uso incorreto do comando /sendcard. Exemplo: /sendcard 1 101");
                }
                break;

            // Adicione mais comandos aqui
            default:
                ChatManager.Instance.SendSystemMessage($"Comando desconhecido: {cmd}");
                break;
        }
    }

    private void MovePlayer(int moveAmount)
    {
        if (!isServer)
        {
            ChatManager.Instance.SendSystemMessage("Apenas o host pode executar este comando.");
            return;
        }

        var currentPlayer = GameManager.Instance.GetCurrentPlayerController();
        if (currentPlayer != null && currentPlayer.SelectedEssent != null)
        {
            currentPlayer.SelectedEssent.MoveNext(moveAmount);
            ChatManager.Instance.SendSystemMessage($"{currentPlayer.PlayerName} foi movido {moveAmount} tiles.");
        }
    }

    private void SendCardToPlayer(int playerId, int cardId)
    {
        if (!isServer)
        {
            ChatManager.Instance.SendSystemMessage("Apenas o host pode executar este comando.");
            return;
        }

        GameManager.Instance.SendCardToPlayer(playerId, cardId);
        ChatManager.Instance.SendSystemMessage($"Carta {cardId} enviada para o jogador {playerId}.");
    }
}