using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food Object", menuName = "Inventory System/Items/BuffItem")]
public class StatusItem : ItemBase
{
    public BuffBase[] StatusEffects;
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
}
