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
    public DialogueScript dialogueBox;
    public int winner;
    public int loser;

    public override void OnNetworkSpawn()
    {
       
        mainButtons.SetActive(false);
        stealItem.SetActive(false);

    }

    public void SetUp(int loserId, int winnerId)
    {
        Debug.Log("I set up when i shouldn't have fmcl");
        gameObject.SetActive(true);
        stealItemUI = stealItem.GetComponent<StealItemUI>();
        stealItemUI.goBackButton.onClick.AddListener(BackFromSteal);
        stealItemUI.finishSteal.AddListener(CheckIfFull);
        stealItemUI.gameObject.SetActive(false);
        dropItem.gameObject.SetActive(false);
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
            dropItem.finishLose.AddListener(FinishVictoryRpc);
            DropTimeRpc();
        }
        else
        {
            FinishVictoryRpc();
        }
    
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void FinishVictoryRpc()
    {
        dialogueBox.endEvent.RemoveAllListeners();
        dialogueBox.endEvent.AddListener(delegate { SceneChanger.Instance.loadClientScenesServerRpc(dialogueBox.nextScene); });
        dialogueBox.lines.Clear();
        dialogueBox.lines.Add("Well that happpened ");
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void DropTimeRpc()
    {
        stealItem.SetActive(false);
        mainButtons.SetActive(false);
    }
}
