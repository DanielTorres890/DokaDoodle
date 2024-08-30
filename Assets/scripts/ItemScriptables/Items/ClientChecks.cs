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

    public TextMeshProUGUI displayTxt;

    private void Awake()
    {
        Instance = this;
        displayTxt = displayText.GetComponentInChildren<TextMeshProUGUI>();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmBuffRpc(int player, int itemId, int inventoryNum)
    {


        NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][inventoryNum]);

        
        display.CreateDisplay(inventoryNum, player);
        display.gameObject.SetActive(false);

        displayTxt.text = NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].useText;
        StartCoroutine(usedItem());
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmItemPickupRpc(int player, int itemId, int inventoryNum)
    {
        NetworkData.Instance.playerInventories[player][inventoryNum].AddItem(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId]);
        displayText.SetActive(true);
        displayText = ItemPickupDisplay.Instance.gameObject;
        displayTxt.text = "Obtained a <color=blue>" + NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].name + "</color>";
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
    private IEnumerator usedItem()
    {
        displayText.SetActive(true);
        while (displayText.activeSelf)
        {

            yield return null;
        }
        display.gameObject.SetActive(true);
    }
}