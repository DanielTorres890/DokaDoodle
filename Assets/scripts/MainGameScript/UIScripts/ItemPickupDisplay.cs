using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemPickupDisplay : MonoBehaviour
{

    // Start is called before the first frame update

    public static ItemPickupDisplay Instance;
    void Awake()
    {
        Instance = this;
    }
    public void HideMe()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        gameObject.SetActive(false);
    }

    
}
