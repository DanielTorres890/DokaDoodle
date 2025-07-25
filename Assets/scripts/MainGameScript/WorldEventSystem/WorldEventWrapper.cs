using UnityEngine;
[System.Serializable]
public class WorldEventWrapper
{

    public int eventId;
    public int daysPassed;


    public WorldEventWrapper(int eventId)
    {
        this.eventId = eventId;
        daysPassed = 0;
    }
    public void Progress()
    {
        daysPassed++;
        var thisEvent = WorldEventManager.Instance.worldDatabase.GetItem[eventId];
        if (thisEvent.Condition(daysPassed)) //maybe just pass it the wrapper instead of turns ?
        {
            WorldEventManager.Instance.eventsToDeactivate.Add(thisEvent);
            if(thisEvent.MainQuestCondition != null) { WorldEventManager.Instance.completeWorldEvents.Add(thisEvent); }
        }
    }
}
