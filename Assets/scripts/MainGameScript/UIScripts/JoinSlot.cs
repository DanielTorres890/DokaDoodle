using System;
using Unity.Netcode;
using UnityEngine;

public class JoinSlot : MonoBehaviour
{
    public int slotNumber;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkData.Instance.IsHost) { gameObject.SetActive(false); return; }
        if(slotNumber >= NetworkData.Instance.maxPlayers) { gameObject.SetActive(false); return; }
        foreach(int id in NetworkData.Instance.clientOrder)
        {
            if (id == Convert.ToInt32(NetworkManager.Singleton.LocalClientId))
            {
                gameObject.SetActive(false);
                return;
            }
        }

        if (NetworkData.Instance.clientOrder[slotNumber] == -1)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
    public void PickSlot()
    {
        NetworkData.Instance.AddOrderClientRpc(slotNumber, NetworkManager.Singleton.LocalClientId);

    }
}
