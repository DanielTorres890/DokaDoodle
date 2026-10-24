using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Condition", menuName = "WorldEvents/QuestCondition/ReqPlayerCount")]
public class ReqPlayerCount : QuestCondition
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int minimumPlayerCount = 2;

    public override bool CanBeginQuest()
    {
        return minimumPlayerCount <= NetworkData.Instance.players.Count;
    }

    
}
