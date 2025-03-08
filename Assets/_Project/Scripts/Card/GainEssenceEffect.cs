using UnityEngine;

[CreateAssetMenu(fileName = "GainEssenceEffect", menuName = "Game/Special Effect/Gain Essence")]
public class GainEssenceEffect : SpecialEffect
{
    public int essenceAmount = 5;

    public override bool ApplyEffect(Essent essent)
    {
        if (essent == null)
        {
            Debug.LogWarning("Essent não encontrado para aplicar o efeito de ganho de essência.");
            return false; // Retorna false se o Essent for nulo
        }

        // Adiciona essência ao Essent que usou a carta
        essent.ModifyEssence(essenceAmount);
        Debug.Log($"{essent.essentName} ganhou {essenceAmount} de essência!");

        return true; // Retorna true para indicar que o efeito foi aplicado com sucesso
    }
}