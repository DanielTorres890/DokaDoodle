using Unity.Netcode;
using UnityEngine;

public class ResetOnEndNetwork : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("am i here? ");
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }


    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("did i attempt a reset? ");
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        if(NetworkManager.Singleton.LocalClientId == clientId && gameObject)
        Destroy(gameObject);

    }
    
}
