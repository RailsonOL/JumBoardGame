using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Steamworks;
using UnityEngine.UI;

public class CharactersCosmestics : MonoBehaviour
{
    public int currentColorIndex = 0;
    public Material[] colors;
    public Image colorImage;
    public TextMeshProUGUI colorText;

    private void Start()
    {
        StartCoroutine(StartSlow());
    }

    IEnumerator StartSlow()
    {
        yield return new WaitWhile(() => LobbyController.Instance.LocalPlayerController != null); // Wait until the local player controller is set
        currentColorIndex = PlayerPrefs.GetInt("currentColorIndex", 0);
        colorImage.color = colors[currentColorIndex].color;
        colorText.text = colors[currentColorIndex].name;
    }

    private void SetColor(int index)
    {
        PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
        colorImage.color = colors[currentColorIndex].color;
        colorText.text = colors[currentColorIndex].name;
        LobbyController.Instance.LocalPlayerController.CmdUpdatePawnColor(index);
    }

    public void NextColor()
    {
        if (currentColorIndex < colors.Length - 1) // Increment
            currentColorIndex++;
        else if (currentColorIndex == colors.Length - 1) // Wrap
            currentColorIndex = 0;

        SetColor(currentColorIndex);
    }

    public void PreviousColor()
    {
        if (currentColorIndex > 0) // Decrement
            currentColorIndex--;
        else if (currentColorIndex == 0) // Wrap
            currentColorIndex = colors.Length - 1;

        SetColor(currentColorIndex);
    }
}
