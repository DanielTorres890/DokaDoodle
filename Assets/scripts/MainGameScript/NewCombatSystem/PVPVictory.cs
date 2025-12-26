using Unity.Netcode;
using UnityEngine;

public class PVPVictory : NetworkBehaviour
{
    public GameObject mainButtons;
    public GameObject stealItem;
    public GameObject confirmMoneySteal;
    public GameObject confirmPrank;
    private StealItemUI stealItemUI;
    public LoseItemManager dropItem;
    public DialogueScript dialogueBox;
    public int winner;
    public int loser;

    public override void OnNetworkSpawn()
    {
       
        mainButtons.SetActive(false);

    }

    public void SetUp(int loserId, int winnerId)
    {
        gameObject.SetActive(true);
        mainButtons.SetActive(true);
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


    public void StealMoneyButton()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
            StealMoneyButtonRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void StealMoneyButtonRpc()
    {
        confirmMoneySteal.SetActive(true);
        mainButtons.SetActive(false);
    }


    public void PrankButton()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
            PrankButtonRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PrankButtonRpc()
    {
        confirmPrank.SetActive(true);
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


    public void BackFromMoney()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
            BackFromMoneyRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void BackFromMoneyRpc()
    {
        confirmMoneySteal.SetActive(false);
        mainButtons.SetActive(true);
    }


    public void BackFromPrank()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
            BackFromPrankRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void BackFromPrankRpc()
    {
        confirmPrank.SetActive(false);
        mainButtons.SetActive(true);
    }



    public void CheckIfFull(bool fullInv, int inventoryNum)
    {
        Debug.Log("I was in fact invoked");
        if (!IsHost) {  return; }
        if (fullInv)
        {
            Debug.Log("I got stuff to drop");
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
        confirmPrank.SetActive(false);
        confirmMoneySteal.SetActive(false);
        dialogueBox.endEvent.RemoveAllListeners();
        dialogueBox.endEvent.AddListener(delegate { SceneChanger.Instance.loadClientScenesServerRpc(dialogueBox.nextScene); });
        dialogueBox.lines.Clear();
        dialogueBox.lines.Add("Well that happpened ");
        dialogueBox.gameObject.SetActive(true);
        dialogueBox.startDialogue();
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void DropTimeRpc()
    {
        stealItem.SetActive(false);
        mainButtons.SetActive(false);
    }

    public void StealMoney()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
            StealMoneyRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void StealMoneyRpc()
    {
        playerData winnerData = NetworkData.Instance.players[winner];
        playerData loserData = NetworkData.Instance.players[loser];
        winnerData.GainMoney(loserData.playerInfo[PlayerInfo.money]);
        loserData.GainMoney(-loserData.playerInfo[PlayerInfo.money]);
        if(IsHost) { FinishVictoryRpc(); }
    }


    public void Prank()
    {
        if (NetworkData.Instance.IsAllowed(winner, NetworkManager.Singleton.LocalClientId))
        {
             PrankRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PrankRpc()
    {
        //does not yet do anything....


        if (IsHost) { FinishVictoryRpc(); }
    }
}
