using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Condition", menuName = "WorldEvents/QuestCondition/EnteredMap")]
public class AreaEnterCondition : QuestCondition
{
    public int mapNum;

    public override bool CanBeginQuest()
    {
        return MapTileSpecialEvents.Instance.mapTiles[mapNum] != null;
    }
}
