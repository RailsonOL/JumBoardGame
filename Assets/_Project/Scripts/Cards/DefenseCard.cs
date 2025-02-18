using UnityEngine;

[CreateAssetMenu(fileName = "New Defense Card", menuName = "Game/Card/DefenseCard")]
public class DefenseCard : Card
{
    public int defensePower;

    // Verifica se a carta pode ser jogada pelo jogador
    public override bool CanPlay()
    {
        return true;
    }

    // Executa a ação da carta
    public override void Execute()
    {
    }

    // Método para bloquear o efeito que vem de um ataque ou ação
    public void BlockEffect()
    {
        // Bloquear o efeito conforme o poder de defesa
    }
}
