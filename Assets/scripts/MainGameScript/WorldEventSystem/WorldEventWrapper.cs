using UnityEngine;
[System.Serializable]
public class WorldEventWrapper
{

    public int eventId;
    public int turnsPassed;


    public WorldEventWrapper(int eventId)
    {
        this.eventId = eventId;
        turnsPassed = 0;
    }
    public void Progress()
    {
        turnsPassed++;
        var thisEvent = WorldEventManager.Instance.worldDatabase.GetItem[eventId];
        //passes the number of days instead of turns
        if (thisEvent.Condition(turnsPassed / NetworkData.Instance.maxPlayers)) //maybe just pass it the wrapper instead of turns ?
        {
            WorldEventManager.Instance.eventsToDeactivate.Add(thisEvent);
            if(thisEvent.MainQuestCondition != null) { WorldEventManager.Instance.completeWorldEvents.Add(thisEvent); }
        }
    }
}
