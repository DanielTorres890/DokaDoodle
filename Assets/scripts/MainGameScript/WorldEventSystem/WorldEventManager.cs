using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class WorldEventManager : NetworkBehaviour
{
    public WorldEventDataBase worldDatabase;

    public WorldEventBase[] randomEvents;
    public WorldEventBase[] questEvents;


    [DoNotSerialize] public List<WorldEventBase> eventsToActivate = new List<WorldEventBase>(); //the reason this is like this is bc an event won't just activate right away,
    public List<WorldEventWrapper> activeWorldEvents = new List<WorldEventWrapper>();
    [DoNotSerialize] public List<WorldEventBase> eventsToDeactivate = new List<WorldEventBase>();

    public static WorldEventManager Instance;
    public int turns;
    public int days;
    public int weeks;

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
        if (turns >= NetworkData.Instance.playerCount)
        {
            days += 1;
            turns = 0;
            Debug.Log("NEXT DAY");
        }
        if (days >= 6)
        {
            weeks++;
            days = 0;
            Debug.Log("NEXT WEEK");
            foreach (var events in activeWorldEvents)
            {
                events.Progress();
            }

            if (IsHost && Random.Range(0,100) > 0)
            {
                AddEventRpc(Random.Range(0, randomEvents.Length));
            }
            else if (IsHost && (eventsToActivate.Count > 0 || eventsToDeactivate.Count > 0))
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
        eventsToActivate.Add(worldDatabase.GetEvent[eventId]);

        if (IsHost)
        {
            ClientChecks.Instance.WorldEventRpc();
        }
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void NoEventRpc()
    {
        ClientChecks.Instance.TurnStartChecks();
    }
}
