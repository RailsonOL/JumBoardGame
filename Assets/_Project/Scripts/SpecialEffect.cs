using UnityEngine;

// Cria um menu no Unity para criar novos efeitos especiais
[CreateAssetMenu(fileName = "New Special Effect", menuName = "Game/Special Effect")]
public class SpecialEffect : ScriptableObject
{
    // Nome do efeito (opcional, para identificação)
    public string effectName;

    // Descrição do efeito (opcional, para UI ou debug)
    public string description;

    // Método virtual que aplica o efeito a um ídolo
    public virtual void ApplyEffect(Idol idol)
    {
        // Implementação base do efeito
        Debug.Log($"Efeito especial '{effectName}' aplicado em {idol.data.idolName}.");

        // Exemplo: Aumenta a essência do ídolo em 10
        idol.ModifyEssence(10);
    }
}