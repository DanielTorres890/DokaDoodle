using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item Tile Reward", menuName = "ItemTile/MoneyReward")]
public class MoneyReward : ItemTileRewards
{
    public int moneyChange;

    public override void GiveReward()
    {
        NetworkData.Instance.GetCurrentPlayer().GainMoney(moneyChange);
        
        ClientChecks.Instance.GetMoney(moneyChange);
    }
}
