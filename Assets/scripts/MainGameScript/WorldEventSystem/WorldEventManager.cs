using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class WorldEventManager : NetworkBehaviour, IDataPersistance
{
    public WorldEventDataBase worldDatabase;

    public WorldEventBase[] randomEvents;
    public WorldEventBase[] questEvents;


    [DoNotSerialize] public List<WorldEventBase> eventsToActivate = new List<WorldEventBase>(); //the reason this is like this is bc an event won't just activate right away,
    public List<WorldEventWrapper> activeWorldEvents = new List<WorldEventWrapper>();
    [DoNotSerialize] public List<WorldEventBase> eventsToDeactivate = new List<WorldEventBase>();

    public List<WorldEventBase> completeWorldEvents = new List<WorldEventBase>();

    public static WorldEventManager Instance;
    public int turns;
    public int days;
    public int weeks;

    public int daysPerWeek;
    private void Awake()
    {
        if(Instance == null) { Instance = this; }

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ProgressDay()
    {
        turns++;
        
        
        if (turns >= NetworkData.Instance.maxPlayers)
        {
            days += 1;
            turns = 0;
            Debug.Log("NEXT DAY TotalPlayers: " + NetworkData.Instance.playerCount);
        }
        if (days >= daysPerWeek)
        {
            weeks++;
            days = 0;
            Debug.Log("NEXT WEEK");
            foreach (var qEvent in questEvents)
            {
                if (qEvent.MainQuestCondition != null && qEvent.MainQuestCondition.CanBeginQuest() && !AlreadyActive(qEvent) && !AlreadyComplete(qEvent))
                {
                    eventsToActivate.Add(qEvent);
                }
                
            }

            foreach (var events in activeWorldEvents)
            {
                events.Progress();
            }

            //i feel like theres a way to do weekly money gain with events (like on week change) vs this but im not sure since events
            //are kinda preplanned? maybe the special tile event hold could have the function/subscribe here but id need to think more
            for(int i = 0; i < NetworkData.Instance.maxPlayers; i++)
            {
                playerData player = NetworkData.Instance.players[i];
                foreach (int tileid in player.ownedTowns)
                {
                    var curTile = MapTileSpecialEvents.Instance.mapTiles[0][tileid];
                    player.playerInfo[PlayerInfo.money] += (curTile.townMoneyLevel + 1) * NetworkData.Instance.TownInfoDataBase.GetItem[curTile.townId].baseMoneyGeneration;
                    Debug.Log("Gained Money from town");
                }

            }
            if (IsHost && Random.Range(0,100) > 90)
            {
                AddEventRpc(worldDatabase.GetId[randomEvents[Random.Range(0, randomEvents.Length)]]);
            }
            if (IsHost && (eventsToActivate.Count > 0 || eventsToDeactivate.Count > 0))
            {
                ClientChecks.Instance.WorldEventRpc();
            }
            else if (IsHost)
            {
                NoEventRpc(); //idk if this is the only way but the sphaghetti is starting to get real
            }
            return;
        }
        ClientChecks.Instance.TurnStartChecks();
    }

    
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void AddEventRpc(int eventId)
    {

        if (AlreadyActive(worldDatabase.GetItem[eventId])) { return; }
        
        eventsToActivate.Add(worldDatabase.GetItem[eventId]);      
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void NoEventRpc()
    {
        ClientChecks.Instance.TurnStartChecks();
    }
    private bool AlreadyActive(WorldEventBase eventToCheck)
    {
        
        foreach (var eventWrapper in activeWorldEvents)
        {
            if (eventWrapper.eventId == worldDatabase.GetId[eventToCheck]) { return true; }
        }

        return false;
    }

    private bool AlreadyComplete(WorldEventBase eventToCheck)
    {
        foreach (var completeEvent in completeWorldEvents)
        {
            if (completeEvent == eventToCheck) { return true; }
        }

        return false;

    }

    public void LoadData(GameData data)
    {
        activeWorldEvents = data.worldEvents;
    }

    public void SaveData(ref GameData data)
    {
        data.worldEvents = activeWorldEvents;

    }
}
