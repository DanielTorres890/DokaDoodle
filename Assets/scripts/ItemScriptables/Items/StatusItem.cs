using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Object", menuName = "Inventory System/Items/BuffItem")]
public class StatusItem : ItemBase
{
    public BuffBase[] StatusEffects;

    [Tooltip("Whether you should be able to use the item when you already have the effect")]
    public bool stackable = true;
    public override void ItemInfoCheck(int player, int itemId)
    {
        if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }

        ClientChecks.Instance.ConfirmBuffRpc(player, itemId, 0);
    }

    public override void PerformItemEffect(int player, InventoryObject inventory)
    {

        foreach(var buff in StatusEffects)
        {
            NetworkData.Instance.players[player].GainStatus(buff);
        }
        
        inventory.RemoveItem(this);
    }

    //im probably gonna regret this later but im tired boss
    public override bool CanUse(int playerNum)
    {
        if(stackable) { return true; }

        if (StatusEffects[0] is not ForceRollBuff && StatusEffects[0] is not RollBuff) { return true; }

        foreach (var status in NetworkData.Instance.players[playerNum].statuses)
        {
            if(NetworkData.Instance.buffDataBase.GetItem[status.buffId] is ForceRollBuff || NetworkData.Instance.buffDataBase.GetItem[status.buffId] is RollBuff)
            {
                return false;
            }
        }
        return true;
    }
}
