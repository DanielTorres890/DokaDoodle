using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StealEventManager : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject buttonPrefab;
    public GameObject stealOptionButtons;
    public GameObject confirmButton;
    public Transform playerSelectParent;

    public int[] stealOdds;
    private int selectedPlayer;
    private int selectedOption;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        TileEventManager.Instance.dialogueScript.endEvent.AddListener(CreateButtons);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void CreateButtons()
    {
        playerSelectParent.parent.gameObject.SetActive(true);
        foreach (var player in NetworkData.Instance.players)
        {
            if (player == NetworkData.Instance.GetCurrentPlayer()) { continue; }

            GameObject spawnedButton = Instantiate(buttonPrefab, playerSelectParent);
            spawnedButton.GetComponent<Button>().onClick.AddListener(delegate { PickPlayerNumber(player.playerNumber); });
            spawnedButton.GetComponentInChildren<TextMeshProUGUI>().text = player.name.ToString();
        }
    }
    public void PickPlayerNumber(int playerNum)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        PickPlayerNumberRpc(playerNum);
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void PickPlayerNumberRpc(int playerNum)
    {
        selectedPlayer = playerNum;
        stealOptionButtons.SetActive(true);
        playerSelectParent.parent.gameObject.SetActive(false);
    }


    public void PickStealOption(int option)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        PickStealOptionRpc(option);
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PickStealOptionRpc(int option)
    {
        selectedOption = option;
        stealOptionButtons.SetActive(false);
        confirmButton.SetActive(true);
    }

    public void FinishSteal()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        FinishStealRpc(Random.Range(0,101));

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void FinishStealRpc(int rando)
    {
        if(rando > stealOdds[selectedOption])
        {
            TileEventManager.Instance.dialogueScript.lines = new List<string> { "I was able to snag "};
            if(selectedOption == 0)
            {
                TileEventManager.Instance.dialogueScript.lines[0] += StealGold(rando);
            }
            if(selectedOption == 1)
            {
                TileEventManager.Instance.dialogueScript.lines[0] += StealItem(rando);
            }
            if(selectedOption == 2)
            {
                TileEventManager.Instance.dialogueScript.lines[0] += StealSkill(rando);
            }
            if(selectedOption == 3)
            {
                TileEventManager.Instance.dialogueScript.lines[0] += StealEquipment(rando);
            }
        }
        else
        {
            TileEventManager.Instance.dialogueScript.lines = new List<string>
            {
                "I wasn't able to steal anything...",
                "*sigh* I'm a failiure"
            };

        }
        TileEventManager.Instance.EndEvent();
        TileEventManager.Instance.dialogueScript.endEvent.RemoveListener(CreateButtons);
        confirmButton.SetActive(false);

    }
    private string StealGold(int randomNum)
    {
        string returnString = "";
        if (NetworkData.Instance.players[selectedPlayer].playerInfo[PlayerInfo.money] <= 0)
        {
            returnString = " no gold, they had no money :(";
        }

        returnString = "<color=blue>" + NetworkData.Instance.players[selectedPlayer].playerInfo[PlayerInfo.money] / 2 + "</color> gold";
        NetworkData.Instance.players[selectedPlayer].GainMoney(-NetworkData.Instance.players[selectedPlayer].playerInfo[PlayerInfo.money] / 2);
        return returnString;
    }
    private string StealItem(int randomNum)
    {
        string returnMsg = "";
        if (NetworkData.Instance.playerInventories[selectedPlayer][0].container.Count <= 0)
        {
            returnMsg = "nothing they had no items :(";
        }
        else
        {

            returnMsg = "<color=blue>" + StealItem(0, randomNum).itemName + "</color>";
        }
        return returnMsg;
    }
    private string StealSkill (int randomNum)
    {
        string returnMsg = "";
        if (NetworkData.Instance.playerInventories[selectedPlayer][1].container.Count <= 0 && NetworkData.Instance.playerInventories[selectedPlayer][2].container.Count <= 0)
        {
            returnMsg = "nothing they had no skills :(";
        }
        else
        {
            
            if(NetworkData.Instance.playerInventories[selectedPlayer][1].container.Count <= 0)
            {
                returnMsg = "<color=blue>" + StealItem(2, randomNum).itemName + "</color>";

            }
            else if (NetworkData.Instance.playerInventories[selectedPlayer][2].container.Count <= 0)
            {
                returnMsg = "<color=blue>" + StealItem(1, randomNum).itemName + "</color>";
            }
            else
            {
                int invNum = (randomNum % 2) + 1;
                returnMsg = "<color=blue>" + StealItem(invNum, randomNum).itemName + "</color>";
            }

        }
        return returnMsg;
    }

    private string StealEquipment(int randomNum)
    {
        string returnMsg = "";
        if (NetworkData.Instance.playerInventories[selectedPlayer][3].container.Count <= 0)
        {
            returnMsg = "nothing they had no equipment :(";
        }
        else
        {

            returnMsg = "<color=blue>" + StealItem(3, randomNum).itemName + "</color>";
        }
        return returnMsg;
    }

    private ItemBase StealItem(int inventoryNumber, int randomNum)
    {
        ItemBase stolenItem = NetworkData.Instance.playerInventories[selectedPlayer][inventoryNumber].container[randomNum % NetworkData.Instance.playerInventories[selectedPlayer][inventoryNumber].container.Count].item;
        NetworkData.Instance.playerInventories[selectedPlayer][inventoryNumber].RemoveItem(stolenItem);
        NetworkData.Instance.playerInventories[NetworkData.Instance.GetCurrentPlayer().playerNumber][inventoryNumber].AddItem(stolenItem);
        return stolenItem;
    }
    //public void ConfirmBoast()
    //{
    //    if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
    //    ConfirmBoastRpc();
    //}

    //[Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    //private void ConfirmBoastRpc()
    //{
    //    NetworkData.Instance.players[selectedPlayer].GainStatus(boastStatus, NetworkData.Instance.GetCurrentPlayer());

    //    TileEventManager.Instance.dialogueScript.lines = new List<string>(NetworkData.Instance.currentEvent.endDialouge);
    //    TileEventManager.Instance.EndEvent();
    //    TileEventManager.Instance.dialogueScript.endEvent.RemoveListener(CreateButtons);
    //    confirmButtons.SetActive(false);
    //    playerSelectParent.parent.gameObject.SetActive(false);
    //}



    public void GoBackToPlayers()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        GoBackPlayersRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void GoBackPlayersRpc()
    {
        stealOptionButtons.SetActive(false);
        playerSelectParent.parent.gameObject.SetActive(true);
    }
    public void GoBackToSteal()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        GoBackToStealRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void GoBackToStealRpc()
    {
        stealOptionButtons.SetActive(true);
        confirmButton.SetActive(false);
    }
}
