using UnityEngine;

[CreateAssetMenu(fileName = "New Trap Object", menuName = "TileSystem/Traps/PercentageTrap")]
public class PercentageTrap : BaseTrap
{
    [Tooltip("Make it between 0-1 where .5 means half health dmg")]
    public float dmgPerecent;

    //since the number changes i need either a new variable or something bru

    public override void TrapEffect(playerData whom)
    {
        bool die = whom.healHp(Mathf.RoundToInt(-dmgPerecent * whom.stats[Attributes.MaxHealth]));

        if (die) { whom.death(); }
    }

    public override string TrapString(playerData whom)
    {
        return ActivateText + Mathf.RoundToInt(-dmgPerecent * whom.stats[Attributes.MaxHealth]).ToString() + " dmg";
    }
}
