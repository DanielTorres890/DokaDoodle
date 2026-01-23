using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class WorldEventManager : NetworkBehaviour, IDataPersistance
{
    public WorldEventDataBase worldDatabase;

    public WorldAndWeight[] randomEvents;
    public WorldEventBase[] questEvents;


    [DoNotSerialize] public List<WorldEventBase> eventsToActivate = new List<WorldEventBase>(); //the reason this is like this is bc an event won't just activate right away,
    public List<WorldEventWrapper> activeWorldEvents = new List<WorldEventWrapper>();
    [DoNotSerialize] public List<WorldEventBase> eventsToDeactivate = new List<WorldEventBase>();

    public List<WorldEventBase> completeWorldEvents = new List<WorldEventBase>();

    public static WorldEventManager Instance;

    public UnityEvent onDayChange; //once again im not a huge fan but better than some alternatives
    public int turns;
    public int days;
    public int weeks;

    public int daysPerWeek;
    public bool firstTime = false; //im a bum so im sticking duct tape to fix this


    public AudioClip roundStartClip;
    private void Awake()
    {
        if(Instance == null) 
        { 
            Instance = this;
            //bc me noob and dont know how to actually handle this
        }
        
        

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
        if(!firstTime) 
        { 
            firstTime = true;
            ClientChecks.Instance.TurnStartChecks();
            return;
        }
        turns++;
        
        
        if (turns >= NetworkData.Instance.maxPlayers)
        {
            
            days += 1;
            onDayChange.Invoke();
            turns = 0;

        }

        if(NetworkData.Instance.IsAllowed())
        {
            SFXManager.Instance.PlaySFX(roundStartClip);
        }
        

        if (days >= daysPerWeek)
        {
            days = 0;
            weeks++;
      
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
            PopUpManager.Instance.PerformPopUp(1, true);
            if (IsHost && Random.Range(0,100) > 30)
            {
                int totalWeight = 0;
                foreach(var weighted in randomEvents)
                {
                    totalWeight += weighted.weight;
                }
                
                int randomWeight = Random.Range(0, totalWeight);
                int currentWeight = randomEvents[0].weight;
                foreach(var weighted in randomEvents)
                {
                    if(randomWeight < currentWeight)
                    {
                        AddEventRpc(worldDatabase.GetId[weighted.worldEvent]);
                        break;
                    }

                }
                //AddEventRpc(worldDatabase.GetId[randomEvents[Random.Range(0, randomEvents.Length)]]);
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
        Debug.Log("Step 2");
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
[System.Serializable]
public class WorldAndWeight
{
    public WorldEventBase worldEvent;
    public int weight;
}