using UnityEngine;

[CreateAssetMenu(fileName = "New Trap Object", menuName = "TileSystem/Traps/FlatDmgTrap")]

public class FlatDmgTrap : BaseTrap
{
    public int dmgAmount;
    public int turnsDeath;
    public override void TrapEffect(playerData whom)
    {
        bool die = whom.healHp(-dmgAmount);

        if (die) { whom.death(turnsDeath); }

    }
}
