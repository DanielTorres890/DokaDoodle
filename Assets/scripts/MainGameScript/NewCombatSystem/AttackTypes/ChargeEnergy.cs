using UnityEngine;

[CreateAssetMenu(fileName = "New Attack Object", menuName = "Abilities/NonDamage/EnergyCharge")]
public class ChargeEnergy : BuffAbility
{

    public float ChargePercent;
    public override GameObject WeaponEffect(GameObject caster)
    {

        var obj = base.WeaponEffect(caster);

        var myManager = caster.GetComponent<AbilityManager>();
        myManager.ChangeEnergy(myManager.maxEnergy * (ChargePercent / 100f));
      
        return obj;

    }
}
