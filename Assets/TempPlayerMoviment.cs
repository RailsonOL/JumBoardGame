using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class TempPlayerMoviment : NetworkBehaviour
{
    public float Speed = 0.1f;
    public GameObject PlayerModel;

    private void Start() {
        PlayerModel.SetActive(false);
    }

    private void Update() {

        if(SceneManager.GetActiveScene().name == "Game"){
            if(PlayerModel.activeSelf == false){
                SetPosition();
                PlayerModel.SetActive(true);
            }

            if(isOwned){
                Movement();
            }
        }
    }

    public void SetPosition(){
        transform.position = new Vector3(Random.Range(-5, 5), 0.8f, Random.Range(-10, 7));
    }

    public void Movement(){
        float xDirection = Input.GetAxis("Horizontal");
        float yDirection = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(xDirection, yDirection, 0);

        transform.position += movement * Speed;
    }
}
