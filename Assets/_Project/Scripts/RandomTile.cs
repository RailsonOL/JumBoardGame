using UnityEngine;
using System.Collections;

public class RandomTile : HexTile
{
    [SerializeField] private SpecialEffect[] possibleEffects; // Lista de efeitos possíveis
    private InGameInterfaceController interfaceController; // Referência ao controlador da interface

    public void SetInterfaceController(InGameInterfaceController controller)
    {
        interfaceController = controller;
    }

    public override void ExecuteTileEffect(Idol idol)
    {
        // Inicia a coroutine para aplicar o efeito com delay
        idol.StartCoroutine(ApplyEffectWithDelay(idol));
    }

    private IEnumerator ApplyEffectWithDelay(Idol idol)
    {
        if (interfaceController == null)
        {
            Debug.LogError("InterfaceController não foi atribuído ao RandomTile!");
            yield break;
        }

        // Mensagem inicial
        interfaceController.RpcUpdateGenericMessage("Sorteando Efeito...");
        yield return new WaitForSeconds(1f); // Delay de 1 segundo

        // Mensagem intermediária
        interfaceController.RpcUpdateGenericMessage("Ainda sorteando...");
        yield return new WaitForSeconds(1f); // Delay de 1 segundo

        // Escolhe um efeito aleatório da lista
        if (possibleEffects.Length > 0)
        {
            int randomIndex = Random.Range(0, possibleEffects.Length);
            SpecialEffect selectedEffect = possibleEffects[randomIndex];

            // Exibe a descrição do efeito escolhido
            interfaceController.RpcUpdateGenericMessage($"Efeito escolhido: {selectedEffect.description}");

            // Aplica o efeito selecionado
            selectedEffect.ApplyEffect(idol);
        }
        else
        {
            interfaceController.RpcUpdateGenericMessage("Nenhum efeito disponível para sortear.");
        }
    }
}