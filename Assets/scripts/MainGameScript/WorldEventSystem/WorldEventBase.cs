using UnityEngine;

public abstract class WorldEventBase : ScriptableObject
{
    [TextArea(5,15)]
    public string ActivateText;
    [TextArea(5, 15)]
    public string DeactivateText;

    public Sprite eventDisplay;

    [Tooltip("IF this is a quest with a specific condition to occur (ex: quest activating after 10 days) then add a scriptable for it")]
    public QuestCondition MainQuestCondition;
    public abstract void OnActivate();

    public abstract void OnDeactivate();

    public abstract bool Condition(int turns);
}




