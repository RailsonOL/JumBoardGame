using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class TempCameraController : NetworkBehaviour
{
    public GameObject cameraHolder;
    public Vector3 cameraOffset;
    public GameObject PlayerModel;

    public override void OnStartAuthority() { //Called when the object is spawned on the client
        cameraHolder.SetActive(true);
    }

    private void Update() {
        if(SceneManager.GetActiveScene().name == "Game") {
            //cameraHolder.transform.position = transform.position + cameraOffset;
            if (PlayerModel.activeSelf == false)
            {
                PlayerModel.SetActive(true);
            }
        }
    }
}
