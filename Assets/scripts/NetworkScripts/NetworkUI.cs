using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button host;
    [SerializeField] private Button client;

    /*
    private void Awake()
    {
        host.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        client.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
    */
}
