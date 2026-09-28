using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class AllyViewNetwork : NetworkBehaviour
{
    public AllyMainViewer display;

    public GameObject allyStateMenu;
    public GameObject allyView;
    public GameObject healItemView;
    public int currentAllyIndex;

    public DisplayHealItems healItemDisplay;
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
        if (NetworkData.Instance.GetCurrentPlayer().partyMembers[allyIndex].boardMovementState == PlayerFollowingStates.KnockedOut) { return; }

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
    public void ShowHealingItems()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.ContainsHealingItem(NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][0])) { return; }
        ShowHealingItemsRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ShowHealingItemsRpc()
    {
        healItemDisplay.CreateDisplay(NetworkData.Instance.currentPlayer);
        allyStateMenu.SetActive(false);
        healItemView.gameObject.SetActive(true);

    }
    public void ReturnFromHealingItem()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ReturnFromHealingItemRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReturnFromHealingItemRpc()
    {
        allyStateMenu.SetActive(true);
        healItemView.SetActive(false);

    }
    public void HealAlly(int itemIndex)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!NetworkData.Instance.ContainsHealingItem(NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][0])) { return; }

        HealAllyRpc(itemIndex);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void HealAllyRpc(int itemIndex)
    {
        var ally = NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex];
        var itemInv = NetworkData.Instance.playerInventories[NetworkData.Instance.currentPlayer][0];
        
        ItemBase chosenItem = itemInv.container[itemIndex].item;
        FoodItem theFood = chosenItem as FoodItem;

        int healedAmount = ally.stats[Attributes.Health];
        ally.healHp(theFood.buffs[0].value);
        healedAmount = ally.stats[Attributes.Health] - healedAmount;

        ClientChecks.Instance.HealAllyDisplayRpc(healedAmount, ally.name);
        allyStateMenu.SetActive(false);
        healItemView.SetActive(false);
        ClientChecks.Instance.displayText.endEvent.AddListener(delegate
        {
            
            allyView.SetActive(true);
            display.UpdateDisplay(NetworkData.Instance.GetCurrentPlayer().partyMembers);
        });

        
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
