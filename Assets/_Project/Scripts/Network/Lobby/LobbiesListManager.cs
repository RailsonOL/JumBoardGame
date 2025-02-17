using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class LobbiesListManager : MonoBehaviour
{
    public static LobbiesListManager Instance;
    public GameObject lobbiesDataItemPrefab;
    public GameObject lobbyListContent;

    public List<GameObject> listOfLobbies = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    public void GetListOfLobbies(){
        SteamLobby.Instance.GetLobbiesList();
    }

    public void DisplayLobbies(List<CSteamID> lobbyID, LobbyDataUpdate_t result)
    {
        for(int i = 0; i < lobbyID.Count; i++)
        {
            if(lobbyID[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                GameObject createdItem = Instantiate(lobbiesDataItemPrefab);

                createdItem.GetComponent<LobbyDataEntry>().lobbyID = (CSteamID)lobbyID[i].m_SteamID;
                createdItem.GetComponent<LobbyDataEntry>().lobbyName = 
                    SteamMatchmaking.GetLobbyData(new CSteamID(lobbyID[i].m_SteamID), "name");
                createdItem.GetComponent<LobbyDataEntry>().SetLobbyData();

                createdItem.transform.SetParent(lobbyListContent.transform);
                createdItem.transform.localScale = Vector3.one;

                listOfLobbies.Add(createdItem);
            }
        }
    }

    public void DestroyLobbies(){
        foreach (GameObject lobby in listOfLobbies)
        {
            Destroy(lobby);
        }
        listOfLobbies.Clear();
    }
}
