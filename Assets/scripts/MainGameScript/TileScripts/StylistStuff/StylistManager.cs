using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class StylistManager : NetworkBehaviour
{
    public RenderTexture[] playerTextures;
    public RawImage playerDisplay;
 


    public GameObject confirmLeave;

    
    public GameObject MainMenu;
    public SpriteLibraryAsset spriteLibrary;

    public TextMeshProUGUI displayText;

    public GameObject headChanger;
    public GameObject victoryChanger;

    public characterEditor currentEditor;

    private int currentFaceLook;
    private int currentHairLook;
    private playerData currentPlayer;

    void Start()
    {
        playerDisplay.texture = playerTextures[NetworkData.Instance.currentPlayer];
        currentEditor = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].GetComponent<characterEditor>();
        currentPlayer = NetworkData.Instance.GetCurrentPlayer();
        for (int i = 0; i < currentPlayer.unlockedFaceIds.Count; i++)
        {
            if (currentPlayer.unlockedFaceIds[i] == currentPlayer.playerFace)
            {
                currentFaceLook = i;
                break;
            }
        }

        for (int i = 0; i < currentPlayer.unlockedHairIds.Count; i++)
        {
            if (currentPlayer.unlockedHairIds[i] == currentPlayer.playerHair)
            {
                currentHairLook = i;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetMainMenuChangeActive(bool toBe)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId)) { return; }
        SetMainMenuChangeActiveRpc(toBe);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetMainMenuChangeActiveRpc(bool toBe)
    {
        MainMenu.SetActive(toBe);
    }

    public void LeaveButton()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        LeaveButtonRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void LeaveButtonRpc()
    {
        confirmLeave.SetActive(true);
        MainMenu.SetActive(false);
        displayText.transform.parent.gameObject.SetActive(false);
    }

    public void ConfirmLeave()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        ConfirmLeaveRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ConfirmLeaveRpc()
    {
        confirmLeave.SetActive(false);
        TileEventManager.Instance.EndEvent();
    }


    public void DontLeave()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }

        DontLeaveRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void DontLeaveRpc()
    {
        confirmLeave.SetActive(false);
        displayText.transform.parent.gameObject.SetActive(true);
    }

    public void SetHeadChanger(bool tobe)
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        SetHeadChangerRpc(tobe);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetHeadChangerRpc(bool tobe)
    {
        headChanger.SetActive(tobe);
    }

    public void NextHair()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        NextHairRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void NextHairRpc()
    {
        if(currentHairLook + 1 >= currentPlayer.unlockedHairIds.Count) { currentHairLook = 0; }
        else { currentHairLook += 1; }

        currentEditor.setHair(currentHairLook);

    }

    public void PreviousHair()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        PreviousHairRpc();

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PreviousHairRpc()
    {
        if (currentHairLook - 1 < 0) { currentHairLook = currentPlayer.unlockedHairIds.Count - 1; } 
        else { currentHairLook -= 1; }
        currentEditor.setHair(currentHairLook);


    }
    public void NextFace()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        NextFaceRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void NextFaceRpc()
    {
        if (currentFaceLook + 1 >= currentPlayer.unlockedFaceIds.Count) { currentFaceLook = 0; }
        else { currentFaceLook += 1; }

        currentEditor.setHair(currentFaceLook);

    }

    public void PreviousFace()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        PreviousFaceRpc();

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PreviousFaceRpc()
    {
        if (currentFaceLook - 1 < 0) { currentFaceLook = currentPlayer.unlockedFaceIds.Count - 1; }
        else { currentFaceLook -= 1; }
        currentEditor.setHair(currentFaceLook);


    }


}
