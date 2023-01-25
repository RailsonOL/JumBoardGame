using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DiceController : MonoBehaviour
{
    [Header("Dices Infos and Logs")]
    public List<GameObject> dices;
    public List<int> diceNumberResults;
    public List<bool> isRolling;

    [Header("Results")]
    public int allDiceResult;
    public bool diceThrown = false;

    private List<Rigidbody> diceRbs;
    private List<Vector3> dicePositions;


    [Header("Cameras")]
    public Camera diceCam;
    public Camera mainCam;

    [Header("UI")]
    public TextMeshProUGUI diceResultText;

    void Start()
    {
        // Inicializando as listas
        diceNumberResults = new List<int>();
        diceRbs = new List<Rigidbody>();
        dicePositions = new List<Vector3>();
        isRolling = new List<bool>();

        for (int i = 0; i < dices.Count; i++)
        {
            diceNumberResults.Add(0);
            diceRbs.Add(dices[i].GetComponent<Rigidbody>());
            dicePositions.Add(dices[i].transform.position);
            isRolling.Add(false);
        }
    }

    private void OnTriggerStay(Collider col)
    {
        for (int i = 0; i < dices.Count; i++)
        {
            if (!isRolling[i] && col.transform.parent.gameObject == dices[i])
            {
                SetResult(i, col);
            }
        }
    }

    void Update()
    {
        // Se todos os dados estiverem parados...
        if (isRolling.All(x => x == false) && diceThrown)
        {
            StartCoroutine(UpdateResultText(0.5f));
            diceThrown = false;
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < dices.Count; i++)
        {
            // if (isRolling[i])
            // {
            //     Debug.Log("O dado " + i + " está rolando...");
            // }

            // Se o dado estiver rolando e parar de se mover...
            if (isRolling[i] && diceRbs[i].velocity.magnitude == 0)
            {
                // Checa novamente se parou mesmo
                StartCoroutine(WaitAndCalculateResult(i));
            }
        }
    }

    void SetResult(int index, Collider col)
    {
        diceNumberResults[index] = (7 - int.Parse(col.name));
    }

    IEnumerator UpdateResultText(float seconds)
    {
        diceResultText.SetText("...");
        // Pausa a execução do script por alguns segundos
        yield return new WaitForSeconds(seconds);
        allDiceResult = diceNumberResults.Sum();
        Debug.Log("Resultado total: " + allDiceResult);
        diceResultText.SetText(allDiceResult.ToString());
    }

    public void ViewDice()
    {
        diceCam.gameObject.SetActive(!diceCam.gameObject.activeSelf);
        mainCam.gameObject.SetActive(!mainCam.gameObject.activeSelf);
    }


    IEnumerator WaitAndCalculateResult(int index)
    {
        // Pausa a execução do script por 0.4 segundos
        yield return new WaitForSeconds(0.4f);

        if (diceRbs[index].velocity.magnitude == 0)
        {
            isRolling[index] = false;
        }
        else
        {
            isRolling[index] = true;
        }
    }

    public void StopDice()
    {
        for (int i = 0; i < dices.Count; i++)
        {
            diceRbs[i].velocity = Vector3.zero;
            diceRbs[i].angularVelocity = Vector3.zero;
            isRolling[i] = false;
        }
    }

    public void RollDice()
    {
        ResetDice();

        for (int i = 0; i < dices.Count; i++)
        {
            float x = Random.Range(0, 1500);
            float y = Random.Range(0, 1500);
            float z = Random.Range(0, 1500);

            diceRbs[i].AddForce(dices[i].transform.up * 1000);
            diceRbs[i].AddTorque(x, y, z);
            isRolling[i] = true;
            diceThrown = true;
        }
    }

    public void ResetDice()
    {
        for (int i = 0; i < dices.Count; i++)
        {
            dices[i].transform.position = dicePositions[i];
            dices[i].transform.rotation = Quaternion.identity;
            diceNumberResults[i] = 0;
            isRolling[i] = false;
            allDiceResult = 0;
            diceResultText.SetText("00");
        }
    }
}