using UnityEngine;

public enum MoveDirection
{
    Forward,
    Backward
}

[CreateAssetMenu(fileName = "MoveEffect", menuName = "Game/Special Effect/Move Effect")]
public class MoveEffect : SpecialEffect
{
    public int moveAmount = 3;
    public MoveDirection direction = MoveDirection.Forward;

    public override bool ApplyEffect(Essent essent)
    {
        if (essent == null)
        {
            Debug.LogWarning("Essent não encontrado para aplicar o efeito de movimento.");
            return false; // Retorna false se o Essent for nulo
        }

        // Aplica o movimento com base na direção
        if (direction == MoveDirection.Forward)
        {
            essent.MoveNext(moveAmount);
            Debug.Log($"{essent.data.essentName} avançou {moveAmount} casas!");
        }
        else if (direction == MoveDirection.Backward)
        {
            essent.MoveBack(moveAmount);
            Debug.Log($"{essent.data.essentName} voltou {moveAmount} casas!");
        }

        return true; // Retorna true para indicar que o efeito foi aplicado com sucesso
    }
}