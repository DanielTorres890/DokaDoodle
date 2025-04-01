using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Trap Item Object", menuName = "Inventory System/Items/Trap")]
public class TrapItem : ItemBase
{
    public BaseTrap trap;

    public override void ItemInfoCheck(int player, int itemId)
    {
        if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }

        FreeMover.Instance.FreeCamera();
        Debug.Log("how many times did we sub");
        FreeMover.Instance.onTileSelect.AddListener(trap.DeployTrap);
        FreeMover.Instance.onTileSelect.AddListener(delegate { ClientChecks.Instance.ConfirmBuffRpc(player, itemId, 0); });
        

    }
    
    public override void PerformItemEffect(int player, InventoryObject inventory)
    {

        inventory.RemoveItem(this);

        FreeMover.Instance.EndFreeCamera();
        Debug.Log("we got here so thats pretty cool"); //the reason why we don't deploy the trap here is bc we don't actually wanna use the trap unless the event gets invoked
    }
}
