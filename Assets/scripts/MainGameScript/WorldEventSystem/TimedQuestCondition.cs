using UnityEngine;

//because i realize its a big ambiguous the difference between this and the condition function in world events is that this is specifically for the start while the other is when it ends (ik mbmb)
[CreateAssetMenu(fileName = "New Quest Condition", menuName = "WorldEvents/QuestCondition/TimedCondition")]
public class TimedQuestCondition : QuestCondition
{
    public int TotalDaysToBegin;
    public override bool CanBeginQuest()
    {
        return WorldEventManager.Instance.weeks * WorldEventManager.Instance.daysPerWeek + WorldEventManager.Instance.days >= TotalDaysToBegin;
    }
}