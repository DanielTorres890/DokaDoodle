using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientChecks : NetworkBehaviour
{
    [SerializeField] private DisplayInventory display;


    public static ClientChecks Instance { get; private set; }

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
}
