using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Trap Item Object", menuName = "Inventory System/Items/Trap")]
public class TrapItem : ItemBase
{
    public BaseTrap trap;

    public AttackBase inCombatAttack;

    public override void ItemInfoCheck(int player, int itemId)
    {
        if (!NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }


        ClientChecks.Instance.ShowConfirmItemButtonsRpc(player, itemId, 0);

        UnityAction subscription = null;
        subscription = () =>
        {
            Subscribe(player, itemId);

        };
        
        ClientChecks.Instance.onItemUse.AddListener(subscription);
        ClientChecks.Instance.onItemUse.AddListener(delegate { ClientChecks.Instance.onItemUse.RemoveListener(subscription); });
        
    }
    
    public override void PerformItemEffect(int player, InventoryObject inventory)
    {

        
        FreeMover.Instance.EndFreeCamera();
        base.PerformItemEffect(player, inventory);
        Debug.Log("we got here so thats pretty cool"); //the reason why we don't deploy the trap here is bc we don't actually wanna use the trap unless the event gets invoked
    }
    public override void InCombatAction(AbilityManager user)
    {
        if(!NetworkManager.Singleton.IsHost) { return; }
        inCombatAttack.WeaponEffect(user.gameObject, Time.time, user.transform.position, user.transform.eulerAngles, 0f);
    }
    private void Subscribe(int player, int itemId)
    {
        FreeMover.Instance.FreeCamera();
        FreeMover.Instance.onTileSelect.AddListener(trap.DeployTrap);
        FreeMover.Instance.onTileSelect.AddListener(delegate { ClientChecks.Instance.ConfirmBuffRpc(player, itemId, 0); });
        FreeMover.Instance.onUndoFree.AddListener(delegate { ClientChecks.Instance.UndoItemUseRpc(); });
        FreeMover.Instance.onUndoFree.AddListener(delegate { ClientChecks.Instance.onItemUse.RemoveListener(delegate { Subscribe(player, itemId); }); });
        
}
}
