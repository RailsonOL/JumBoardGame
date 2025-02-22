using UnityEngine;

[CreateAssetMenu(fileName = "MoveForwardEffect", menuName = "Game/Special Effect/Move Forward Effect")]
public class MoveForwardEffect : SpecialEffect
{
    public int moveAmount = 3; // Quantidade de casas que o ídolo vai andar

    public override void ApplyEffect(Idol idol)
    {
        base.ApplyEffect(idol); // Chama o método base (opcional)

        // Move o ídolo +3 casas
        idol.MoveNext(moveAmount);

        Debug.Log($"{idol.data.idolName} avançou {moveAmount} casas!");
    }
}