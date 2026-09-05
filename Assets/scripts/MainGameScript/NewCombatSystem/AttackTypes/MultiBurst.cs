using UnityEngine;
using static BurstSpawner;


[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/MagicAttacks/MultiAttack")]
public class MultiBurst : AttackBase
{

    [Header("Burst info")]
    public int attacksPerBurst;
    public int totalBursts;

    [Tooltip("How much to rotate the y axis from the initial spawn (may not always matter)")]
    public float startingRotationOffset;

    [Tooltip("If during a burst 20 things come out this is that single thing")]
    public GameObject individualAttack;

    [Tooltip("The time between each burst spawn")]
    public float interval;
    [Tooltip("How much it should rotate (y axis) per each interval")]
    public float intervalRotation;

    public AttackBase attackInfo;

    [Tooltip("how far of a starting offset they should have from the player")]
    public Vector3 attackOffset;

    [Tooltip("It selects a random direction to fall (will always fall) \nand these values are the maximums itll be offset (instead of falling straight down")]
    public float attackRadius;

    [Tooltip("This is the sound the invidual attack itself will make when falling")]
    public AudioClip individualAttackSound;

    public BurstTargetMode targetMode = BurstTargetMode.Circle;

    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking, float chargedDuration, Vector3 origin, Vector3 direction)
    {
        var spawnedObj =  base.WeaponEffect(caster, time, whereiscaster, casterLooking, chargedDuration, origin, direction);
        spawnedObj.transform.Rotate(Vector3.up * startingRotationOffset);
        return spawnedObj;
    }
}
