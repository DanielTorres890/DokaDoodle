using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Condition", menuName = "WorldEvents/QuestCondition/EventOccuring")]
public class EventOccuring : QuestCondition
{
    public WorldEventBase eventToCheck;
   
    public override bool CanBeginQuest()
    {
        foreach(var worldEvent in WorldEventManager.Instance.activeWorldEvents)
        {
            if(worldEvent.eventId == WorldEventManager.Instance.worldDatabase.GetId[eventToCheck])
            {
                return true;
            }
        }
        return false;
    }
}
