using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EmploymentEventManager : NetworkBehaviour
{
    public RenderTexture[] playerTextures;
    public RawImage playerDisplay;
    public static EmploymentEventManager instance;

    public GameObject UIParent;
    public GameObject confirmLeave;
    public void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        for(int i = 0; i < NetworkData.Instance.playerSticks.Count; i++)
        {
            NetworkData.Instance.playerSticks[i].transform.position = new Vector3(50 * i, 100, 100);
        }

        playerDisplay.texture = playerTextures[NetworkData.Instance.currentPlayer];
    }

    public void ChangePlayerClass(int classId)
    {
        if(!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        ChangePlayerClassRpc(classId);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ChangePlayerClassRpc(int classId)
    {
        NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].GetComponent<characterEditor>().setClass(classId);
        NetworkData.Instance.GetCurrentPlayer().ChangeClass(NetworkData.Instance.classDataBase.GetItem[classId]);
    }

    public void LeaveButton()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        LeaveButtonRpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LeaveButtonRpc()
    {
        confirmLeave.SetActive(true);
        UIParent.SetActive(false);
    }


    public void ConfirmLeave()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        ConfirmLeaveRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ConfirmLeaveRpc()
    {
        confirmLeave.SetActive(false);
        UIParent.SetActive(false);
        TileEventManager.Instance.EndEvent();
    }

    public void DontLeave()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }

        DontLeaveRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void DontLeaveRpc()
    {
        confirmLeave.SetActive(false );
        UIParent.SetActive(true);
    }
}
