using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "Game/Special Effect/Heal Effect")]
public class HealEffect : SpecialEffect
{
    public int healAmount = 20;

    public override void ApplyEffect(Idol idol)
    {
        base.ApplyEffect(idol); // Chama o método base (opcional)
        idol.ModifyEssence(healAmount);
        Debug.Log($"{idol.data.idolName} foi curado em {healAmount} de essência.");
    }
}