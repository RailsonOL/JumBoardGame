using System.Net.Http.Headers;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action Card", menuName = "Game/Card/ActionCard")]
public class ActionCard : Card
{
    public int actionPower;

    // Verifica se a carta pode ser jogada pelo jogador
    public override bool CanPlay()
    {
        return true;
    }

    // Executa a ação da carta
    public override void Execute()
    {
    }

    // Método para executar uma ação específica no alvo
    public void ExecuteAction()
    {

    }
}
