using UnityEngine;

[CreateAssetMenu(fileName = "New Town", menuName = "TownSystem/Town")]
public class TownInfo : ScriptableObject
{
    //since scriptable objects arent intended to be mutable (and itd suck to try and make it) SpecialTileEventHold has any and all info about town upgrades
    //maybe if i can think of smarter way later but this makes the most sense to me for now
    public string TownName;
    public int baseMoneyGeneration;

    public int maxMoneyLevel, maxDefenseLevel, maxUnitLevel;
    public int moneyUpgradeCost, defenseUpgradeCost,unitUpgradeCost;
    public float upgradeCostMultiplier;
    public int restCost;
    public int baseFame;

    [Tooltip("Each entry corrseponds with the equal defense level")]
    public EnemyEncounter[] defenseEncounters;

}
