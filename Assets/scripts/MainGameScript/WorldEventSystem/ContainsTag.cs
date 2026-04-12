using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Condition", menuName = "WorldEvents/QuestCondition/ContainsTag")]
public class ContainsTag : QuestCondition
{
    public string loyaltyTag;
    public override bool CanBeginQuest()
    {
       return NetworkData.Instance.GetCurrentPlayer().loyaltyTags.Contains(loyaltyTag);
    }

    
}
