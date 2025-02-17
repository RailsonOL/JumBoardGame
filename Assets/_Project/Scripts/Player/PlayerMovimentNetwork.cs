using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovimentNetwork : NetworkBehaviour
{
    [Header("Camera")]
    public Camera cam;
    public float fovSpeed = 500.0f;
    public float mouseSense = 2f;
    public float lookXLimit = 90f;
    private float rotationX = 0;
    private float defaultFov = 70.0f;
    private float fov;

    [Header("========================================")]
    [Header("Movement")]
    public float walkSpeed = 10f;
    public float runSpeedMultiplier = 2f;
    public float flightSpeed = 10f;
    public bool canMove = true;
    private Vector3 moveDirection = Vector3.zero;

    [Header("========================================")]
    [Header("GameObjects")]
    public GameObject PlayerModel;
    private PlayerObjectController playerObjectController;

    //Components
    CharacterController characterController;
    [SyncVar] public GameController gameController;

    public override void OnStartAuthority()
    {
        cam.gameObject.SetActive(true);
        SetPosition();
    }

    private void Start()
    {
        playerObjectController = GetComponent<PlayerObjectController>();
        PlayerModel.SetActive(false);
        fov = cam.fieldOfView;
        defaultFov = fov;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (PlayerModel.activeSelf == false)
            {
                PlayerModel.SetActive(true);

                characterController = GetComponent<CharacterController>();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (isOwned)
            {
                Movement();
                FOVCamera();
                //Rolling dice and start movement
                if (Input.GetKeyDown(KeyCode.R) && playerObjectController.isOurTurn)
                {
                    gameController.CmdRollDice();
                }
            }
        }
    }

    public void SetPosition()
    {
        transform.position = new Vector3(Random.Range(-5, 5), 2f, Random.Range(-10, 7));
    }

    void Movement()
    {
        #region Movement 
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? walkSpeed * runSpeedMultiplier : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? walkSpeed * runSpeedMultiplier : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        //Up and Down
        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection.y += flightSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDirection.y -= flightSpeed;
        }

        #endregion

        #region Rotation
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * mouseSense;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseSense, 0);
        }

        #endregion
    }

    void FOVCamera()
    {
        float wheelMove = Input.GetAxis("Mouse ScrollWheel");
        fov -= wheelMove * fovSpeed * Time.deltaTime;

        if (fov > 90)
            fov = 90;
        if (fov < 30)
            fov = 30;

        cam.fieldOfView = fov;
    }
}
