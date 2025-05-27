using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/WeaponAttacks/Dash Attack")]
public class DashAttack : AttackBase
{
    [Tooltip("Enter some Vector 3 values to act as dash. Z axis is forward. \nAlso more as a note to myself try and keep values between 0 and 1")]
    public Vector3 dashDirection;
    public float dashSpeed;
    public override GameObject WeaponEffect(GameObject caster)
    {

        caster.GetComponent<Rigidbody>().AddForce(caster.transform.TransformDirection(Vector3.forward * dashSpeed), ForceMode.Impulse);
        return base.WeaponEffect(caster);
    }
    public override GameObject WeaponEffect(GameObject caster, float time, Vector3 whereiscaster, Vector3 casterLooking)
    {
        var attack = base.WeaponEffect(caster,time, whereiscaster, casterLooking);
        attack.transform.SetParent(caster.transform); //im sure only good things can happen
        return attack;
    }

}
