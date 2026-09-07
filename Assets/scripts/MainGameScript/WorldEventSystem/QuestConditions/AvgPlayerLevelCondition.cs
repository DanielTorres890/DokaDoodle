using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Condition", menuName = "WorldEvents/QuestCondition/AvgPlayerLevel")]
public class AvgPlayerLevelCondition : QuestCondition
{
    public int requiredAvgLevel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override bool CanBeginQuest()
    {

        int avgLevel = 0;
        foreach(var player in NetworkData.Instance.players)
        {
            avgLevel += player.playerInfo[PlayerInfo.level];
        }
        avgLevel /= NetworkData.Instance.players.Count;
        return avgLevel >= requiredAvgLevel;
    }
}
