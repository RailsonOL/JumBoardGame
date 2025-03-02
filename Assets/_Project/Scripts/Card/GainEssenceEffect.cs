using UnityEngine;

[CreateAssetMenu(fileName = "GainEssenceEffect", menuName = "Game/Special Effect/Gain Essence")]
public class GainEssenceEffect : SpecialEffect
{
    public int essenceAmount = 5;

    public override void ApplyEffect(Essent essent)
    {
        base.ApplyEffect(essent);

        // Adiciona essência ao Essent que usou a carta
        essent.ModifyEssence(essenceAmount);
        Debug.Log($"{essent.essentName} ganhou {essenceAmount} de essência!");
    }
}