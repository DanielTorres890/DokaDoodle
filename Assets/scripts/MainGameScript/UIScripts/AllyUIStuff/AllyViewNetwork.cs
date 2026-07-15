using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class AllyViewNetwork : NetworkBehaviour
{
    public AllyMainViewer display;

    public GameObject allyStateMenu;
    public GameObject allyView;

    public int currentAllyIndex;
    public override void OnNetworkSpawn()
    {
       
    }
    public void UpdateDisplay()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            UpdateDisplayRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateDisplayRpc()
    {
        display.UpdateDisplay(NetworkData.Instance.GetCurrentPlayer().partyMembers);
    }


    public void EnterAllyStateMenu(int allyIndex)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        EnterAllyStateMenuRpc(allyIndex);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnterAllyStateMenuRpc(int allyIndex)
    {
        currentAllyIndex = allyIndex;
        allyStateMenu.SetActive(true);
        allyView.SetActive(false);

    }
    public void ExitAllyStateMenu()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ExitAllyStateMenuRpc();

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ExitAllyStateMenuRpc()
    {
 
        allyStateMenu.SetActive(false);
        allyView.SetActive(true);
        display.UpdateDisplay(NetworkData.Instance.GetCurrentPlayer().partyMembers);

    }

    public void HealAlly()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.ContainsHealingItem(NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][0])) { return; }

        HealAllyRpc();

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void HealAllyRpc()
    {
        var ally = NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex];
        var itemInv = NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][0];
        
        ItemBase chosenItem = itemInv.container[0].item;
        int allyMissingHp = ally.stats[Attributes.MaxHealth] - ally.stats[Attributes.Health];

        foreach (var slot in itemInv.container)
        {
            if(chosenItem is not FoodItem) { chosenItem = slot.item; continue; }
            if(slot.item is not FoodItem) { continue; }

            if(!IsHealingItem(chosenItem as FoodItem)) { chosenItem = slot.item; continue; }
            if(!IsHealingItem(slot.item as FoodItem)) { continue; }

            FoodItem chosenFood = chosenItem as FoodItem;
            FoodItem otherFood = slot.item as FoodItem;

            int chosenFoodHealing = chosenFood.HealingAmount();
            int otherFoodHealing = otherFood.HealingAmount();

            if(chosenFoodHealing > otherFoodHealing)
            {
                if(otherFoodHealing >= allyMissingHp)
                {
                    chosenItem = slot.item; 
                }
            }
            else
            {
                if(chosenFoodHealing < allyMissingHp)
                {
                    chosenItem = slot.item;
                }
            }

        }
        FoodItem theFood = chosenItem as FoodItem;
        theFood.PerformItemEffect(NetworkData.Instance.currentPlayer, itemInv);
        ExitAllyStateMenu();
    }
    private bool IsHealingItem(FoodItem item)
    {
        bool canHeal = false;
        foreach (var buff in (item as FoodItem).buffs)
        {
            if (buff.attribute == Attributes.Health) { canHeal = true; break; }
        }
        return canHeal;
    }
    public void ReturnToOwner()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ReturnToOwnerRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReturnToOwnerRpc()
    {
        var currentAlly = NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex];
        currentAlly.SetFollowingState(PlayerFollowingStates.FollowingOwner);
        if(currentAlly.curTileId == NetworkData.Instance.GetCurrentPlayer().curTileId) { currentAlly.SetFollowingState(PlayerFollowingStates.WithOwner); }
        ExitAllyStateMenu();
    }

    public void PickTileToHold()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        PickTileToHoldRpc();
    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickTileToHoldRpc()
    {

        FreeMover.Instance.FreeCamera();
        allyStateMenu.SetActive(false);


        FreeMover.Instance.onTileSelect.AddListener(LinkedToTile);

        FreeMover.Instance.onUndoFree.AddListener(delegate { ExitAllyStateMenu(); });
        FreeMover.Instance.onTileSelect.AddListener(delegate { ExitAllyStateMenu(); });
        FreeMover.Instance.onTileSelect.AddListener(delegate { FreeMover.Instance.EndFreeCamera(); });

    }

    public void LinkedToTile(int tileId)
    {
        NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex].targetTile = tileId;
        NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex].SetFollowingState (PlayerFollowingStates.HoldTile);
    }
}
