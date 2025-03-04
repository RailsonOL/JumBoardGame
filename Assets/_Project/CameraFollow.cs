using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    private GameObject target; // Objeto que a câmera segue
    public float yPosition = 10f; // Altura fixa da câmera
    public float distance = 10f; // Distância da câmera ao objeto
    public float xOffset = 0f; // Deslocamento lateral da câmera

    [Header("Zoom Settings")]
    [Tooltip("Velocidade do zoom ao girar o scroll do mouse.")]
    public float zoomSpeed = 5f; // Velocidade do zoom
    [Tooltip("Distância mínima da câmera ao target (zoom in máximo).")]
    public float minDistance = 5f; // Distância mínima
    [Tooltip("Distância máxima da câmera ao target (zoom out máximo).")]
    public float maxDistance = 20f; // Distância máxima
    [Tooltip("Velocidade de suavização do zoom.")]
    public float zoomSmoothSpeed = 0.2f; // Suavização do zoom

    [Header("Smooth Transition Settings")]
    [Tooltip("Velocidade da suavização ao mudar de target. Quanto menor, mais lento e suave.")]
    public float smoothSpeed = 0.5f; // Velocidade da suavização
    private Vector3 velocity = Vector3.zero; // Usado pelo SmoothDamp para calcular a suavização

    private Vector3 targetPosition; // Posição do target no plano X e Z
    private bool isSmoothing = false; // Indica se a câmera está em transição suave
    private float targetDistance; // Distância suavizada da câmera ao target
    private float zoomVelocity; // Usado para suavizar o zoom

    private void Awake()
    {
        // Configura o Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Garante que só haja uma instância do CameraFollow
        }

        // Inicializa a distância suavizada
        targetDistance = distance;
    }

    // Método para atualizar o target da câmera
    public void UpdateTarget(GameObject newTarget)
    {
        if (newTarget != target) // Verifica se o target mudou
        {
            target = newTarget;
            isSmoothing = true; // Ativa a suavização
        }

        if (target != null)
        {
            Debug.Log($"CameraFollow: Now following {target.name}");
        }
        else
        {
            Debug.LogWarning("CameraFollow: Target is null.");
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Obtém a posição do target apenas no plano X e Z (ignora o Y)
            Vector3 newTargetPosition = new Vector3(
                target.transform.position.x, // Posição X do target
                0, // Ignora a altura do target (Y)
                target.transform.position.z // Posição Z do target
            );

            // Suaviza a distância da câmera ao target
            distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, zoomSmoothSpeed);

            // Se a câmera está em transição suave
            if (isSmoothing)
            {
                // Usa SmoothDamp para mover a câmera suavemente
                transform.position = Vector3.SmoothDamp(transform.position, newTargetPosition + new Vector3(xOffset, yPosition, -distance), ref velocity, smoothSpeed);

                // Verifica se a câmera chegou perto o suficiente do target
                if (Vector3.Distance(transform.position, newTargetPosition + new Vector3(xOffset, yPosition, -distance)) < 0.1f)
                {
                    isSmoothing = false; // Desativa a suavização
                }
            }
            else
            {
                // Move a câmera diretamente para a posição do target (sem suavização)
                transform.position = newTargetPosition + new Vector3(xOffset, yPosition, -distance);
            }

            // Mantém a câmera olhando para o objeto (apenas no plano X e Z)
            transform.LookAt(new Vector3(newTargetPosition.x, 0, newTargetPosition.z));
        }
    }

    void Update()
    {
        // Verifica se há um target e se o scroll do mouse foi usado
        if (target != null)
        {
            HandleZoom();
        }
    }

    private void HandleZoom()
    {
        // Obtém o movimento do scroll do mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            // Ajusta a distância alvo da câmera com base no scroll
            targetDistance -= scroll * zoomSpeed;

            // Limita a distância alvo entre os valores mínimo e máximo
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

            Debug.Log($"CameraFollow: Zoom target adjusted. Current target distance: {targetDistance}");
        }
    }
}