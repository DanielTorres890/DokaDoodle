using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StealItemUI : NetworkBehaviour
{
    //this pretty much doesn't need to be a singleton but ill be darned if you make me make a seperate display script
    public static StealItemUI instance;

    public Button goBackButton;
    public DisplayInventory inventoryDisplay;
    public InventoryChangeScript inventoryChangeScript;
    public int stealingPlayer;
    public int stolenPlayer;

    private int stolenItem;
    private int stolenItemInv;

    public GameObject confirmButtons;
    public GameObject mainItemDisplay;

    public UnityEvent<bool,int> finishSteal;

    public override void OnNetworkSpawn()
    {
        
        instance = this;
        gameObject.SetActive(false);


    }
    public void SetUp(int stealerId, int stolenId)
    {
        if (!IsHost) { return; }
        SetUpRpc(stealerId, stolenId);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetUpRpc(int stealerId, int stolenId)
    {
        stealingPlayer = stealerId;
        stolenPlayer = stolenId;
        stolenItemInv = 0;

        inventoryDisplay.inventory = NetworkData.Instance.playerInventories[stolenId][0];
        inventoryChangeScript.whomsInventory = stolenId;
        inventoryChangeScript.whoInControl = stealingPlayer;
        

        inventoryDisplay.CreateDisplay(stolenId, stolenItemInv);
        gameObject.SetActive(true);
        confirmButtons.SetActive(false);
        mainItemDisplay.SetActive(true);
    }
    
    public void StealItem(int itemNum, int inventoryNum)
    {
        if (!NetworkData.Instance.IsAllowed(stealingPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        StealItemRpc(itemNum, inventoryNum);
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void StealItemRpc(int itemNum, int inventoryNumber)
    {
        stolenItem = itemNum;
        stolenItemInv = inventoryNumber;
        confirmButtons.SetActive(true);
        mainItemDisplay.SetActive(false);

    }
    public void GoBack()
    {
        if (!NetworkData.Instance.IsAllowed(stealingPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        GoBackRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void GoBackRpc()
    {
        confirmButtons.SetActive(false);
        mainItemDisplay.SetActive(true);
    }
    public void Finish()
    {
        if (!NetworkData.Instance.IsAllowed(stealingPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        FinishRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void FinishRpc()
    {

        var stolenPlayerInventory = NetworkData.Instance.playerInventories[stolenPlayer][stolenItemInv].container;
        var stealingPlayerInventory = NetworkData.Instance.playerInventories[stealingPlayer][stolenItemInv];


        bool success = stealingPlayerInventory.AddItem(stolenPlayerInventory[stolenItem].item);

        if(IsHost)
        NetworkData.Instance.LoseItemRpc(stolenPlayer, stolenItem, stolenItemInv);
        finishSteal.Invoke(success, stolenItemInv);
        gameObject.SetActive(false);
    }
}
