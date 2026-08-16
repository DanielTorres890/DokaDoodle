using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class WorldEventBase : ScriptableObject
{
    
    public List<string> ActivateText;
    [TextArea(5, 15)]
    public string DeactivateText;

    public Sprite eventDisplay;

    [Tooltip("Not required but if it contains will occur")]
    public CutSceneInfo startCutscene;
    public CutSceneInfo finishCutscene;

    [Tooltip("IF this is a quest with a specific condition to occur (ex: quest activating after 10 days) then add a scriptable for it")]
    public QuestCondition MainQuestCondition;
 

    //I dont like making literally everything take a random number but i'm reallllyyy not sure where i'd even want to generate it since everywhere feels like a bad place
    public virtual void OnActivate(int randomNum)
    {
        if (startCutscene)
        {
            WorldEventManager.Instance.currentCutscene = startCutscene;
            if (NetworkManager.Singleton.IsHost) { WorldEventManager.Instance.SyncStartCutsceneRpc(WorldEventManager.Instance.worldDatabase.GetId[this]); }

        }
    }

    public virtual void OnDeactivate()
    {
        if(finishCutscene) 
        { 
            WorldEventManager.Instance.currentCutscene = finishCutscene;
            if (NetworkManager.Singleton.IsHost) { WorldEventManager.Instance.SyncFinishCutsceneRpc(WorldEventManager.Instance.worldDatabase.GetId[this]); }
            
        }
    }
    public abstract bool Condition(int turns);
    
}




