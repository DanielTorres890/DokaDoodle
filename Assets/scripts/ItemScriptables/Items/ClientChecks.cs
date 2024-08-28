using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ClientChecks : NetworkBehaviour
{
    [SerializeField] private DisplayInventory display;


    public static ClientChecks Instance { get; private set; }

    public GameObject displayText;


    private void Awake()
    {
        Instance = this;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmBuffRpc(int player, int itemId, int inventoryNum)
    {


        NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][inventoryNum]);

        display.CreateDisplay(inventoryNum, player);

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmItemPickupRpc(int player, int itemId, int inventoryNum)
    {
        NetworkData.Instance.playerInventories[player][inventoryNum].AddItem(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId]);
        displayText.SetActive(true);
        displayText = ItemPickupDisplay.Instance.gameObject;
        displayText.GetComponentInChildren<TextMeshProUGUI>().text = "Obtained a <color=blue>" + NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].name + "</color>";
        StartCoroutine(displayItem());
    }



    private IEnumerator displayItem()
    {
        
        displayText.SetActive(true);
        while (displayText.activeSelf)
        {

            yield return null;
        }
        PlayerMoveManager.Instance.NextTurnRpc();

    }
}