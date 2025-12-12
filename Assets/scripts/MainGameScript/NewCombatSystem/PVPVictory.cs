using Unity.Netcode;
using UnityEngine;

public class PVPVictory : NetworkBehaviour
{
    public GameObject mainButtons;
    public GameObject stealItem;
    public GameObject stealMoney;
    public GameObject prank;
    public int winner;

    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(false);
    }

    public void SetUp()
    {

    }

    public void StealItemButton()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
            StealItemButtonRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void StealItemButtonRpc()
    {
        stealItem.SetActive(true);
        mainButtons.SetActive(false);
    }
    public void BackFromSteal()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
            BackFromStealRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void BackFromStealRpc()
    {
        stealItem.SetActive(false);
        mainButtons.SetActive(true);
    }


}
