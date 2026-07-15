using System;
using Unity.Netcode;
using UnityEngine;

public class LeaveSlot : MonoBehaviour
{
    public GameObject leaveButton;
   
    void Update()
    {
        if (NetworkData.Instance.IsHost) { leaveButton.SetActive(false); return; }
        int thisPlayerNum = NetworkData.Instance.ClientNumToPlayerNum(NetworkManager.Singleton.LocalClientId);
        if (!NetworkData.Instance.readyPlayers[thisPlayerNum]) { leaveButton.SetActive(false); return; }

        leaveButton.SetActive(true);

    }

    public void RemoveSlot()
    {
        NetworkData.Instance.RemoveOrderRpc(NetworkManager.Singleton.LocalClientId);
    }
}
