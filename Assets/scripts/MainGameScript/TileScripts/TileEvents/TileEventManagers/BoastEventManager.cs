using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BoastEventManager : NetworkBehaviour
{

    
    public GameObject buttonPrefab;
    public GameObject confirmButtons;
    public Transform playerSelectParent;
    private int selectedPlayer;

    public BoastBuff boastStatus;
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
            if(player == NetworkData.Instance.GetCurrentPlayer()) { continue; }

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
        confirmButtons.SetActive(true);
        playerSelectParent.parent.gameObject.SetActive(false);
    }

    public void ConfirmBoast()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        ConfirmBoastRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ConfirmBoastRpc()
    {
        NetworkData.Instance.players[selectedPlayer].GainStatus(boastStatus, NetworkData.Instance.GetCurrentPlayer());

        TileEventManager.Instance.dialogueScript.lines = new List<string>(NetworkData.Instance.currentEvent.endDialouge);
        TileEventManager.Instance.EndEvent();
        TileEventManager.Instance.dialogueScript.endEvent.RemoveListener(CreateButtons);
        confirmButtons.SetActive(false);
        playerSelectParent.parent.gameObject.SetActive(false);
    }
    public void GoBack()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        GoBackRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void GoBackRpc()
    {
        confirmButtons.SetActive(false);
        playerSelectParent.parent.gameObject.SetActive(true);
    }
}
