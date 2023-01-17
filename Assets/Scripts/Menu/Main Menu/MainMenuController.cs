using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuWindow;
    public GameObject joinRoomWindow;
    public GameObject CreateRoomWindow;
    public GameObject optionsMenuWindow;
    public GameObject LobbyPainelWindow;

    void Start()
    {
        mainMenuWindow.SetActive(true);
        optionsMenuWindow.SetActive(false);
        joinRoomWindow.SetActive(false);
        CreateRoomWindow.SetActive(false);
        LobbyPainelWindow.SetActive(false);
    }

    public void OnClickJoinRoom()
    {
        mainMenuWindow.SetActive(false);
        joinRoomWindow.SetActive(true);
    }

    public void OnClickCreateRoom()
    {
        mainMenuWindow.SetActive(false);
        CreateRoomWindow.SetActive(true);
    }

    public void OnClickOptions()
    {
        mainMenuWindow.SetActive(false);
        optionsMenuWindow.SetActive(true);
    }

    public void OnClickBack()
    {
        mainMenuWindow.SetActive(true);
        optionsMenuWindow.SetActive(false);
        joinRoomWindow.SetActive(false);
        CreateRoomWindow.SetActive(false);
        LobbyPainelWindow.SetActive(false);
    }

    public void OnEnterLobby()
    {
        mainMenuWindow.SetActive(false);
        optionsMenuWindow.SetActive(false);
        joinRoomWindow.SetActive(false);
        CreateRoomWindow.SetActive(false);
        LobbyPainelWindow.SetActive(true);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
