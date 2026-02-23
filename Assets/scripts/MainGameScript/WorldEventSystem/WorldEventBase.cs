using Unity.Netcode;
using UnityEngine;

public abstract class WorldEventBase : ScriptableObject
{
    [TextArea(5,15)]
    public string ActivateText;
    [TextArea(5, 15)]
    public string DeactivateText;

    public Sprite eventDisplay;

    [Tooltip("Not required but if it contains will occur")]
    public CutSceneInfo cutscene;

    [Tooltip("IF this is a quest with a specific condition to occur (ex: quest activating after 10 days) then add a scriptable for it")]
    public QuestCondition MainQuestCondition;
    public abstract void OnActivate();

    public virtual void OnDeactivate()
    {
        if(cutscene) 
        { 
            WorldEventManager.Instance.currentCutscene = cutscene;
            if (NetworkManager.Singleton.IsHost) { WorldEventManager.Instance.SyncCutsceneRpc(WorldEventManager.Instance.worldDatabase.GetId[this]); }
            
        }
    }

    public abstract bool Condition(int turns);
}




