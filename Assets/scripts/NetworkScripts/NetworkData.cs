using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkData : NetworkBehaviour
{

    public List<playerData> players = new List<playerData>();
    [SerializeField] public List<GameObject> playerSticks = new List<GameObject>();
    [SerializeField] private GameObject characterEditor;

    [SerializeField] private List<InventoryObject> player1Inventories = new List<InventoryObject>();
    [SerializeField] private List<InventoryObject> player2Inventories = new List<InventoryObject>();
    [SerializeField] private List<InventoryObject> player3Inventories = new List<InventoryObject>();
    [SerializeField] private List<InventoryObject> player4Inventories = new List<InventoryObject>();

    public static NetworkData Instance { get; private set;}
    public int playerCount = -1;
    public int currentPlayer = 0;
    private List<bool> readyPlayers = new List<bool>();


  

    public void Awake()
    {
        Instance = this;
        readyPlayers.Add(false);
        readyPlayers.Add(false);
        readyPlayers.Add(false);
        readyPlayers.Add(false);
       // players.OnListChanged += SyncSticks;
        
    }
 
    public override void OnNetworkSpawn()
    {   
        
        
            players.Add(new playerData());
            players.Add(new playerData());
            players.Add(new playerData());
            players.Add(new playerData());
            players[0].Inventories = player1Inventories;
            
            players[1].Inventories = player2Inventories;
            players[2].Inventories = player3Inventories;
            players[3].Inventories = player4Inventories;

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log(playerCount);
        playerCount++;
        if (clientId != NetworkManager.Singleton.LocalClientId) { return;  } 
        
       
        characterEditor.SetActive(true);
        for (int i = 0; i < players.Count; i++)
        {
            SyncSticksClientRpc(i, players[i].playerName, players[i].playerClass, players[i].playerFace, players[i].playerHair);
        }

    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerCount--;
    }

    [ServerRpc (RequireOwnership = false)]
    public void unreadyServerRpc(ServerRpcParams serverRpcParams)
    {
        readyPlayers[Convert.ToInt32(serverRpcParams.Receive.SenderClientId.ToString())] = false;
    }

    [ServerRpc (RequireOwnership = false)]
    public void sendPlayerDataServerRpc(FixedString32Bytes playerName, int playerClass, int playerFace,int playerHair, ServerRpcParams serverRpcParams)
    {
        int playerId = Convert.ToInt32(serverRpcParams.Receive.SenderClientId.ToString());
        players[playerId] = 
            new playerData(playerClass,playerName,playerFace,playerHair);
        readyPlayers[playerId] = true;
        SyncSticksClientRpc(playerId, players[playerId].playerName, players[playerId].playerClass, players[playerId].playerFace, players[playerId].playerHair);

        
    }
    [ServerRpc(RequireOwnership = false)]
    public void fillPlayerServerRpc(int index)
    {
        readyPlayers[index] = true;
        playerCount++;
        Debug.Log(playerCount);
    }
    [ServerRpc(RequireOwnership = false)]
    public void removePlayerServerRpc(int index)
    {
        readyPlayers[index] = false;
        playerCount--;
    }
    [ClientRpc (RequireOwnership = false)]
    public void SyncSticksClientRpc(int playerId, FixedString32Bytes playerName, int playerClass, int playerFace, int playerHair)
    {
        players[playerId].playerName = playerName;
        players[playerId].playerFace = playerFace;
        players[playerId].playerClass = playerClass;
        players[playerId].playerHair = playerHair;
        players[0].Inventories = player1Inventories;
        players[1].Inventories = player2Inventories;
        players[2].Inventories = player3Inventories;
        players[3].Inventories = player4Inventories;

        characterEditor curStickEdit = playerSticks[playerId].GetComponent<characterEditor>();
        curStickEdit.setClass(players[playerId].playerClass);
        playerSticks[playerId].GetComponent<characterEditor>().setFace(players[playerId].playerFace);
        playerSticks[playerId].GetComponent<characterEditor>().setHair(players[playerId].playerHair);
        
    }

    public void startGame()
    {
        foreach(var ready in readyPlayers)
        {
            if (!ready) { return; }

        }
        SceneChanger.Instance.loadClientScenesServerRpc("PregameCutScene");
    }
}
