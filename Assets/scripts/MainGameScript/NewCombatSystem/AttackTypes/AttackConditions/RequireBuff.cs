using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/AttackConditions/RequireBuff")]
public class RequireBuff : AttackCondition
{
    public BuffBase requiredBuff;
    public int requiredAmount = 1;

    public override bool Condition(AbilityManager user)
    {
        int amount = 0;
        foreach(var status in user.stats.statuses)
        {
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == requiredBuff)
            {
                amount++;

            }
        }
        if(amount >= requiredAmount)
        {
            return true;
        }
        return false;
    }
}
