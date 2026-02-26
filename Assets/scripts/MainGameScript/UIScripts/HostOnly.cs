using Unity.Netcode;
using UnityEngine;

public class HostOnly : MonoBehaviour
{
    public GameObject toShow;
    public void ChangeVisible(bool  visible)
    {
        if(NetworkManager.Singleton.IsHost && NetworkData.Instance.IsAllowed(NetworkData.Instance.GetCurrentPlayer().playerNumber,NetworkManager.Singleton.LocalClientId))
        {
            toShow.SetActive(visible);
        }
    }
}
