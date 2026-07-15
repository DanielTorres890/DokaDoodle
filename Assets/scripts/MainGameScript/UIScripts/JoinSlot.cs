using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class JoinSlot : MonoBehaviour
{
    public int slotNumber;
    public GameObject myButton;
 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (NetworkData.Instance.IsHost) { myButton.SetActive(false); return; }
        if(slotNumber >= NetworkData.Instance.maxPlayers) { myButton.SetActive(false); return; }
        foreach(int id in NetworkData.Instance.clientOrder)
        {
            if (id == Convert.ToInt32(NetworkManager.Singleton.LocalClientId))
            {
                myButton.SetActive(false);
                return;
            }
        }

        if (NetworkData.Instance.clientOrder[slotNumber] == -1)
            myButton.SetActive(true);
        else
            myButton.SetActive(false);
    }
    public void PickSlot()
    {
        NetworkData.Instance.AddOrderClientRpc(slotNumber, NetworkManager.Singleton.LocalClientId);

    }
}
