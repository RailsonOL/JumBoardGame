using UnityEngine;

[CreateAssetMenu(fileName = "New Instant Card", menuName = "Game/Card/InstantCard")]
public class InstantCard : Card
{
    public override bool CanPlay()
    {
        return true;
    }

    public override void Execute()
    {

    }

    // Executa um efeito instantâneo
    public void ExecuteInstantEffect()
    {
        // Executa o efeito instantâneo da carta
    }
}
