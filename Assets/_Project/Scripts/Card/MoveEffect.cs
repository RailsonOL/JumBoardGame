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

    public override void ApplyEffect(Essent essent)
    {
        base.ApplyEffect(essent);

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
    }
}