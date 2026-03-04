using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkData : NetworkBehaviour, IDataPersistance
{

    public List<playerData> players = new List<playerData>();
    [SerializeField] public List<GameObject> playerSticks = new List<GameObject>();

    [SerializeField] public ClassDataBase classDataBase;
    public BuffDataBase buffDataBase;
    public TrapDataBase trapDataBase;
    public AudioDataBase audioDataBase;
    public TownInfoDataBase TownInfoDataBase;
    //BEFORE U @ ME FOR THIS ITS BC UNITY DOESNT ALLOW U TO SERIALIZE 2D LIST SO THIS IS MY WORK AROUND SO I CAN ADD THEM IN THE INSPECTOR
    public List<List<InventoryObject>> playerInventories = new List<List<InventoryObject>>();
    [SerializeField] private List<InventoryObject> player1Inventories = new List<InventoryObject>();
    [SerializeField] private List<InventoryObject> player2Inventories = new List<InventoryObject>();
    [SerializeField] private List<InventoryObject> player3Inventories = new List<InventoryObject>();
    [SerializeField] private List<InventoryObject> player4Inventories = new List<InventoryObject>();

    public static NetworkData Instance { get; private set; }
    public int playerCount = -1; //you know i have 0 clue why i did this im a dummy dumb
    public int maxPlayers = 4;
    public int currentPlayer = 0;
    public int[] clientOrder = new int[4];
    
    public float globalShopMultiplier = 1;
    private List<bool> readyPlayers = new List<bool>();

    public EventBase currentEvent;
    public UnityEvent onStatusProgress;
    private bool started;
    public bool LoadedIn = false;


    [Header("\n\nPregameStuff")]
    [SerializeField] private GameObject editor;
    [SerializeField] private GameObject previewLoaded;
    [SerializeField] private GameObject[] playerPreviews;
    [SerializeField] private GameObject[] playerPreviewsLoaded;

    public Dictionary<Attributes, string> attributeStrings = new Dictionary<Attributes, string>
    {
        { Attributes.MaxHealth, "MaxHP" },
        { Attributes.Health, "HP" },
        { Attributes.Attack, "ATK" },
        { Attributes.Defense, "DEF" },
        { Attributes.Magic, "MAG" },
        { Attributes.MDefense, "MDEF" },
        { Attributes.Dexterity, "DEX" }, 
        { Attributes.Potency, "POT" }

    };

    public int statsPerLevel = 2;
    public const string BattleScene = "NewBattleArea";
    public void Awake()
    {
        Instance = this;
        readyPlayers.Add(false);
        readyPlayers.Add(false);
        readyPlayers.Add(false);
        readyPlayers.Add(false);
        playerInventories.Add(player1Inventories);
        playerInventories.Add(player2Inventories);
        playerInventories.Add(player3Inventories);
        playerInventories.Add(player4Inventories);
        players.Add(new playerData());
        players.Add(new playerData());
        players.Add(new playerData());
        players.Add(new playerData());
        clientOrder[0] = 0;
        clientOrder[1] = -1;
        clientOrder[2] = -1;
        clientOrder[3] = -1;
        readyPlayers[0] = true;
        // players.OnListChanged += SyncSticks;

    }
    public void LoadData(GameData data)
    {
        players = data.players;
        maxPlayers = data.maxPlayers;
        LoadedIn = true;
        InventoriesToDeserialize(data);
    }
    public void SaveData(ref GameData data)
    {

        data.players = players;
        data.maxPlayers = maxPlayers;
        InventoriesToSerialize(ref data);


    }

    private void InventoriesToSerialize(ref GameData data)
    {
        for (int i = 0; i < playerInventories.Count; i++)
        {
            data.inventoryObjects.Add(new List<List<int>>());
            for (int j = 0; j < playerInventories[i].Count; j++)
            {
                data.inventoryObjects[i].Add(playerInventories[i][j].serializeInventory());
            }

        }
    }
    private void InventoriesToDeserialize(GameData data)
    {
        for (int i = 0; i < data.inventoryObjects.Count; i++)
        {

            for (int j = 0; j < data.inventoryObjects[i].Count; j++)
            {

                for (int k = 0; k < data.inventoryObjects[i][j].Count; k++)
                {
                    playerInventories[i][j].AddItem(playerInventories[i][j].database.GetItem[data.inventoryObjects[i][j][k]]);
                }

            }

        }
    }
    public override void OnNetworkSpawn()
    {
        if (!SettingsManager.instance.settingsOpen && SceneManager.GetSceneByName("Settings").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Settings");
        }



        

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        editor.SetActive(true);
    }

    private void OnClientConnected(ulong clientId)
    {
        
        playerCount++;
        
        if(NetworkManager.Singleton.ConnectedClientsIds.Count > maxPlayers)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
            return;
        }
        if (!IsServer) { return; }

        for (int i = 0; i < players.Count; i++)
        {
           
            SyncSticksClientRpc(i, players[i].name, players[i].playerClass, players[i].playerFace, players[i].playerHair, maxPlayers, players.Count);
        }
        if(LoadedIn)
        {
            string dataToStore = JsonConvert.SerializeObject(DataPersistenceManager.instance.GetCurrentGameData(), Formatting.Indented);
            SyncOtherDataRpc(dataToStore, clientOrder, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerCount--;
    }

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void unreadyServerRpc(RpcParams serverRpcParams)
    {
        readyPlayers[Convert.ToInt32(serverRpcParams.Receive.SenderClientId.ToString())] = false;
    }

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void sendPlayerDataServerRpc(FixedString32Bytes playerName, int playerClass, int playerFace, int playerHair, RpcParams serverRpcParams)
    {
        int playerId = Convert.ToInt32(serverRpcParams.Receive.SenderClientId.ToString());
        
        players[playerId] =
            new playerData(playerClass, playerName, playerFace, playerHair);
        readyPlayers[playerId] = true;
        SyncSticksClientRpc(playerId, players[playerId].name, players[playerId].playerClass, players[playerId].playerFace, players[playerId].playerHair, maxPlayers, players.Count);


    }

    //This function actually removes players if i wanted to readd players/add ai i gotta do somethin diffy but until then
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void fillPlayerServerRpc(int index)
    {
        players.RemoveAt(players.Count - 1);
        readyPlayers.RemoveAt(readyPlayers.Count - 1);
        maxPlayers--;   


    }
	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void removePlayerServerRpc(int index)
    {
        readyPlayers[index] = false;    
        playerCount--;
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void SyncSticksClientRpc(int playerId, FixedString32Bytes playerName, int playerClass, int playerFace, int playerHair, int playerCountin, int openSlots)
    {

        if(playerName != "")
        {
            playerPreviews[playerId].SetActive(true);
            playerPreviewsLoaded[playerId].SetActive(true);
        }


        players[playerId].name = playerName.ToString();
        players[playerId].playerFace = playerFace;
        players[playerId].playerClass = playerClass;
        players[playerId].playerHair = playerHair;


        maxPlayers = playerCountin;
        players[playerId].maxInventorySizes = NetworkData.Instance.classDataBase.GetItem[playerClass].inventorySizes;


        
        playerInventories[playerId][0].MAXSIZE = players[playerId].maxInventorySizes[0];
        playerInventories[playerId][1].MAXSIZE = players[playerId].maxInventorySizes[1];
        playerInventories[playerId][2].MAXSIZE = players[playerId].maxInventorySizes[2];
        

        characterEditor curStickEdit = playerSticks[playerId].GetComponent<characterEditor>();
        curStickEdit.setClass(players[playerId].playerClass);
        playerSticks[playerId].GetComponent<characterEditor>().setFace(players[playerId].playerFace);
        playerSticks[playerId].GetComponent<characterEditor>().setHair(players[playerId].playerHair);
        
        while(players.Count > openSlots)
        {
            players.RemoveAt(playerCountin-1);
            readyPlayers.RemoveAt(playerCountin-1);
        }
        
    }


    [Rpc(SendTo.SpecifiedInParams)]
    public void SyncOtherDataRpc(string jsonString,int[] idOrder, RpcParams rpcStuff)
    {
        //sooo client connected includes host
        

        clientOrder = idOrder;
		LoadedIn = true;
		editor.SetActive(false);
		previewLoaded.SetActive(true);

		if (IsHost) { return; }
		DataPersistenceManager.instance.LoadDataFromString(jsonString);
        
        for(int i = 0; i < players.Count; i++)
        {
            playerPreviews[i].SetActive(true);
            playerPreviewsLoaded[i].SetActive(true);
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddOrderClientRpc(int slotNumber, int clientId)
    {
        clientOrder[slotNumber] = clientId;
        readyPlayers[slotNumber] = true;
    }

    public void startGame()
    {

        
        for (int i = 0; i < players.Count; i++)
        {
            
            bool ready = readyPlayers[i];
            if (!ready) { return; }

        }
       
        if(!started)
        {
            if(!LoadedIn)
            PlayerClassStatsRpc();
            else
            {
                for(int i = 0;i < players.Count;i++)
                {
                    players[i].playerNumber = clientOrder[i];
                }
            }
                started = true;
            SceneChanger.Instance.loadClientScenesServerRpc("PregameCutScene");
        }
        
    }
    public bool IsAllowed(int playerNum, ulong playerId)
    {

        if (playerNum != Convert.ToInt32(playerId)) { return false; }

        return true;
    }
    public bool IsAllowed()
    {
        return IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.LocalClientId);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayerClassStatsRpc()
    {
        for (int i = 0; i < players.Count; i++)
        {
            foreach (var stat in classDataBase.GetItem[players[i].playerClass].stats)
            {
                players[i].stats[stat.attribute] += stat.value;

            }
            players[i].playerNumber = i;
            players[i].loyaltyTags.Add("Player" + players[i].playerNumber);
            players[i].PostStatusStatCalc();
            if(IsServer) { playerSticks[i].GetComponent<NetworkObject>().ChangeOwnership((ulong)i);  }
        }

    }
    public bool AddItemToInventory(int playerId, ItemBase item)
    {
        
        int type = item.determineType();
        return playerInventories[playerId][type].AddItem(item);
    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void LoseItemRpc(int playerid, int itemNum, int inventoryNum)
    {
  

       
        NetworkData.Instance.playerInventories[playerid][inventoryNum].container.RemoveAt(itemNum);
       
    }
    public void setNextTurnNum()
    {
        
        if (NetworkData.Instance.currentPlayer < NetworkData.Instance.maxPlayers - 1) { NetworkData.Instance.currentPlayer += 1; }

        else { NetworkData.Instance.currentPlayer = 0; }
        
    }
    public void setBackTurnNum()
    {
        if(NetworkData.Instance.currentPlayer > 0) { NetworkData.Instance.currentPlayer -=  1; }
        else { NetworkData.Instance.currentPlayer = NetworkData.Instance.maxPlayers - 1; }
    }
    public void ProgressStatus(int player)
    {
        for (int i = 0; i < NetworkData.Instance.players[player].statuses.Count; i++)
        {
            if (NetworkData.Instance.players[player].statuses[i].ProgressStatus())
            {
                NetworkData.Instance.players[player].statuses.RemoveAt(i);
                
                i--;
            }
        }
        
        NetworkData.Instance.players[player].PostStatusStatCalc();
    }

    public playerData GetCurrentPlayer()
    {
        return Instance.players[Instance.currentPlayer];
    }
    public int checkUnlockedClass(int player)
    {
        foreach(var classId in classDataBase.GetId.Values)
        {
            if (players[player].playerClassProgress.ContainsKey(classId)) { continue; }

            if (!classDataBase.GetItem[classId].UnlockCondition(NetworkData.Instance.players[player])) { continue; }

            Instance.players[player].playerClassProgress.Add(classId, new PlayerClassProgress());
            return classId;
        }
        return -1;
    }
    public void OnApplicationQuit()
    {
        foreach(var inventories in playerInventories)
        {
            foreach(var inventory in inventories)
            {
                inventory.container.Clear();
            }
            
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddItemToAllyRpc(int playerNum, int allyNum, int itemNum)
    {
        players[playerNum].partyMembers[allyNum].AddAbility(itemNum);
    }
}
