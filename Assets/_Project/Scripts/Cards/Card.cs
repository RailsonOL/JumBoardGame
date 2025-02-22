using UnityEngine;

[Icon("Assets/_Project/Resources/Card SO Icon.png")]
public abstract class Card : ScriptableObject
{
    [Header("Card Visual Data")]
    public Sprite icon;  // Mudado de Image para Sprite
    public string cardName;
    [TextArea(3, 10)]  // Permite múltiplas linhas no editor
    public string description;

    [Header("Card Properties")]
    public int essenceCost;

    // Método para verificar se o jogador pode jogar a carta
    public abstract bool CanPlay();

    // Método para executar a carta, que será implementado nas classes derivadas
    public abstract void Execute();
}