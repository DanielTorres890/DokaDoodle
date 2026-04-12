using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/Special/LoyaltyTag")]
public class LoyaltyBuff : BuffBase
{
    public string LoyaltyTag;
    public override void BuffEffect(EntityStats whoWon)
    {
      

    }

    public override void OnApply(EntityStats stats)
    {
        stats.loyaltyTags.Add(LoyaltyTag);
        base.OnApply(stats);
    }
    public override void OnRemove(EntityStats stats)
    {
        stats.loyaltyTags.Remove(LoyaltyTag);
        base.OnRemove(stats);
    }
}
