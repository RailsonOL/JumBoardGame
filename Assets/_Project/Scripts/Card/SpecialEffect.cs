using UnityEngine;

// Cria um menu no Unity para criar novos efeitos especiais
[Icon("Assets/Editor/SO Icons/Effect SO Icon.png")]
[CreateAssetMenu(fileName = "New Special Effect", menuName = "Game/Special Effect Base")]
public class SpecialEffect : ScriptableObject
{
    [Header("Settings")]
    [Tooltip("This data is not displayed in game, it is for identification purposes only.")]
    // Nome do efeito (opcional, para identificação)
    public string effectName;

    // Descrição do efeito (opcional, para UI ou debug)
    [Tooltip("This data is not displayed in game, it is for identification purposes only.")]
    public string description;

    // Método virtual que aplica o efeito a um ídolo
    public virtual void ApplyEffect(Essent essent)
    {
        // Implementação base do efeito
        Debug.Log($"Efeito especial '{effectName}' aplicado em {essent.data.essentName}.");

        // Exemplo: Aumenta a essência do ídolo em 10
        essent.ModifyEssence(10);
    }
}