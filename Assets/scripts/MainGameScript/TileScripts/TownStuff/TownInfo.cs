using UnityEngine;

[CreateAssetMenu(fileName = "New Town", menuName = "TownSystem/Town")]
public class TownInfo : ScriptableObject
{
    public int baseMoneyGeneration;
    public int maxMoneyLevel;
    public int upgradeCost;

}
