using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Configurações de Scroll")]
    [SerializeField] private float scrollSpeedX = 0.1f; // Velocidade de scroll no eixo X
    [SerializeField] private float scrollSpeedY = 0f;   // Velocidade de scroll no eixo Y

    private RawImage rawImage;

    private void Start()
    {
        // Obtém o componente RawImage do objeto
        rawImage = GetComponent<RawImage>();

        if (rawImage == null)
        {
            Debug.LogError("RawImage não encontrado no objeto. Certifique-se de que este script está anexado a um objeto com RawImage.");
        }
    }

    private void Update()
    {
        // Atualiza o uvRect para criar o efeito de scroll
        if (rawImage != null)
        {
            rawImage.uvRect = new Rect(
                rawImage.uvRect.x + scrollSpeedX * Time.deltaTime,
                rawImage.uvRect.y + scrollSpeedY * Time.deltaTime,
                rawImage.uvRect.width,
                rawImage.uvRect.height
            );
        }
    }
}