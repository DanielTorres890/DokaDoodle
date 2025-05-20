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
        if(WorldEventManager.Instance.worldDatabase.GetEvent[eventId].Condition(daysPassed)) //maybe just pass it the wrapper instead of turns ?
        {
            WorldEventManager.Instance.eventsToDeactivate.Add(WorldEventManager.Instance.worldDatabase.GetEvent[eventId]);
        }
    }
}
