using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InventoryChangeScript : NetworkBehaviour
{
    private int currentInventory = 0;
    [SerializeField] public DisplayInventory inventoryDisplay;


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
    }


    public void InventoryForward()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            InventoryForwardRpc();
        }
    }
    [Rpc( SendTo.ClientsAndHost,RequireOwnership = false)]
    private void InventoryForwardRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, rpcstuff.Receive.SenderClientId)) { return; }

        if (currentInventory >= 2) { return; }

        currentInventory += 1;

        inventoryDisplay.CreateDisplay( NetworkData.Instance.currentPlayer, currentInventory);
    }
   /* [ClientRpc( RequireOwnership = false)]
    private void InventoryForwardClientRpc(int inv)
    {
        currentInventory = inv;
        inventoryDisplay.CreateDisplay(currentInventory, NetworkData.Instance.currentPlayer);
    }*/

    public void InventoryBack()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            InventoryBackRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void InventoryBackRpc(RpcParams rpcstuff = default)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, rpcstuff.Receive.SenderClientId)) { return; }

        if (currentInventory <= 0) { return;  }

        currentInventory -= 1;
        inventoryDisplay.CreateDisplay(NetworkData.Instance.currentPlayer, currentInventory);
    }

    
}
