using UnityEngine;
using System.Collections;

public class Idol : MonoBehaviour
{
    public IdolData data;
    public int position;
    public HexTile currentTile;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float arcHeight = 0.5f;

    public bool isMoving = false;

    public bool IsAlive()
    {
        return data.essence > 0;
    }

    public void ModifyEssence(int amount)
    {
        data.essence += amount;
        Debug.Log($"{data.idolName} agora tem {data.essence} de essência.");
    }

    public void UseSpecialAbility()
    {
        Debug.Log($"{data.idolName} usou sua habilidade especial!");
    }

    public void Initialize(HexTile startTile)
    {
        currentTile = startTile;
        transform.position = startTile.transform.position + Vector3.up * 2f;
        Debug.Log($"{data.idolName} começou no hexágono {currentTile.GetTileIndex()}.");
    }

    public void MoveHexes(int amount)
    {
        if (isMoving) return;

        if (amount == 0)
        {
            Debug.Log($"{data.idolName} não se moveu pois a quantidade de casas é zero.");
            return;
        }

        StartCoroutine(MoveMultipleHexes(amount));
    }

    private IEnumerator MoveMultipleHexes(int amount)
    {
        int remainingMoves = Mathf.Abs(amount);
        bool isForward = amount > 0;

        while (remainingMoves > 0 && !isMoving)
        {
            HexTile nextTile = isForward ?
                currentTile.GetNextHex() :
                currentTile.GetPreviousHex();

            if (nextTile == null)
            {
                Debug.Log($"{data.idolName} não pode continuar se movendo. Caminho bloqueado.");
                break;
            }

            yield return StartCoroutine(MoveSmoothly(nextTile, isForward));
            remainingMoves--;
        }

        Debug.Log($"{data.idolName} completou seu movimento de {Mathf.Abs(amount)} casas.");
    }

    private IEnumerator MoveSmoothly(HexTile targetTile, bool isForward)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = targetTile.transform.position + Vector3.up * 2f;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * moveSpeed;
            float percentageComplete = elapsedTime;

            // Movimento horizontal com curva de suavização
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0f, 1f, percentageComplete));

            // Efeito de pulo
            float yOffset = Mathf.Sin(percentageComplete * Mathf.PI) * jumpHeight;

            // Arco entre hexágonos
            float arcOffset = (1f - Mathf.Abs(percentageComplete - 0.5f) * 2f) * arcHeight;

            currentPosition.y += yOffset + arcOffset;

            transform.position = currentPosition;

            yield return null;
        }

        transform.position = targetPosition;
        currentTile = targetTile;
        isMoving = false;
    }

    // Função para mostrar os botões no Inspector (apenas para testes)
#if UNITY_EDITOR
    void OnGUI()
    {
        if (GUILayout.Button("Mover para o próximo Hexágono"))
        {
            MoveHexes(1);
        }

        if (GUILayout.Button("Mover para o hexágono anterior"))
        {
            MoveHexes(-1);
        }

        if (GUILayout.Button("Mover 3 casas para frente"))
        {
            MoveHexes(5);
        }

        if (GUILayout.Button("Mover 2 casas para trás"))
        {
            MoveHexes(-2);
        }
    }
#endif
}

