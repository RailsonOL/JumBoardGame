using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class TempPlayerMoviment : NetworkBehaviour
{
    [SerializeField] float speed = 5.0f;
    [SerializeField] float shift = 10.0f;
    public GameObject PlayerModel;
    [SerializeField] Vector3 plusLimit = new Vector3(500.0f, 500.0f, 500.0f);
    [SerializeField] Vector3 minusLimit = new Vector3(500.0f, 500.0f, 500.0f);
    public Camera cam;
    [SerializeField] float distanceToTarget = 10;
    Vector3 previousPosition;
    [SerializeField] Transform pivot;

    private void Start()
    {
        PlayerModel.SetActive(false);
    }

    private void Update()
    {

        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (PlayerModel.activeSelf == false)
            {
                SetPosition();
                PlayerModel.SetActive(true);
            }

            if (isOwned)
            {
                WASDMovement();
                RotateCamera();

                if (Input.GetKey(KeyCode.Space))
                {
                    transform.position += Vector3.up * speed * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    transform.position += Vector3.down * speed * Time.deltaTime;
                }
            }
        }
    }

    public void SetPosition()
    {
        transform.position = new Vector3(Random.Range(-5, 5), 0.8f, Random.Range(-10, 7));
    }

    void RotateCamera()
    {
        if (Input.GetMouseButtonDown(1))
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        else if (Input.GetMouseButton(1))
        {
            distanceToTarget = Vector3.Distance(pivot.position, cam.transform.position);

            Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            Vector3 direction = previousPosition - newPosition;

            float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
            float rotationAroundXAxis = direction.y * 180; // camera moves vertically

            cam.transform.position = pivot.position;

            cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World);

            cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

            previousPosition = newPosition;
        }
    }

    Vector3 GetInput(Vector3 vec)
    {
        if (Input.GetKey(KeyCode.W))
        {
            vec.z += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            vec.z += -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            vec.x += -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            vec.x += 1;
        }

        return vec;
    }

    Vector3 CheckLimits(Transform pos, Vector3 moveDir)
    {
        Vector3 newPos = pos.position + moveDir;

        if (newPos.x > plusLimit.x)
        {
            newPos.x = plusLimit.x;
        }
        if (newPos.x < -minusLimit.x)
        {
            newPos.x = -minusLimit.x;
        }

        if (newPos.y > plusLimit.y)
        {
            newPos.y = plusLimit.y;
        }
        if (newPos.y < -minusLimit.y)
        {
            newPos.y = -minusLimit.y;
        }

        if (newPos.z > plusLimit.z)
        {
            newPos.z = plusLimit.z;
        }
        if (newPos.z < -minusLimit.z)
        {
            newPos.z = -minusLimit.z;
        }

        return newPos;
    }

    void WASDMovement()
    {
        Vector3 inputDir = new Vector3(0.0f, 0.0f, 0.0f);
        inputDir = GetInput(inputDir);

        Transform pos = transform;

        // disable y-axis
        Vector3 forward = transform.forward;
        forward.y = 0;
        Vector3 right = transform.right;
        right.y = 0;
        Vector3 moveDir = (forward * inputDir.z) + (right * inputDir.x);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDir *= shift * speed * Time.deltaTime;
        }
        else
        {
            moveDir *= speed * Time.deltaTime;
        }

        transform.position = CheckLimits(pos, moveDir);
    }
}
