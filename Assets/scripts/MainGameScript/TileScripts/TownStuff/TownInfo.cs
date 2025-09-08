using UnityEngine;

[CreateAssetMenu(fileName = "New Town", menuName = "TownSystem/Town")]
public class TownInfo : ScriptableObject
{
    //since scriptable objects arent intended to be mutable (and itd suck to try and make it) SpecialTileEventHold has any and all info about town upgrades
    //maybe if i can think of smarter way later but this makes the most sense to me for now
    public string TownName;
    public int baseMoneyGeneration;
    public int maxMoneyLevel;
    public int upgradeCost;

}
