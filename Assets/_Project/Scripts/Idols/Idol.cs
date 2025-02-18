using UnityEngine;

public class Idol : MonoBehaviour
{
    public IdolData data;  // O ScriptableObject com as informações do Idol
    public int position;
    public HexTile currentTile; // Hexágono atual em que o Idol está posicionado

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
        transform.position = startTile.transform.position;
        Debug.Log($"{data.idolName} começou no hexágono {currentTile.GetTileIndex()}.");
    }

    public void MoveToNextHex()
    {
        if (currentTile != null && currentTile.GetNextHex() != null)
        {
            currentTile = currentTile.GetNextHex();
            Vector3 newPosition = currentTile.transform.position;

            // Ajuste a posição para ficar acima do tile (aqui o valor de Y é aumentado)
            newPosition.y += 1f; // Ajuste o valor conforme necessário para que o Idol fique acima

            transform.position = newPosition;
            Debug.Log($"{data.idolName} se moveu para o hexágono {currentTile.GetTileIndex()}.");
        }
        else
        {
            Debug.Log($"{data.idolName} não pode se mover para o próximo hexágono.");
        }
    }


    public void MoveToPreviousHex()
    {
        if (currentTile != null && currentTile.GetPreviousHex() != null)
        {
            currentTile = currentTile.GetPreviousHex();
            Vector3 newPosition = currentTile.transform.position;

            // Ajuste a posição para ficar acima do tile (aqui o valor de Y é aumentado)
            newPosition.y += 1f; // Ajuste o valor conforme necessário para que o Idol fique acima

            transform.position = newPosition;
            Debug.Log($"{data.idolName} se moveu para o hexágono {currentTile.GetTileIndex()}.");
        }
        else
        {
            Debug.Log($"{data.idolName} não pode se mover para o hexágono anterior.");
        }
    }

    // Função para mostrar os botões no Inspector (apenas para testes)
#if UNITY_EDITOR
    void OnGUI()
    {
        if (GUILayout.Button("Mover para o próximo Hexágono"))
        {
            MoveToNextHex();
        }

        if (GUILayout.Button("Mover para o hexágono anterior"))
        {
            MoveToPreviousHex();
        }
    }
#endif
}
