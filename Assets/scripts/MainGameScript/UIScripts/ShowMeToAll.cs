using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ShowMeToAll : NetworkBehaviour
{
    [SerializeField] private GameObject menuUI;



    public void ShowUIToAll ()
    {
        if(!NetworkData.Instance.IsAllowed(NetworkData.Instance.GetCurrentPlayer().playerNumber, NetworkManager.Singleton.LocalClientId)) { return; }

        ShowUIToAllServerRpc();
    }

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void ShowUIToAllServerRpc()
    {

        ShowUIToAllClientRpc();
    }

	[Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
	public void ShowUIToAllClientRpc()
    {
        menuUI.SetActive(true);
        
    }

    public void HideUIFromAll()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.GetCurrentPlayer().playerNumber, NetworkManager.Singleton.LocalClientId)) { return; }
        HideUiFromAllServerRpc();
    }

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	private void HideUiFromAllServerRpc()
    {
        
        HideUiFromAllClientRpc();
    }
	[Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]

	private void HideUiFromAllClientRpc()
    {
        menuUI.SetActive(false);
    }

   
}
