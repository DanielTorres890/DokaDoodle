using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShowMeToAll : NetworkBehaviour
{
    [SerializeField] private GameObject menuUI;



    public void ShowUIToAll ()
    {
        if (NetworkData.Instance.currentPlayer != Convert.ToInt32(NetworkManager.Singleton.LocalClientId) && !IsHost) { return; }

        ShowUIToAllServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowUIToAllServerRpc()
    {

        ShowUIToAllClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    public void ShowUIToAllClientRpc()
    {
        menuUI.SetActive(true);
    }

    public void HideUIFromAll()
    {
        if (NetworkData.Instance.currentPlayer != Convert.ToInt32(NetworkManager.Singleton.LocalClientId) && !IsHost) { return; }
        HideUiFromAllServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HideUiFromAllServerRpc()
    {
        
        HideUiFromAllClientRpc();
    }
    [ClientRpc(RequireOwnership = false)]

    private void HideUiFromAllClientRpc()
    {
        menuUI.SetActive(false);
    }

   
}
