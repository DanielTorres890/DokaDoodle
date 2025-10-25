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
        if(!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

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
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
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
        Debug.Log("tHE PANEL HAS BEN HIDENEND");
        menuUI.SetActive(false);
    }

   
}
