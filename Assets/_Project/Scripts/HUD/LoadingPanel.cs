using UnityEngine;
using DG.Tweening;

public class LoadingPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform jumpingObject; // Objeto que vai pular (Image ou outro UI element)

    [Header("Settings")]
    [SerializeField] private float jumpHeight = 50f; // Altura do pulo (tamanho do arco)
    [SerializeField] private float jumpDistance = 100f; // Distância horizontal de cada pulo
    [SerializeField] private float jumpDuration = 0.5f; // Duração de cada pulo
    [SerializeField] private int numberOfJumps = 3; // Quantidade de pulos por ciclo

    private Vector2 originalPosition; // Posição original do objeto
    private Sequence jumpSequence; // Sequência de animação do DOTween

    private void Awake()
    {
        // Salva a posição original do objeto
        if (jumpingObject != null)
        {
            originalPosition = jumpingObject.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        // Inicia a animação quando o painel é ativado
        StartLoading();
    }

    private void OnDisable()
    {
        // Para a animação quando o painel é desativado
        StopLoading();
    }

    public void StartLoading()
    {
        // Inicia a animação de pulo
        if (jumpingObject != null)
        {
            CreateJumpSequence();
            jumpSequence.Play();
        }
    }

    public void StopLoading()
    {
        // Para a animação de pulo
        if (jumpSequence != null && jumpSequence.IsActive())
        {
            jumpSequence.Kill(); // Interrompe a animação
        }

        // Retorna o objeto à posição original
        if (jumpingObject != null)
        {
            jumpingObject.anchoredPosition = originalPosition;
        }
    }

    private void CreateJumpSequence()
    {
        // Cria uma sequência de pulos
        jumpSequence = DOTween.Sequence();

        // Define os pulos da direita para a esquerda
        for (int i = 0; i < numberOfJumps; i++)
        {
            // Calcula a posição horizontal do próximo pulo (movendo para a esquerda)
            float targetX = originalPosition.x - (jumpDistance * (i + 1));

            // Pulo para a esquerda (movimento horizontal + arco)
            jumpSequence.Append(jumpingObject.DOAnchorPos(new Vector2(targetX, originalPosition.y + jumpHeight), jumpDuration / 2).SetEase(Ease.OutQuad));
            // Volta para o chão (movimento horizontal + arco)
            jumpSequence.Append(jumpingObject.DOAnchorPos(new Vector2(targetX, originalPosition.y), jumpDuration / 2).SetEase(Ease.InQuad));
        }

        // Define os pulos da esquerda para a direita
        for (int i = 0; i < numberOfJumps; i++)
        {
            // Calcula a posição horizontal do próximo pulo (movendo para a direita)
            float targetX = originalPosition.x - (jumpDistance * (numberOfJumps - i - 1));

            // Pulo para a direita (movimento horizontal + arco)
            jumpSequence.Append(jumpingObject.DOAnchorPos(new Vector2(targetX, originalPosition.y + jumpHeight), jumpDuration / 2).SetEase(Ease.OutQuad));
            // Volta para o chão (movimento horizontal + arco)
            jumpSequence.Append(jumpingObject.DOAnchorPos(new Vector2(targetX, originalPosition.y), jumpDuration / 2).SetEase(Ease.InQuad));
        }

        // Repete a sequência indefinidamente
        jumpSequence.SetLoops(-1, LoopType.Restart);
    }
}