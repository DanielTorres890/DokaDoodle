using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/DrunkenStatus")]


public class Drunken : BuffBase
{
    [Tooltip("How strong the drunk effect is ")]
    public float strength;
    public override void BuffEffect(EntityStats whoWon)
    {
        
    }

    public override void OnApply(EntityStats stats)
    {
        //yo ngl this is some bs with how i did this icl
        stats.PostStatusStatCalc();
        base.OnApply(stats);
        
        
            

    }

  

    public override void OnRemove(EntityStats stats)
    {
        base.OnRemove(stats);
    }

    public override void OnEveryTick(AbilityManager manager)
    {

        if (NewCombatManager.instance)
        {
                             
            CombatantMovement playerMover = manager.GetComponent<CombatantMovement>();
            playerMover.additionalForces = new Vector3(Mathf.Sin(Time.time), 0, Mathf.Cos(Time.time)) * strength;

        }

    }
}
