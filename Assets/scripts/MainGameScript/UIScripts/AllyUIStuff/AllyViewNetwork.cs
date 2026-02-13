using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class AllyViewNetwork : NetworkBehaviour
{
    public AllyMainViewer display;

    public GameObject allyStateMenu;
    public GameObject allyView;

    public int currentAllyIndex;
    public override void OnNetworkSpawn()
    {
       
    }
    public void UpdateDisplay()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            UpdateDisplayRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void UpdateDisplayRpc()
    {
        display.UpdateDisplay(NetworkData.Instance.GetCurrentPlayer().partyMembers);
    }


    public void EnterAllyStateMenu(int allyIndex)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        EnterAllyStateMenuRpc(allyIndex);

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void EnterAllyStateMenuRpc(int allyIndex)
    {
        currentAllyIndex = allyIndex;
        allyStateMenu.SetActive(true);
        allyView.SetActive(false);

    }
    public void ExitAllyStateMenu()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ExitAllyStateMenuRpc();

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ExitAllyStateMenuRpc()
    {
 
        allyStateMenu.SetActive(false);
        allyView.SetActive(true);
        display.UpdateDisplay(NetworkData.Instance.GetCurrentPlayer().partyMembers);

    }


    public void ReturnToOwner()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ReturnToOwnerRpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ReturnToOwnerRpc()
    {
        var currentAlly = NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex];
        currentAlly.boardMovementState = PlayerFollowingStates.FollowingOwner;
        if(currentAlly.curTileId == NetworkData.Instance.GetCurrentPlayer().curTileId) { currentAlly.boardMovementState = PlayerFollowingStates.WithOwner; }
        ExitAllyStateMenu();
    }

    public void PickTileToHold()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        PickTileToHoldRpc();
    }


    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PickTileToHoldRpc()
    {

        FreeMover.Instance.FreeCamera();
        allyStateMenu.SetActive(false);


        FreeMover.Instance.onTileSelect.AddListener(LinkedToTile);

        FreeMover.Instance.onUndoFree.AddListener(delegate { ExitAllyStateMenu(); });
        FreeMover.Instance.onTileSelect.AddListener(delegate { ExitAllyStateMenu(); });
        FreeMover.Instance.onTileSelect.AddListener(delegate { FreeMover.Instance.EndFreeCamera(); });

    }

    public void LinkedToTile(int tileId)
    {
        NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex].targetTile = tileId;
        NetworkData.Instance.GetCurrentPlayer().partyMembers[currentAllyIndex].boardMovementState = PlayerFollowingStates.HoldTile;
    }
}
