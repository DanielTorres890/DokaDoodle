using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/NonDamage/Guard")]
public class GuardAbility : AttackBase
{
    [Tooltip("By percentage bc i'm not consistant ie: 30 = 30%")]
    public int PDmgReduction,MDmgReduction;


    public override void OnStartUp(GameObject caster)
    {
        var stats = caster.GetComponent<AbilityManager>();
        stats.stats.dmgReduction[AttackTypes.Physical] += PDmgReduction;
        stats.stats.dmgReduction[AttackTypes.Magic] += MDmgReduction;
    }

    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking, float chargeDuration)
    {
        var stats = caster.GetComponent<AbilityManager>();
        stats.stats.dmgReduction[AttackTypes.Physical] -= PDmgReduction;
        stats.stats.dmgReduction[AttackTypes.Magic] -= MDmgReduction;
        return base.WeaponEffect(caster, time, whereiscaster, casterLooking, chargeDuration);
    }
}
