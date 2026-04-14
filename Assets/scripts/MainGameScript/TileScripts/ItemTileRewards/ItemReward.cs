using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item Tile Reward", menuName = "ItemTile/ItemReward")]
public class ItemReward : ItemTileRewards, ISerializationCallbackReceiver
{
    public ItemBase item;
    public override void GiveReward()
    {
        int itemId = NetworkData.Instance.playerInventories[0][item.determineType()].database.GetId[item];
        ClientChecks.Instance.ConfirmItemPickup(NetworkData.Instance.currentPlayer, itemId, item.determineType());
    }

    public void OnAfterDeserialize()
    {
        if(item == null) { return; }
        rewardName = item.itemName;
        rewardSprite = item.itemSprite;
    }

    public void OnBeforeSerialize()
    {
        
    }
}