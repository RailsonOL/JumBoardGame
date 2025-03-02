using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Essent Data", menuName = "Game/Essent Data")]
public class EssentData : ScriptableObject
{
    public int id;
    public Sprite icon;
    public string essentName;
    public int essence;
    public List<Card> initialCards;
    public string description;
}
