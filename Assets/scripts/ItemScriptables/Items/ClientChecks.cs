using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientChecks : NetworkBehaviour
{
    [SerializeField] private ItemDataBase _itemData;
    public static ClientChecks Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmBuffRpc(int player, int itemId) 
    {
        if(NetworkData.Instance.IsAllowed(player, NetworkManager.Singleton.LocalClientId)) { return; }

        _itemData.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][0]);
    }
}
