using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/NonDamage/Buff")]
public class BuffAbility : AttackBase
{

    public BuffBase[] StatusEffects;


    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking)
    {

        int[] buffids = new int[StatusEffects.Length];

        for (int i = 0; i < StatusEffects.Length;i++)
        {
            buffids[i] = NetworkData.Instance.buffDataBase.GetId[StatusEffects[i]];
        }

        caster.GetComponent<AbilityManager>().IGainedBuffRpc(buffids);
        return base.WeaponEffect(caster, time, whereiscaster, casterLooking);
        
    }
}
