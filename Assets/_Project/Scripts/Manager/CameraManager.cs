using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Cameras")]
    public Camera essentFollowCamera; // Câmera que segue o Essent
    public Camera diceCamera; // Câmera do dado
    public Camera fullBoardCamera; // Câmera de visualização completa do tabuleiro

    private CameraFollow cameraFollow; // Referência ao script CameraFollow
    private Camera lastActiveCamera; // Armazena a última câmera ativa antes de ativar a câmera do dado ou full board

    private void Awake()
    {
        // Configura o Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Obtém o componente CameraFollow da câmera que segue o Essent
        cameraFollow = essentFollowCamera.GetComponent<CameraFollow>();

        // Inicializa a última câmera ativa como a câmera que segue o Essent
        lastActiveCamera = essentFollowCamera;
    }

    // Método para ativar a câmera que segue o Essent
    public void ActivateEssentFollowCamera(GameObject target = null)
    {
        DeactivateAllCameras();
        essentFollowCamera.gameObject.SetActive(true);
        lastActiveCamera = essentFollowCamera; // Atualiza a última câmera ativa

        if (target != null)
        {
            cameraFollow.UpdateTarget(target);
        }
    }

    // Método para ativar a câmera do dado
    public void ActivateDiceCamera()
    {
        if (diceCamera.gameObject.activeSelf)
        {
            // Se a câmera do dado já estiver ativa, volta para a última câmera ativa
            DeactivateAllCameras();
            lastActiveCamera.gameObject.SetActive(true);
        }
        else
        {
            // Se a câmera do dado não estiver ativa, ativa ela e armazena a última câmera ativa
            DeactivateAllCameras();
            diceCamera.gameObject.SetActive(true);
            lastActiveCamera = GetActiveCameraBeforeDice(); // Armazena a última câmera ativa
        }
    }

    // Método para ativar a câmera de visualização completa do tabuleiro
    public void ActivateFullBoardCamera()
    {
        if (fullBoardCamera.gameObject.activeSelf)
        {
            // Se a câmera full board já estiver ativa, volta para a última câmera ativa
            DeactivateAllCameras();
            lastActiveCamera.gameObject.SetActive(true);
        }
        else
        {
            // Se a câmera full board não estiver ativa, ativa ela e armazena a última câmera ativa
            DeactivateAllCameras();
            fullBoardCamera.gameObject.SetActive(true);
            lastActiveCamera = GetActiveCameraBeforeFullBoard(); // Armazena a última câmera ativa
        }
    }

    // Método para desativar todas as câmeras
    private void DeactivateAllCameras()
    {
        essentFollowCamera.gameObject.SetActive(false);
        diceCamera.gameObject.SetActive(false);
        fullBoardCamera.gameObject.SetActive(false);
    }

    // Método para obter a câmera ativa antes de ativar a câmera do dado
    private Camera GetActiveCameraBeforeDice()
    {
        if (essentFollowCamera.gameObject.activeSelf)
        {
            return essentFollowCamera;
        }
        else if (fullBoardCamera.gameObject.activeSelf)
        {
            return fullBoardCamera;
        }
        return lastActiveCamera; // Caso nenhuma câmera esteja ativa, retorna a última câmera armazenada
    }

    // Método para obter a câmera ativa antes de ativar a câmera full board
    private Camera GetActiveCameraBeforeFullBoard()
    {
        if (essentFollowCamera.gameObject.activeSelf)
        {
            return essentFollowCamera;
        }
        else if (diceCamera.gameObject.activeSelf)
        {
            return diceCamera;
        }
        return lastActiveCamera; // Caso nenhuma câmera esteja ativa, retorna a última câmera armazenada
    }
}