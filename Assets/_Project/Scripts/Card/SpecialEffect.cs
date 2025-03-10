using UnityEngine;

// Cria um menu no Unity para criar novos efeitos especiais
[Icon("Assets/Editor/SO Icons/Effect SO Icon.png")]
[CreateAssetMenu(fileName = "New Special Effect_effect", menuName = "Game/Special Effect Base")]
public class SpecialEffect : ScriptableObject
{
    [Header("Settings")]

    // Descrição do efeito (opcional, para identificação ou debug)
    [Tooltip("This data is not displayed in game, it is for identification purposes only.")]
    [TextArea(3, 10)]
    public string description;

    // Método virtual que aplica o efeito a um ídolo
    public virtual bool ApplyEffect(Essent essent)
    {
        // Exemplo: Aumenta a essência do ídolo em 10
        // essent.ModifyEssence(10);
        Debug.Log($"Efeito aplicado ao Essent {essent.essentName}.");
        return true; // Efeito aplicado com sucesso
    }
}