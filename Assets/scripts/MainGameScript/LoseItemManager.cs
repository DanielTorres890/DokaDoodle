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
    private int currentPlayer;
    private int itemToLose;//based on position
    private int inventoryNum;
    public UnityEvent finishLose;


    public void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        if (instance == null)
        {
            instance = this;
            gameObject.SetActive(false);
        }


    }


    public void SetUp(int playerId, int inventoryNumber)
    {
        if (!IsServer) { return; }
        SetUpRpc(playerId, inventoryNumber);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetUpRpc(int playerId, int inventoryNumber)
    {
        currentPlayer = playerId;
        inventoryNum = inventoryNumber;

        displayedItems.inventory = NetworkData.Instance.playerInventories[playerId][inventoryNumber];
        inventorySizeText.text = "<color=red>"+ (displayedItems.inventory.MAXSIZE + 1).ToString() + "/" + displayedItems.inventory.MAXSIZE.ToString() + "</color>";
        displayedItems.CreateDisplay(playerId, inventoryNumber);
        gameObject.SetActive(true);
        confirmButtons.SetActive(false);
        mainItemDisplay.SetActive(true);
    }

    public void LoseItem(int itemNum, int inventoryNumber)
    {
        if (!NetworkData.Instance.IsAllowed(currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        LoseItemRpc(itemNum, inventoryNumber);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LoseItemRpc(int itemNum, int inventoryNumber)
    {
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
        finishLose.Invoke();
        gameObject.SetActive(false);
    }
}
