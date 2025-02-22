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

    public override void Execute()
    {
        // Aqui você pode adicionar lógica para encontrar o alvo apropriado
        // Por exemplo, pegar o Idol selecionado ou o Idol atual
        if (effect != null)
        {
            // Supondo que você tenha acesso ao Idol alvo de alguma forma
            // Idol targetIdol = ... 
            // effect.ApplyEffect(targetIdol);

            Debug.Log($"Executando carta com efeito: {effect.effectName}");
        }
    }
}