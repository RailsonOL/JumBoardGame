using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharactersCosmestics : MonoBehaviour
{
    public int currentEssentIndex = 0;
    public EssentData[] essentOptions; // Array of EssentData ScriptableObjects
    public Image essentIconImage;      // UI Image to display the essent's icon
    public TextMeshProUGUI essentNameText; // UI Text to display the essent's name

    private PlayerObjectController localPlayer; // Reference to the local PlayerObjectController

    private void Start()
    {
        StartCoroutine(StartSlow());
    }

    IEnumerator StartSlow()
    {
        // Wait until the LobbyController has set the LocalPlayerController
        yield return new WaitUntil(() => LobbyController.Instance != null && LobbyController.Instance.LocalPlayerController != null);

        localPlayer = LobbyController.Instance.LocalPlayerController;
        currentEssentIndex = PlayerPrefs.GetInt("currentEssentIndex", 0);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (essentOptions.Length == 0 || currentEssentIndex >= essentOptions.Length)
        {
            Debug.LogWarning("Essent options not set or index out of range!");
            return;
        }

        // Update the UI with the selected essent's data
        EssentData selectedEssent = essentOptions[currentEssentIndex];
        essentIconImage.sprite = selectedEssent.icon;
        essentNameText.text = selectedEssent.essentName;

        // Save the selected index
        PlayerPrefs.SetInt("currentEssentIndex", currentEssentIndex);

        // Send the selected essent ID to the PlayerObjectController
        if (localPlayer != null && localPlayer.isOwned)
        {
            localPlayer.CmdSetSelectedEssentId(selectedEssent.id);
        }
    }

    public void NextEssent()
    {
        if (currentEssentIndex < essentOptions.Length - 1) // Increment
            currentEssentIndex++;
        else if (currentEssentIndex == essentOptions.Length - 1) // Wrap to start
            currentEssentIndex = 0;

        UpdateUI();
    }

    public void PreviousEssent()
    {
        if (currentEssentIndex > 0) // Decrement
            currentEssentIndex--;
        else if (currentEssentIndex == 0) // Wrap to end
            currentEssentIndex = essentOptions.Length - 1;

        UpdateUI();
    }
}