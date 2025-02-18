using UnityEngine;

public abstract class Card : ScriptableObject
{
    public string cardName;
    public string description;
    public int essenceCost;

    // Método para verificar se o jogador pode jogar a carta
    public abstract bool CanPlay();

    // Método para executar a carta, que será implementado nas classes derivadas
    public abstract void Execute();

}
