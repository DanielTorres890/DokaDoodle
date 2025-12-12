 using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class InventoryChangeScript : NetworkBehaviour
{
    private int currentInventory = 0;
    [SerializeField] public DisplayInventory inventoryDisplay;

    [SerializeField] private string[] inventoryNames;
    [SerializeField] private TextMeshProUGUI nameText;
    [TextArea(3,12)]
    [SerializeField] private string[] inventoryToolTips;
    [SerializeField] private TextMeshProUGUI mouseOverText;
    [SerializeField] private TextMeshProUGUI sizeText;
    public void ResetDisplay()
    {
        if(NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer,NetworkManager.Singleton.LocalClientId))
        {
            ResetDisplayRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ResetDisplayRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, rpcstuff.Receive.SenderClientId)) { return; }

        
        inventoryDisplay.CreateDisplay(NetworkData.Instance.currentPlayer, currentInventory);
        if(sizeText)
        sizeText.text = inventoryDisplay.inventory.container.Count.ToString() + "/" + inventoryDisplay.inventory.MAXSIZE.ToString();
    }

    //for some god forsaken reason my button keeps forcing itself to subscribe to inventory forward which makes 0 sense
    public void InventoryForwardFrickU()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            InventoryForwardRpc();
        }
    }
    [Rpc( SendTo.ClientsAndHost,RequireOwnership = false)]
    public void InventoryForwardRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, rpcstuff.Receive.SenderClientId)) { return; }

        if (currentInventory >= 2) { return; }

        currentInventory += 1;
        if (nameText)
        {
            nameText.text = inventoryNames[currentInventory];
            mouseOverText.text = inventoryToolTips[currentInventory];
        }
        inventoryDisplay.CreateDisplay( NetworkData.Instance.currentPlayer, currentInventory);
        if (sizeText)
            sizeText.text = inventoryDisplay.inventory.container.Count.ToString() + "/" + inventoryDisplay.inventory.MAXSIZE.ToString();
    }
   /* [ClientRpc( RequireOwnership = false)]
    private void InventoryForwardClientRpc(int inv)
    {
        currentInventory = inv;
        inventoryDisplay.CreateDisplay(currentInventory, NetworkData.Instance.currentPlayer);
    }*/

    public void InventoryBackFrickU()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            InventoryBackRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void InventoryBackRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, rpcstuff.Receive.SenderClientId)) { return; }

        if (currentInventory <= 0) { return;  }

        currentInventory -= 1;
        if(nameText)
        {
            nameText.text = inventoryNames[currentInventory];
            mouseOverText.text = inventoryToolTips[currentInventory];
        }
        
        inventoryDisplay.CreateDisplay(NetworkData.Instance.currentPlayer, currentInventory);
        if (sizeText)
            sizeText.text = inventoryDisplay.inventory.container.Count.ToString() + "/" + inventoryDisplay.inventory.MAXSIZE.ToString();
    }
    public void currentMouseOver()
    {
        mouseOverText.text = inventoryToolTips[currentInventory];
    }
    
}
