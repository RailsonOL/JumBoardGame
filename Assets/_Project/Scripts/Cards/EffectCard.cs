using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Card", menuName = "Game/Card/EffectCard")]
public class EffectCard : Card
{
    [Header("Effect Settings")]
    [SerializeField] private SpecialEffect effect; // O efeito que será aplicado

    public override bool CanPlay()
    {
        return effect != null;
    }

    public override void Execute(Idol targetIdol)
    {
        if (effect != null && targetIdol != null)
        {
            effect.ApplyEffect(targetIdol); // Aplica o efeito ao ídolo
            Debug.Log($"Efeito {effect.effectName} aplicado ao ídolo {targetIdol.data.idolName}.");
        }
        else
        {
            Debug.LogWarning("Efeito ou ídolo não encontrado.");
        }
    }
}