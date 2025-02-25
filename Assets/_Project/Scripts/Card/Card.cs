using UnityEngine;

[Icon("Assets/Editor/SO Icons/Card SO Icon.png")]
[CreateAssetMenu(fileName = "New Card", menuName = "Game/Card")]
public class Card : ScriptableObject
{
    [Header("Card Visual Data")]
    public Sprite icon;
    public string cardName;
    [TextArea(3, 10)]
    public string description;

    [Header("Card Properties")]
    public int essenceCost;

    public enum Rarity
    {
        Common,     // Branco - #ffffff (255,255,255)
        Uncommon,   // Verde  - #1eff00 (30,255,0) 
        Rare,       // Azul   - #0070dd (0,112,221)
        Epic,       // Roxo   - #a335ee (163,53,238)
        Legendary   // Laranja - #ff8000 (255,128,0)
    }

    [Header("Rarity Settings")]
    [SerializeField] private Rarity cardRarity = Rarity.Common;
    [SerializeField] private Material customMaterial; // Opcional - para customização avançada

    [Header("Effect Settings")]
    [SerializeField] private SpecialEffect effect; // O efeito que será aplicado

    // Método para obter a cor baseada na raridade
    public Color GetRarityColor()
    {
        switch (cardRarity)
        {
            case Rarity.Common:
                return new Color(1f, 1f, 1f); // #ffffff
            case Rarity.Uncommon:
                return new Color(0.118f, 1f, 0f); // #1eff00
            case Rarity.Rare:
                return new Color(0f, 0.439f, 0.866f); // #0070dd
            case Rarity.Epic:
                return new Color(0.639f, 0.208f, 0.933f); // #a335ee
            case Rarity.Legendary:
                return new Color(1f, 0.502f, 0f); // #ff8000
            default:
                return Color.white;
        }
    }

    // Método para aplicar a cor da raridade ao SpriteRenderer
    public void ApplyRarityColor(SpriteRenderer renderer)
    {
        if (renderer == null) return;

        // Se tiver um material personalizado, use-o
        if (customMaterial != null)
        {
            renderer.material = customMaterial;
        }
        // Caso contrário, aplique a cor diretamente
        else
        {
            renderer.color = GetRarityColor();
        }
    }

    // Método para verificar se o jogador pode jogar a carta
    public bool CanPlay()
    {
        return effect != null;
    }

    // Método para executar a carta
    public void Execute(Essent targetEssent)
    {
        if (effect != null && targetEssent != null)
        {
            effect.ApplyEffect(targetEssent); // Aplica o efeito ao ídolo
            Debug.Log($"Efeito {effect.effectName} aplicado ao ídolo {targetEssent.data.essentName}.");
        }
        else
        {
            Debug.LogWarning("Efeito ou ídolo não encontrado.");
        }
    }
}