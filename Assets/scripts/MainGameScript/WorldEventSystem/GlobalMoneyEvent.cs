using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/MoneyEvent")]
public class GlobalMoneyEvent : TimedWEvent
{
    public float multiplier;
    public override void OnActivate()
    {
        NetworkData.Instance.globalMoneyMultiplier *= multiplier;
    }
    public override void OnDeactivate()
    {
        NetworkData.Instance.globalMoneyMultiplier /= multiplier;
    }


}
