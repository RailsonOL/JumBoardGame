using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Essent Data", menuName = "Game/Essent Data")]
public class EssentData : ScriptableObject
{
    public string essentName;
    public int essence;
    public List<Card> initialCards;
    public string description;
}
