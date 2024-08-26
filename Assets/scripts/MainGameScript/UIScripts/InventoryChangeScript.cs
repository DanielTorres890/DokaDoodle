using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InventoryChangeScript : NetworkBehaviour
{
    private int currentInventory = 0;
    [SerializeField] public DisplayInventory inventoryDisplay;

    [Rpc( SendTo.ClientsAndHost,RequireOwnership = false)]
    public void InventoryForwardRpc()
    {
        
        if (currentInventory >= 2) { return; }

        currentInventory += 1;

        inventoryDisplay.CreateDisplay(currentInventory, NetworkData.Instance.currentPlayer);
    }
   /* [ClientRpc( RequireOwnership = false)]
    private void InventoryForwardClientRpc(int inv)
    {
        currentInventory = inv;
        inventoryDisplay.CreateDisplay(currentInventory, NetworkData.Instance.currentPlayer);
    }*/
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void InventoryBackRpc()
    {
        Debug.Log(currentInventory);
        if (currentInventory <= 0) { return;  }

        currentInventory -= 1;
        inventoryDisplay.CreateDisplay(currentInventory,NetworkData.Instance.currentPlayer);
    }

    
}
