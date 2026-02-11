using Unity.Netcode;
using UnityEngine;

public class AllyViewNetwork : NetworkBehaviour
{
    public AllyMainViewer display;



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
}
