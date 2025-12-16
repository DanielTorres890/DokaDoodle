using Unity.Netcode;
using UnityEngine;

public class PVPVictory : NetworkBehaviour
{
    public GameObject mainButtons;
    public GameObject stealItem;
    public GameObject stealMoney;
    public GameObject prank;
    
    private StealItemUI stealItemUI;
    public LoseItemManager dropItem;

    public int winner;
    public int loser;

    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(false);
    }

    public void SetUp(int loserId, int winnerId)
    {
        gameObject.SetActive(true);
        stealItemUI = stealItem.GetComponent<StealItemUI>();
        stealItemUI.goBackButton.onClick.AddListener(BackFromSteal);
        stealItemUI.finishSteal.AddListener(CheckIfFull);
        
        winner = winnerId;
        loser = loserId;
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
        stealItemUI.SetUp(winner, loser);
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

    public void CheckIfFull(bool fullInv, int inventoryNum)
    {
        if (!NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!IsHost) {  return; }
        if (fullInv)
        {
            dropItem.SetUp(winner, inventoryNum);
        }
        else
        {
            FinishVictory();
        }
    
    }
    
    public void FinishVictory()
    {

    }

}
