using Unity.Multiplayer.Playmode;
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
    public int inventoryNum;

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
        if (!IsServer) { return; }
        SetUpRpc(stealerId, stolenId);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetUpRpc(int stealerId, int stolenId)
    {
        stealingPlayer = stealerId;
        inventoryNum = 0;

        inventoryDisplay.inventory = NetworkData.Instance.playerInventories[stolenId][0];
        inventoryChangeScript.whomsInventory = stolenId;
        Debug.Log("I should be created");
        inventoryDisplay.CreateDisplay(stolenId, inventoryNum);
        gameObject.SetActive(true);
        confirmButtons.SetActive(false);
        mainItemDisplay.SetActive(true);
    }
    
    public void StealItem(int itemNum, int inventoryNum)
    {
        if (!NetworkData.Instance.IsAllowed(stealingPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        StealItemRpc(itemNum, inventoryNum);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void StealItemRpc(int itemNum, int inventoryNumber)
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
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void FinishRpc()
    {
        var stolenPlayerInventory = NetworkData.Instance.playerInventories[stolenPlayer][inventoryNum].container;
        var stealingPlayerInventory = NetworkData.Instance.playerInventories[stealingPlayer][inventoryNum];


        bool success = stealingPlayerInventory.AddItem(stolenPlayerInventory[stolenItem].item);
        stolenPlayerInventory.RemoveAt(stolenItem);
        finishSteal.Invoke(success,inventoryNum);
        gameObject.SetActive(false);
    }
}
