using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    private GameObject target;
    private Vector3 offset;
    public float startDelay = 1f; // Tempo de espera para procurar o objeto
    public float yPosition = 10f; // Altura fixa da câmera
    public float distance = 10f; // Distância da câmera ao objeto
    public float xOffset = 0f; // Deslocamento lateral da câmera

    void Start()
    {
        StartCoroutine(FindTargetWithDelay());
    }

    IEnumerator FindTargetWithDelay()
    {
        yield return new WaitForSeconds(startDelay);

        target = GameObject.FindGameObjectWithTag("Idol");

        if (target != null)
        {
            UpdateCameraPosition();
        }
    }

    void UpdateCameraPosition()
    {
        if (target != null)
        {
            // Calcula a nova posição baseada na distância configurada
            Vector3 targetPosition = target.transform.position;
            transform.position = new Vector3(
                targetPosition.x + xOffset,
                yPosition,
                targetPosition.z - distance
            );

            // Atualiza o offset para manter a mesma relação com o objeto
            offset = transform.position - new Vector3(target.transform.position.x, 0, target.transform.position.z);
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Define diretamente a posição sem interpolação
            transform.position = new Vector3(
                target.transform.position.x + xOffset,
                yPosition,
                target.transform.position.z - distance
            );

            // Mantém a câmera olhando para o objeto
            transform.LookAt(new Vector3(target.transform.position.x, 0, target.transform.position.z));
        }
    }
}