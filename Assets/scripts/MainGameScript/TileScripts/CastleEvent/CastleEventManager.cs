using System.Collections.Generic;
using System.Net.Mail;
using Unity.Netcode;
using UnityEngine;

public class CastleEventManager : NetworkBehaviour
{
    public GameObject MainMenu;

    public GameObject RestMenu; //okay not really a menu but frick u

    public GameObject textObject;

    void Start()
    {
        NetworkData.Instance.GetCurrentPlayer().playerSpawnTile = NetworkData.Instance.GetCurrentPlayer().curTileId;
        NetworkData.Instance.GetCurrentPlayer().playerSpawnMap = NetworkData.Instance.GetCurrentPlayer().curMap;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetMainMenuVisible(bool visibility)
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetMainMenuVisibleRpc(visibility);
        }

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetMainMenuVisibleRpc(bool visibility)
    {
        MainMenu.SetActive(visibility);

    }

    public void SetRestMenuVisible(bool visibility)
    {

        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            SetRestMenuVisibleRpc(visibility);
            SetMainMenuVisible(!visibility);
        }

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetRestMenuVisibleRpc(bool visibility)
    {

        RestMenu.SetActive(visibility);

    }

    public void Rest()
    {

        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            RestRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void RestRpc()
    {
       

        NetworkData.Instance.GetCurrentPlayer().healHp(99999);
        MainMenu.SetActive(false);
        RestMenu.SetActive(false);
        textObject.SetActive(false);
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines = new List<string>() { "Mimimiimimimi" };

        TileEventManager.Instance.EndEvent();

    }
    public void Leave()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            LeaveRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void LeaveRpc()
    {

        MainMenu.SetActive(false);
        RestMenu.SetActive(false);
        textObject.SetActive(false);
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines = new List<string>(NetworkData.Instance.currentEvent.endDialouge);

        TileEventManager.Instance.EndEvent();
    }
}
