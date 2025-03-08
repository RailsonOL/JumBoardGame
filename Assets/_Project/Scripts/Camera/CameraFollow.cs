using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Target Settings")]
    [SerializeField] private GameObject target;

    [Header("Camera Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Camera Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomDistance = 2f;
    [SerializeField] private float maxZoomDistance = 10f;
    [SerializeField] private float zoomSmoothTime = 0.2f;

    [Header("Camera Position Settings")]
    [SerializeField] private float yPosition = 2f; // Altura fixa da câmera

    [SerializeField] private float currentZoomDistance = 5f;
    [SerializeField] private float desiredZoomDistance = 5f;
    [SerializeField] private float currentRotationX = 0f;
    [SerializeField] private float currentRotationY = 0f;
    private float zoomVelocity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Rotação da câmera ao segurar o botão direito do mouse
        if (Input.GetMouseButton(1)) // Botão direito do mouse
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed;

            currentRotationX += mouseX;
            currentRotationY += mouseY;

            // Limita o ângulo vertical da câmera
            currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
        }

        // Zoom com o scroll do mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        desiredZoomDistance -= scroll * zoomSpeed;
        desiredZoomDistance = Mathf.Clamp(desiredZoomDistance, minZoomDistance, maxZoomDistance);

        // Suavização do zoom
        currentZoomDistance = Mathf.SmoothDamp(currentZoomDistance, desiredZoomDistance, ref zoomVelocity, zoomSmoothTime);

        // Obtém a posição do target apenas no plano X e Z (ignora o Y)
        Vector3 newTargetPosition = new Vector3(
            target.transform.position.x, // Posição X do target
            yPosition, // Altura fixa da câmera
            target.transform.position.z // Posição Z do target
        );

        // Calcula a posição da câmera com base na rotação e zoom
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        Vector3 desiredPosition = newTargetPosition + rotation * (Vector3.back * currentZoomDistance);

        // Aplica a posição da câmera
        transform.position = desiredPosition;

        // Mantém a câmera olhando para o target (apenas no plano X e Z)
        transform.LookAt(new Vector3(target.transform.position.x, yPosition, target.transform.position.z));
    }

    public void UpdateTarget(GameObject newTarget)
    {
        target = newTarget;
        currentZoomDistance = Vector3.Distance(transform.position, target.transform.position);
        desiredZoomDistance = currentZoomDistance;
    }
}