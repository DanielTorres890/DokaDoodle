using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/MoneyEvent")]
public class GlobalMoneyEvent : TimedWEvent
{
    public float multiplier;
    public override void OnActivate(int randomNum)
    {
        NetworkData.Instance.globalShopMultiplier *= multiplier;
        base.OnActivate(randomNum);
    }
    public override void OnDeactivate()
    {
        NetworkData.Instance.globalShopMultiplier /= multiplier;
        base.OnDeactivate();
    }


}
