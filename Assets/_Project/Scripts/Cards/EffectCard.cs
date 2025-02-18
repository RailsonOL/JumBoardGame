using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Card", menuName = "Game/Card/EffectCard")]
public class EffectCard : Card
{
    public int duration;

    public override bool CanPlay()
    {
        return true;
    }

    public override void Execute()
    {

    }

    // Aplica o efeito ao alvo
    public void ApplyEffect()
    {
        // Aplica o efeito (ex: envenenar, enfraquecer, etc.)
    }

    // Remove o efeito de um alvo
    public void RemoveEffect()
    {
        // Remove o efeito do alvo
    }
}
