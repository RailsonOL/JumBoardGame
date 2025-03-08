using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Effect", menuName = "Game/Special Effect/Attack Effect")]
public class AttackEffect : SpecialEffect
{
    public enum AttackType
    {
        SameTile,
        FrontTile,
        BackTile,
        FrontAndBack
    }

    [Header("Attack Settings")]
    public AttackType attackType;
    public int damageAmount = 5;

    public override bool ApplyEffect(Essent essent)
    {
        if (essent == null)
        {
            Debug.LogWarning("Essent não encontrado para aplicar o efeito de ataque.");
            return false;
        }

        HexTile currentTile = essent.currentTile;
        bool effectApplied = false;

        switch (attackType)
        {
            case AttackType.SameTile:
                effectApplied = AttackEssentOnTile(currentTile, essent);
                break;
            case AttackType.FrontTile:
                effectApplied = AttackEssentOnTile(currentTile.GetNextHex(), essent);
                break;
            case AttackType.BackTile:
                effectApplied = AttackEssentOnTile(currentTile.GetPreviousHex(), essent);
                break;
            case AttackType.FrontAndBack:
                bool frontApplied = AttackEssentOnTile(currentTile.GetNextHex(), essent);
                bool backApplied = AttackEssentOnTile(currentTile.GetPreviousHex(), essent);
                effectApplied = frontApplied || backApplied;
                break;
        }

        return effectApplied;
    }

    private bool AttackEssentOnTile(HexTile tile, Essent attacker)
    {
        if (tile == null) return false;

        Essent[] essentsOnTile = FindObjectsByType<Essent>(FindObjectsSortMode.None)
            .Where(e => e.currentTile == tile && e != attacker) // Exclui o próprio Essent
            .ToArray();

        if (essentsOnTile.Length == 0)
        {
            Debug.LogWarning("Nenhum alvo válido encontrado no tile.");
            return false;
        }

        foreach (Essent targetEssent in essentsOnTile)
        {
            targetEssent.ModifyEssence(-damageAmount);
            Debug.Log($"Ataque aplicado ao Essent {targetEssent.essentName} causando {damageAmount} de dano.");
        }

        return true; // Efeito aplicado com sucesso
    }
}