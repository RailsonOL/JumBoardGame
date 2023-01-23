using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Steamworks;
using UnityEngine.UI;

public class CharactersCosmestics : MonoBehaviour
{
    public int currentColorIndex;
    public Material[] colors;
    public Image colorImage;
    public TextMeshProUGUI colorText;

    private void Start()
    {
        currentColorIndex = PlayerPrefs.GetInt("currentColorIndex", 0);
        colorImage.color = colors[currentColorIndex].color;
        colorText.text = colors[currentColorIndex].name;
    }

    public void NextColor()
    {
        if(currentColorIndex < colors.Length - 1)
        {
            currentColorIndex++;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            colorImage.color = colors[currentColorIndex].color;
            colorText.text = colors[currentColorIndex].name;
        }
    }

    public void PrevColor()
    {
        if (currentColorIndex > 0)
        {
            currentColorIndex--;
            PlayerPrefs.SetInt("currentColorIndex", currentColorIndex);
            colorImage.color = colors[currentColorIndex].color;
            colorText.text = colors[currentColorIndex].name;
        }
    }
}
