using UnityEngine;

[CreateAssetMenu(fileName = "New Direct Attack Effect", menuName = "Game/Special Effect/Direct Attack Effect")]
public class DirectAttackEffect : SpecialEffect
{
    public int damageAmount = 5; // Quantidade de dano que o ataque causa
    public Essent currentTarget; // Alvo atual do ataque
    public override bool ApplyEffect(Essent essent)
    {
        if (essent == null)
        {
            Debug.LogWarning("Nenhum alvo selecionado para o ataque direto.");
            return false; // Retorna false se o alvo for nulo
        }

        // Aplica o dano ao alvo selecionado
        essent.GetSelectedTarget().ModifyEssence(-damageAmount);
        Debug.Log($"Ataque direto aplicado a {essent.GetSelectedTarget().essentName}, causando {damageAmount} de dano.");

        return true; // Retorna true para indicar que o efeito foi aplicado com sucesso
    }
}