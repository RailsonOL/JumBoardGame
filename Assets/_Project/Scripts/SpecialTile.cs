using UnityEngine;

public class SpecialTile : HexTile
{
    [SerializeField] private SpecialEffect effect;

    public override void ExecuteTileEffect(Idol idol)
    {
        if (effect != null)
        {
            effect.ApplyEffect(idol);
        }
    }
}