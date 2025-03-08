using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AttackTargetSelection : MonoBehaviour
{
    [Header("UI References")]
    public GameObject targetPanel;
    public Transform targetListContainer;
    public GameObject targetButtonPrefab;

    // Evento para notificar quando um alvo é selecionado
    public event Action<Essent> OnTargetSelectedEvent;

    public void ShowTargets(List<Essent> targets)
    {
        // Ativar o painel de seleção de alvos
        targetPanel.SetActive(true);

        // Limpar botões de alvos anteriores
        foreach (Transform child in targetListContainer)
        {
            Destroy(child.gameObject);
        }

        // Criar um botão para cada alvo
        foreach (Essent target in targets)
        {
            GameObject button = Instantiate(targetButtonPrefab, targetListContainer);
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            Button buttonComponent = button.GetComponent<Button>();

            if (buttonText != null)
            {
                buttonText.text = target.essentName; // Definir o texto do botão como o nome do Essent
            }

            if (buttonComponent != null)
            {
                // Adicionar um listener para selecionar o alvo quando o botão for clicado
                buttonComponent.onClick.AddListener(() => OnTargetSelected(target));
            }
        }
    }

    private void OnTargetSelected(Essent target)
    {
        // Notificar que um alvo foi selecionado
        OnTargetSelectedEvent?.Invoke(target);

        // Fechar o painel de seleção de alvos
        targetPanel.SetActive(false);

        Debug.Log($"Selected target: {target.essentName}");
    }

    public void HideTargets()
    {
        targetPanel.SetActive(false);
    }
}