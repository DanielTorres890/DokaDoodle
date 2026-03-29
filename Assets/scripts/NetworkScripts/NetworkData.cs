using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
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
    public AnimationDatabase victoryAnimDatabase;


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

    public Dictionary<ulong,string> clientIdToGuid = new Dictionary<ulong,string>();
    public Dictionary<string, ulong> GuidToClientId = new Dictionary<string, ulong>();


    public string[] currentGuids = new string[4];


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

    public Guid localUniqueId;
    
    public Dictionary<Attributes, string> attributeStrings = new Dictionary<Attributes, string>
    {
        { Attributes.MaxHealth, "MaxHP" },
        { Attributes.Health, "HP" },
        { Attributes.Attack, "ATK" },
        { Attributes.Defense, "DEF" },
        { Attributes.Magic, "MAG" },
        { Attributes.MDefense, "MDEF" },
        { Attributes.Dexterity, "DEX" }, 
        

    };
    public List<int> seenEnemies = new List<int>();

    public int statsPerLevel = 2;
    public const string BattleScene = "NewBattleArea";
    public void Awake()
    {
        var previousNetwork = Instance;
        if(previousNetwork != null)
        {
            Destroy(previousNetwork);
            Destroy(previousNetwork.gameObject);
            
        }
        
        NetworkData.Instance = this;

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


        if(!PlayerPrefs.HasKey("uniqueId") || true)
        {
            localUniqueId = Guid.NewGuid();
            Debug.Log("My inque id is " + localUniqueId.ToString());
            PlayerPrefs.SetString("uniqueId", localUniqueId.ToString());
            PlayerPrefs.Save();

        }
        else
        {
            localUniqueId = new Guid(PlayerPrefs.GetString("uniqueId"));
        }
        // players.OnListChanged += SyncSticks;

    }
    public void LoadData(GameData data)
    {
        if(Instance == this)
        Debug.Log("Did i get called twice? ");
        players = data.players;
        maxPlayers = data.maxPlayers;
        LoadedIn = true;
        seenEnemies = data.seenEnemies;
        Debug.Log(Instance.players.Count);
        
        InventoriesToDeserialize(data);
    }
    public void SaveData(ref GameData data)
    {
        
        data.players = players;
        data.maxPlayers = maxPlayers;
        data.seenEnemies = seenEnemies;

        InventoriesToSerialize(ref data);


    }

    private void InventoriesToSerialize(ref GameData data)
    {
        //this is a work around bc for some reason data is being called to load twice? (now that i think i think this was an issue previously)
        List<List<List<int>>> serializedInventories = new List<List<List<int>>>(); 
        for (int i = 0; i < playerInventories.Count; i++)
        {
            serializedInventories.Add(new List<List<int>>());
            
            for (int j = 0; j < playerInventories[i].Count; j++)
            {
                serializedInventories[i].Add(playerInventories[i][j].serializeInventory());
            }

        }
        data.inventoryObjects = serializedInventories;
    }
    private void InventoriesToDeserialize(GameData data)
    {

        for (int i = 0; i < data.inventoryObjects.Count; i++)
        {

            for (int j = 0; j < data.inventoryObjects[i].Count; j++)
            {

                for (int k = 0; k < data.inventoryObjects[i][j].Count; k++)
                {
                    Debug.Log("Im being added " + playerInventories[i][j].database.GetItem[data.inventoryObjects[i][j][k]].itemName);
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

        if(IsHost)
        {
            clientIdToGuid.Add(0, localUniqueId.ToString());
            currentGuids[0] = localUniqueId.ToString();
            GuidToClientId.Add(localUniqueId.ToString(),0);
            Debug.Log("The host has mapped their unque id " + currentGuids[0].ToString());
        }

        

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        editor.SetActive(true);
    }

    private void OnClientConnected(ulong clientId)
    {
        
        playerCount++;
        
        if(!IsHost)
        {
            Debug.Log("Map my Id");
            SendGuidRpc(localUniqueId.ToString());
        }
        
        if(NetworkManager.Singleton.ConnectedClientsIds.Count > maxPlayers)
        {
            playerCount--;
            NetworkManager.Singleton.DisconnectClient(clientId);
            return;
        }

        if (!IsServer) { return; }

        for (int i = 0; i < players.Count; i++)
        {
           
            SyncSticksClientRpc(i, players[i].name, players[i].playerClass, players[i].playerFace, players[i].playerHair, maxPlayers, players.Count);
        }
        foreach(var key in GuidToClientId)
        {
            if (ClientNumToPlayerNum(key.Value) == -1) { continue; }
            
            SyncGuidsRpc(key.Key, key.Value, ClientNumToPlayerNum(key.Value));
        }
        if(LoadedIn)
        {
            Debug.Log("WHO GOES THERE ");
            string dataToStore = JsonConvert.SerializeObject(DataPersistenceManager.instance.GetCurrentGameData(), Formatting.Indented);
            SyncOtherDataRpc(dataToStore, clientOrder, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerCount--;
        
        for (int i = currentGuids.Length - 1;  i >= 0; i--)
        {
            if (currentGuids[i] == clientIdToGuid[clientId])
            {
                currentGuids[i] = null;
                break;
            }
        }
        
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            EmptyInventories();

            //Destroy(gameObject);
            for (int i = playerSticks.Count - 1; i >= 0; i--)
            {
                Destroy(playerSticks[i]);
            }
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            SceneManager.LoadScene("MainMenu");
        }

    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SendGuidRpc(string GuidAsString, RpcParams senderInfo = default)
    {

        
        foreach(var key in clientIdToGuid.Keys)
        {
            Debug.Log(key);
        }

        if(clientIdToGuid.ContainsKey(senderInfo.Receive.SenderClientId)) { Debug.Log(senderInfo.Receive.SenderClientId.ToString() + " has Already been mapped "); return; }
        clientIdToGuid.Add(senderInfo.Receive.SenderClientId, GuidAsString);

        if (GuidToClientId.ContainsKey(GuidAsString)) { GuidToClientId[GuidAsString] = senderInfo.Receive.SenderClientId; }
        else { GuidToClientId.Add(GuidAsString, senderInfo.Receive.SenderClientId); }
            
        for(int i = 0;  i < currentGuids.Length; i++)
        {
            //frick u unity serialization
            if(currentGuids[i] == null || currentGuids[i] == "")
            {
                
                currentGuids[i] = GuidAsString;
                SendGuidToClientsRpc(GuidAsString, senderInfo.Receive.SenderClientId, i);
                break;
            }
        }
        
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SendGuidToClientsRpc(string GuidAsString, ulong key, int index)
    {
        currentGuids[index] = GuidAsString;
        if (clientIdToGuid.ContainsKey(key)) {  return; }

        clientIdToGuid.Add(key, GuidAsString);

        if (GuidToClientId.ContainsKey(GuidAsString)) { GuidToClientId[GuidAsString] = key; }
        else { GuidToClientId.Add(GuidAsString, key); }

    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void unreadyServerRpc(RpcParams serverRpcParams)
    {
        readyPlayers[ClientNumToPlayerNum(serverRpcParams.Receive.SenderClientId)] = false;
    }

	[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
	public void sendPlayerDataServerRpc(FixedString32Bytes playerName, int playerClass, int playerFace, int playerHair, RpcParams serverRpcParams)
    {
        int playerId = ClientNumToPlayerNum(serverRpcParams.Receive.SenderClientId);
        
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
        playerInventories[playerId][3].MAXSIZE = players[playerId].maxInventorySizes[3];



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

    [Rpc(SendTo.ClientsAndHost)]
    public void SyncGuidsRpc(string guid, ulong key, int index)
    {
       
        currentGuids[index] = guid;
        if (clientIdToGuid.ContainsKey(key)) { return; }

        
        clientIdToGuid.Add(key, guid);
        if (GuidToClientId.ContainsKey(guid))
        {
            GuidToClientId[guid] = key;
        }
        else
        {
            GuidToClientId.Add(guid, key);
        }
    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddOrderClientRpc(int slotNumber,  ulong clientId)
    {
        clientOrder[slotNumber] = ClientNumToPlayerNum(clientId);
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
    public void ResetGame()
    {
        if (!IsHost)
            NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
        else
            NetworkManager.Singleton.Shutdown();
        
    }
    public bool IsAllowed(int playerNum, ulong playerId)
    {

        if (playerNum != ClientNumToPlayerNum(playerId)) { return false; }

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


        if (inventoryNum == 3)
        {
            if (NetworkData.Instance.players[playerid].equipItems[ItemType.Equipment] == itemNum)
            {
                NetworkData.Instance.players[playerid].UnequipItem(ItemType.Equipment);
            }
        }
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
        EmptyInventories();
    }
    private void EmptyInventories()
    {
        foreach (var inventories in playerInventories)
        {
            foreach (var inventory in inventories)
            {
                inventory.container.Clear();
            }

        }

    }
    public int ClientNumToPlayerNum(ulong playerId)
    {
        string Guid = clientIdToGuid[playerId];
        for(int i = 0; i < currentGuids.Length; i++)
        {
            if(Guid == currentGuids[i])
            {
                return i;
            }
        }
        Debug.Log("this should never happened EVERY player id should be mapped to identifier");
        return -1;
    }
    public ulong PlayerNumToClientId(int playerNum)
    {
        return GuidToClientId[currentGuids[playerNum]];
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddItemToAllyRpc(int playerNum, int allyNum, int itemNum)
    {
        players[playerNum].partyMembers[allyNum].AddAbility(itemNum);
    }
    
}
