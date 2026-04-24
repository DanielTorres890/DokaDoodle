using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/NonDamage/Guard")]
public class GuardAbility : AttackBase
{
    [Tooltip("By percentage bc i'm not consistant ie: 30 = 30%")]
    
    public BuffBase[] gainStatus;

    public override void OnStartUp(GameObject caster)
    {
        var stats = caster.GetComponent<AbilityManager>();
        Debug.Log("I should have gained buff ");
        foreach(var status in  gainStatus)
        {
            stats.stats.GainStatus(status);
        }
        
    }

    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking, float chargeDuration, Vector3 origin, Vector3 direction)
    {
        var stats = caster.GetComponent<AbilityManager>();
        return base.WeaponEffect(caster, time, whereiscaster, casterLooking, chargeDuration, origin, direction);
    }
}
