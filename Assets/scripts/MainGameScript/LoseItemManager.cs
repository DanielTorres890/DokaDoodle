using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class LoseItemManager : NetworkBehaviour
{
    public static LoseItemManager instance;

    public DisplayInventory displayedItems;

    public TextMeshProUGUI inventorySizeText;

    public GameObject mainItemDisplay;
    public GameObject confirmButtons;
    public int currentPlayer;
    public int itemToLose;//based on position
    public int inventoryNum;
    public UnityEvent finishLose;

    public void Awake()
    {
        
    }
    public void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        instance = this;
        gameObject.SetActive(false);
        Debug.Log("Ive been set FRICK U MF" +instance.gameObject);
    }


    public void SetUp(int playerId, int inventoryNumber)
    {
        Debug.Log("I'm setting up?");
        if (!IsHost) { return; }
        Debug.Log("Dono walled or what");
        SetUpRpc(playerId, inventoryNumber);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetUpRpc(int playerId, int inventoryNumber)
    {
        Debug.Log("I'm setting up");
        currentPlayer = playerId;
        inventoryNum = inventoryNumber;

        displayedItems.inventory = NetworkData.Instance.playerInventories[playerId][inventoryNumber];
        inventorySizeText.text = "<color=red>"+ (displayedItems.inventory.container.Count).ToString() + "/" + displayedItems.inventory.MAXSIZE.ToString() + "</color>";
        displayedItems.CreateDisplay(playerId, inventoryNumber);
        gameObject.SetActive(true);
        confirmButtons.SetActive(false);
        mainItemDisplay.SetActive(true);
    }

    public void LoseItem(int itemNum, int inventoryNumber)
    {
        if (!NetworkData.Instance.IsAllowed(currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        Debug.Log("hello(?) i am very real right :) " + instance.gameObject);
        LoseItemRpc(itemNum, inventoryNumber);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void LoseItemRpc(int itemNum, int inventoryNumber)
    {
        Debug.Log(gameObject);
        itemToLose = itemNum;
        inventoryNum = inventoryNumber;
        confirmButtons.SetActive(true);
        mainItemDisplay.SetActive(false);
    }

    public void GoBack()
    {
        if (!NetworkData.Instance.IsAllowed(currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
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
        if (!NetworkData.Instance.IsAllowed(currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        FinishRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void FinishRpc()
    {
        if(IsServer) { NetworkData.Instance.LoseItemRpc(currentPlayer, itemToLose, inventoryNum); }

        for(int i = 0; i < NetworkData.Instance.playerInventories[currentPlayer].Count; i++)
        {
            var inventory = NetworkData.Instance.playerInventories[currentPlayer][i];
            if (inventory.container.Count > inventory.MAXSIZE)
            {
                if(IsHost) { SetUp(currentPlayer, i); }
                return;
            }
        }

        finishLose.Invoke();
        gameObject.SetActive(false);
    }
}
