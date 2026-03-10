using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Netcode.NetworkSceneManager;

public class ClientChecks : NetworkBehaviour
{
    [SerializeField] private DisplayInventory display;
    [SerializeField] private InventoryChangeScript changeScript;
    [SerializeField] private GameObject confirmButtons;

    public static ClientChecks Instance { get; private set; }

    public List<string> mapNames = new List<string>();
    public DialogueScript displayText;
    public GameObject mainMenuButtons;
    public GameObject cameraControlDisplay;



    private TextMeshProUGUI displayTxt;
    public TextMeshProUGUI rollNum;
    public UnityEvent onRoundStart;
    public GameObject combatPreview;
    public UnityEvent onItemUse;//i'd like to say that im not that happy about whats going on here but this has to be better than updating stat UI every frame
    public UnityEvent onClassAbilityUse;

    public Image worldEventImage;
    private bool loadedIn = false;

    public RandomItemSelect randomItemPickup;


    //i dont like this but i also cant imagine making it more robust would be a better use of time
    private int currentPlayerLook;
    private int currentItemId;
    private int currentInvNumber;
    //im gonna be so fr this whole thingy i have going on with this class is some big buns and im sorry to anyone who looks at this
    //(the main issue is im doing wayyy to much in here in the worst ways possible
   
    
    public override void OnNetworkSpawn()
    {
        
        
        
      
        

        //StartCoroutine(WaitUntilAllLoaded());
        /* if(SceneChanger.Instance.everyoneLoaded())
         {
             PreturnStuff();

             Debug.Log("fmcl bruh WHY DOES IT DO IT MULTIPLE TIMES FOR EACH CLIENT THAT LOADS IN ON THE SERVER ");
         }*/



    }
    public void Start()
    {
       
        if (IsHost)
        {

            StartCoroutine(WaitUntilAllLoaded());
        }
    }
    private void SceneStart()
    {
        
        loadedIn = false;
        //THIS SHOULD CHANGE THIS IS ONLY FOR NOW
        

        //kind of a mickey mouse manuever but its okay i hope
        if(!loadedIn)
        {   
            
            if(IsHost)
            {
                NetworkManager.SceneManager.OnLoadEventCompleted += McChickenWrapper;
                SceneChanger.Instance.loadClientScenesAddidtiveRpc(mapNames[NetworkData.Instance.GetCurrentPlayer().curMap]);
            }
        }
        
        loadedIn = true;
    }
    //silly name bc it only exists for this
    private void McChickenWrapper(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= McChickenWrapper;

        
        StartCoroutine(WaitUntilAllLoaded2());
        
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void EveryoneLockInRpc()
    {
        StartCoroutine(WaitUntilLocalLoaded());
    }
    public void PreturnStuff()
    {
        //im really not sure if this is the best way, its basically saying maps dont exist until someone sees them but surely that cant be good

        NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd] -= 1;
        NetworkData.Instance.ProgressStatus(NetworkData.Instance.currentPlayer);

        onRoundStart.Invoke();
        WorldEventManager.Instance.ProgressDay();
        

    }
    private void Awake()
    {
        

        onRoundStart = new UnityEvent();
        if (Instance == null)
        {
            Instance = this;
        }

    }

    
    public void TurnStartChecks()
    {

        bool rumble = false;
        //at some point im probably gonna have to make this a different event but frick u
        onRoundStart.Invoke();

        var curTile = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];
        foreach (var players in curTile.players)
        {

            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && !NetworkData.Instance.players[players].isDead && PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {

                rumble = true;
            }
        }


        List<EntityStats> potentialCombatants = new List<EntityStats>(curTile.tileEnemy);
        foreach(var ally in curTile.partyMembers)
        {
            if(ally.allyOwner == NetworkData.Instance.GetCurrentPlayer().playerNumber) { continue; }
            potentialCombatants.Add(ally);
        }
        


        if ((potentialCombatants.Count == 0 && !rumble) && !NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead)
        {

            curTile.players.Remove(NetworkData.Instance.currentPlayer);
            PlayerMoveManager.Instance.playerCam.Follow = PlayerMoveManager.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform;
            
            
            PopUpManager.Instance.PerformPopUp(0);

            var unlockedClassId = NetworkData.Instance.checkUnlockedClass(NetworkData.Instance.currentPlayer);
            if(unlockedClassId == -1)
            {
                mainMenuButtons.SetActive(true);
                rollNum.gameObject.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                if(IsHost) { DisplayGainedClassRpc(unlockedClassId); }
            }

            
        }

        else
        {

            if (NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead)
            {
                mainMenuButtons.SetActive(false);

                if (IsServer) { ClientChecks.Instance.DisplayDeadRpc(); }
                return;
            }


            mainMenuButtons.SetActive(false);

            if (IsServer) { SyncEnemyRpc(0); }

        }
    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void ShowConfirmItemButtonsRpc(int player, int itemId, int inventoryNum)
    {
        display.gameObject.SetActive(false);
        confirmButtons.SetActive(true);

        if (!NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].battleItem)
        {
            confirmButtons.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            confirmButtons.transform.GetChild(1).gameObject.SetActive(true);
        }
        
        
        currentInvNumber = inventoryNum;
        currentItemId = itemId;
        currentPlayerLook = player;

    }
    public void ReturnToInv()
    {
        if(!NetworkData.Instance.IsAllowed()) { return; }
        ReturnToInvRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReturnToInvRpc()
    {
        display.gameObject.SetActive(true);
        confirmButtons.SetActive(false);
    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void ConfirmBuffRpc()
    {
        confirmButtons.SetActive(false);
        int player = currentPlayerLook;
        int itemId = currentItemId;
        int inventoryNum = currentInvNumber;

        NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][inventoryNum]);
        displayText.lines.Clear();

        changeScript.ResetDisplay();

        display.gameObject.SetActive(false);
        

        displayText.lines.Add(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].useText);
        StartCoroutine(usedItem());

        onItemUse.Invoke();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void ConfirmBuffRpc(int player, int itemId, int inventoryNum)
    {

        NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][inventoryNum]);
        displayText.lines.Clear();

        changeScript.ResetDisplay();

        display.gameObject.SetActive(false);
        display.transform.parent.gameObject.SetActive(false);

        displayText.lines.Add(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].useText);
        StartCoroutine(usedItem());

        onItemUse.Invoke();
    }


    public void SlotBattleItem()
    {
        if (!NetworkData.Instance.IsAllowed()) { return; }
        SlotBattleItemRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SlotBattleItemRpc()
    {
        NetworkData.Instance.players[currentPlayerLook].battleSlotItemId = currentItemId;
        confirmButtons.SetActive(false);
        changeScript.ResetDisplay();
        display.gameObject.SetActive(true);
        display.transform.parent.gameObject.SetActive(true);

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]

    public void RandomizedItemSelectRpc(int player, int itemId, int inventoryType)
    {
        var playerinfo = NetworkData.Instance.players[player];
        rollNum.gameObject.transform.parent.gameObject.SetActive(false);
        randomItemPickup.ShuffleDisplay((PlayerMoveManager.Instance.mapTiles[playerinfo.curTileId] as ItemTile).items, itemId, inventoryType);
    }

    public void ConfirmItemPickup(int player, int itemId, int inventoryNum)
    {
        displayText.lines.Clear();

        //?displayText = ItemPickupDisplay.Instance.gameObject;
        bool overflowed = NetworkData.Instance.playerInventories[player][inventoryNum].AddItem(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId]);


        displayText.lines.Add("Obtained a <color=blue>" + NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].name + "</color>");
        StartCoroutine(displayItem(overflowed, player, itemId, inventoryNum));
        
        
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void SyncEnemyRpc(int encounterId)
    {

        string encounterName = PlayerCombatManager.Instance.BattleSetUp(encounterId);
        combatPreview.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name.ToString();
       
        
        combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = encounterName;
              
      
        
        StartCoroutine(previewFight());
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void SyncEventRpc(int eventNum)
    {
        
        NetworkData.Instance.currentEvent = (PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId] as DefaultTile).events[eventNum];
        SceneChanger.Instance.loadClientScenesServerRpc(NetworkData.Instance.currentEvent.SceneToGoTo);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void WorldEventRpc()
    {
        Debug.Log("Step 3");
        if (WorldEventManager.Instance.eventsToActivate.Count > 0) { StartCoroutine(displayActivateEvent()); }
        else if (WorldEventManager.Instance.eventsToDeactivate.Count > 0) { StartCoroutine(displayDeactivateEvent()); }

    }
    /* [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
     public void InitiateFightRpc()
     {
         combatPreview.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name.ToString();
         if (PlayerCombatManager.Instance.combatant2 is playerData)
         {
             combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.players[(PlayerCombatManager.Instance.combatant2 as playerData).playerNumber].name.ToString();
         }
         else
         {
             Debug.Log(PlayerCombatManager.Instance.combatant2.name.ToString());
             combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant2.name.ToString();
         }
         Debug.Log(combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text);

         StartCoroutine(previewFight());
     } */

    [Rpc(SendTo.ClientsAndHost,InvokePermission = RpcInvokePermission.Everyone)]
    public void DisplayDeadRpc()
    {
        displayText.lines.Clear();
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].progressDeath();
        displayText.lines.Add(NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name + " is dead for <color=red>" + (NetworkData.Instance.players[NetworkData.Instance.currentPlayer].tillRevive + 1) + "</color> turns");
        
        StartCoroutine(displayItem(false,0,0,0));//man im lazy
        
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void DisplayGainedClassRpc(int classId)
    {
        displayText.lines.Clear();
        displayText.lines.Add(NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name + " unlocked the <color=purple>" + (NetworkData.Instance.classDataBase.GetItem[classId].className) + "</color> class\nGo to the employment office to change your class!");
        StartCoroutine(displayClassGained());

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeployTrapRpc(int tileId, int trapId)
    {
        MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][tileId].trapIds.Add(trapId);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void UseClassAbilityRpc(int randomNum = 0)
    {

        NetworkData.Instance.classDataBase.GetItem[NetworkData.Instance.GetCurrentPlayer().playerClass].ClassAction(NetworkData.Instance.GetCurrentPlayer(), randomNum);

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void CompleteClassAbilityRpc()
    {
        displayText.lines.Clear();
        displayText.lines.Add(NetworkData.Instance.classDataBase.GetItem[NetworkData.Instance.GetCurrentPlayer().playerClass].actionUseText);

        StartCoroutine(usedAbility());
        onClassAbilityUse.Invoke();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void ActivateTrapsRpc()
    {
        StartCoroutine(TrapActivates());
    }
    private IEnumerator previewFight()
    {

        combatPreview.SetActive(true);
        yield return new WaitForSecondsRealtime(5f);
        if(IsHost)
        SceneChanger.Instance.loadClientScenesServerRpc(PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.GetCurrentPlayer().curTileId].battleEnvironment);

    }
    private IEnumerator TrapActivates()
    {
        displayText.lines.Clear();
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        var trapcache = NetworkData.Instance.trapDataBase.GetTrap[MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds[0]];
        displayText.lines.Add(trapcache.TrapString(NetworkData.Instance.GetCurrentPlayer()));
        trapcache.TrapEffect(NetworkData.Instance.players[NetworkData.Instance.currentPlayer]);
        onItemUse.Invoke();
        MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds.RemoveAt(0);
        displayText.gameObject.SetActive(true);
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {

            yield return null;
        }
        if (MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds.Count > 0) 
        {
            StartCoroutine(TrapActivates());
        }
        else if (IsServer && NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead) {  PlayerMoveManager.Instance.NextTurnRpc(); }

        else if (IsServer) { PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].TileEvent(); }
            

    }
    private IEnumerator displayItem(bool overflow, int player, int itemId, int inventoryNum)
    {
        
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {

            yield return null;
        }
        if (IsServer)
        {
            
            if(overflow)
            {

                LoseItemManager.instance.SetUp(player,inventoryNum);
                LoseItemManager.instance.finishLose.AddListener(PlayerMoveManager.Instance.NextTurnRpc);
                Debug.Log("Im only subbed once right?");
            }
            else
            {
                PlayerMoveManager.Instance.NextTurnRpc();
                Debug.Log("Am i skipping to here? ");
            }
                
        }
        

    }
    private IEnumerator usedItem()
    {
        display.transform.parent.gameObject.SetActive(false);
        display.gameObject.SetActive(false);
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {
            
            yield return null;
        }

        Debug.Log("am i showing early?");
        display.transform.parent.gameObject.SetActive(true);
        display.gameObject.SetActive(true);
    }
    private IEnumerator displayClassGained()
    {
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {

            yield return null;
        }
        mainMenuButtons.SetActive(true);
    }
    private IEnumerator usedAbility()
    {
        mainMenuButtons.SetActive(false);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        displayText.gameObject.SetActive(true);
        displayText.Awake();

        mainMenuButtons.transform.GetChild(3).gameObject.GetComponent<ClassAbility>().setButtonText(); //nasty work i shouuld redo this frfr
        while (displayText.gameObject.activeSelf)
        {


            yield return null;
        }

        mainMenuButtons.SetActive(true);
    }

    private IEnumerator displayActivateEvent()
    {
   
        displayText.lines.Clear();
        displayText.lines.Add(WorldEventManager.Instance.eventsToActivate[0].ActivateText);
        worldEventImage.sprite = WorldEventManager.Instance.eventsToActivate[0].eventDisplay;
        worldEventImage.gameObject.SetActive(true);
        
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        WorldEventManager.Instance.activeWorldEvents.Add(new WorldEventWrapper(WorldEventManager.Instance.worldDatabase.GetId[WorldEventManager.Instance.eventsToActivate[0]]));
        displayText.Awake();

        
        while (displayText.gameObject.activeSelf)
        {
            yield return null;
        }
        worldEventImage.gameObject.SetActive(false);

        WorldEventManager.Instance.eventsToActivate[0].OnActivate();
        WorldEventManager.Instance.eventsToActivate.RemoveAt(0);
        if (WorldEventManager.Instance.currentCutscene != null) { }

        else if (WorldEventManager.Instance.eventsToActivate.Count > 0) { StartCoroutine(displayActivateEvent()); }

        else if (WorldEventManager.Instance.eventsToDeactivate.Count > 0 ) {  StartCoroutine(displayDeactivateEvent()); }

        else { TurnStartChecks(); }
    }

    private IEnumerator displayDeactivateEvent()
    {
        displayText.lines.Clear();
        displayText.lines.Add(WorldEventManager.Instance.eventsToDeactivate[0].DeactivateText);
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        
        displayText.Awake();
        for (int i = WorldEventManager.Instance.activeWorldEvents.Count - 1; i >= 0; i--)
        {
            if (WorldEventManager.Instance.activeWorldEvents[i].eventId == WorldEventManager.Instance.worldDatabase.GetId[WorldEventManager.Instance.eventsToDeactivate[0]])
            {
                WorldEventManager.Instance.activeWorldEvents.RemoveAt(i);
            }
        }


        

        while (displayText.gameObject.activeSelf)
        {
            yield return null;
        }
        //we pray for no desync 
        WorldEventManager.Instance.eventsToDeactivate[0].OnDeactivate();
        WorldEventManager.Instance.eventsToDeactivate.RemoveAt(0);
        if(WorldEventManager.Instance.currentCutscene != null) {  }
        else if (WorldEventManager.Instance.eventsToDeactivate.Count > 0) { StartCoroutine(displayDeactivateEvent()); }
        else { TurnStartChecks(); }
    }

    private IEnumerator WaitUntilAllLoaded()
    {
        while(!SceneChanger.Instance.everyoneLoaded())
        {
            yield return null;
        }
        SceneStart();


       
        
    }
    private IEnumerator WaitUntilAllLoaded2()
    {
        //the second half is sorta(?) redundant but it happened once where everyone loaded before the tiles were initialized the first time
        while (!SceneChanger.Instance.everyoneLoaded())
        {
            yield return null;
        }

        EveryoneLockInRpc();
    }
    private IEnumerator WaitUntilLocalLoaded()
    {
        //incase you wonder why this whole thing is here when technically the end of playermovemanager would guarentee all this to be true so it could be called there
        //its bc technically since awake hasn't finished the scene isnt loaded in yet so popups can't occur (racist) so i see no alternative
        while (MapTileSpecialEvents.Instance == null)
            yield return null;

        while (MapTileSpecialEvents.Instance.mapTiles == null)
            yield return null;

        while (PlayerMoveManager.Instance == null)
            yield return null;

        while (PlayerMoveManager.Instance.mapNumber >=MapTileSpecialEvents.Instance.mapTiles.Length)
            yield return null;

        while (MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber] == null)
            yield return null;

        PreturnStuff();
    }

    //specifically for undoing finder
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void UndoItemUseRpc()
    {
        if(displayText.gameObject.activeSelf) { return; }
        display.transform.parent.gameObject.SetActive(true);
        display.gameObject.SetActive(true);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void UndoClassAbilityRpc()
    {
        StartCoroutine(WaitUntilBoxGone());
    }

    //mickeymouse unlucky
    private IEnumerator WaitUntilBoxGone()
    {
        while (displayText.gameObject.activeSelf)
        {
            yield return null;
        }

        mainMenuButtons.gameObject.SetActive(true);
    }

   
}