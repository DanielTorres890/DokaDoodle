using Unity.Netcode;
using UnityEngine;

public class TownUIManager : NetworkBehaviour
{
    public GameObject MainMenu;
    public GameObject LevelMenu;
    public GameObject RestMenu; //okay not really a menu but frick u

    //i could make one script and reuse it for each like i did in the main game menu but that gives me itchiness so im not going to
    public void SetMainMenuVisible(bool visibility)
    {
        if(NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetMainMenuVisibleRpc(visibility);
        }

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetMainMenuVisibleRpc(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            MainMenu.SetActive(visibility);
        }

    }

    public void SetLevelMenuVisible(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetLevelMenuVisibleRpc(visibility);
        }

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetLevelMenuVisibleRpc(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            LevelMenu.SetActive(visibility);
        }

    }
    public void SetRestMenuVisible(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetRestMenuVisibleRpc(visibility);
        }

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetRestMenuVisibleRpc(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            RestMenu.SetActive(visibility);
        }

    }

}
