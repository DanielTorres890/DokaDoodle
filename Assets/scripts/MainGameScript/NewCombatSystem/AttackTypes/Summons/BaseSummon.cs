using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/NonDamage/Summon")]
public class BaseSummon : AttackBase
{
    public EnemyBase summonSO;

    public override GameObject WeaponEffect(GameObject caster)
    {
        var attack = Instantiate(startUpPrefab);
        attack.transform.position = caster.transform.position + caster.transform.TransformDirection(offset);
        attack.transform.rotation = caster.transform.rotation;
        attack.transform.localScale = ablitySize;
        
        return attack;
    }
    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking)
    {
        
        var npcfab =  base.WeaponEffect(caster, time, whereiscaster, casterLooking);
        npcfab.GetComponent<SummonDespawn>().SpawnInRpc(PlayerCombatManager.Instance.EnemyDataBase.GetId[summonSO]);
        return npcfab;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
