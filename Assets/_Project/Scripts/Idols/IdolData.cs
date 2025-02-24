using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idol Data", menuName = "Game/Idol Data")]
public class IdolData : ScriptableObject
{
    public string idolName;
    public int essence;
    public List<Card> initialCards;
    public string description;
}
