using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/MoneyEvent")]
public class GlobalMoneyEvent : TimedWEvent
{
    public float multiplier;
    public override void OnActivate()
    {
        NetworkData.Instance.globalShopMultiplier *= multiplier;
        base.OnActivate();
    }
    public override void OnDeactivate()
    {
        NetworkData.Instance.globalShopMultiplier /= multiplier;
        base.OnDeactivate();
    }


}
