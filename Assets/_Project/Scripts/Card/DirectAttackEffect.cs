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
        Essent targetEssent = GameManager.Instance.GetEssentByID(essent.selectedTargetID);
        if (targetEssent != null)
        {
            targetEssent.ModifyEssence(-damageAmount);
        }
        else
        {
            Debug.LogWarning("Alvo não encontrado para o ataque direto.");
            return false; // Retorna false se o alvo não for encontrado
        }

        ChatManager.Instance.SendSystemMessage($"Direct attack applied to {targetEssent.essentName}, with {damageAmount} damage");

        essent.SetSelectedTarget(0); // reset target
        return true; // Retorna true para indicar que o efeito foi aplicado com sucesso
    }
}