using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Netcode.NetworkSceneManager;

public class ClientChecks : NetworkBehaviour
{
    [SerializeField] private DisplayInventory display;


    public static ClientChecks Instance { get; private set; }

    public DialogueScript displayText;
    public GameObject mainMenuButtons;
    
    private TextMeshProUGUI displayTxt;
    public TextMeshProUGUI rollNum;
    public UnityEvent onRoundStart;
    public GameObject combatPreview;
    public UnityEvent onItemUse;//i'd like to say that im not that happy about whats going on here but this has to be better than updating stat UI every frame
    public UnityEvent onClassAbilityUse;

    public Image worldEventImage;
    private bool loadedIn = false;
    //im gonna be so fr this whole thingy i have going on with this class is some big buns and im sorry to anyone who looks at this
    //(the main issue is im doing wayyy to much in here in the worst ways possible
   
    
    public override void OnNetworkSpawn()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        if(IsHost)
        {
            StartCoroutine(WaitUntilAllLoaded());
        }
       
        

        //StartCoroutine(WaitUntilAllLoaded());
        /* if(SceneChanger.Instance.everyoneLoaded())
         {
             PreturnStuff();

             Debug.Log("fmcl bruh WHY DOES IT DO IT MULTIPLE TIMES FOR EACH CLIENT THAT LOADS IN ON THE SERVER ");
         }*/



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
                SceneChanger.Instance.loadClientScenesAddidtiveRpc("MainGameScene");
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

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void EveryoneLockInRpc()
    {
        PreturnStuff();
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
        
    }

    
    public void TurnStartChecks()
    {
        bool rumble = false;
        //at some point im probably gonna have to make this a different event but frick u
        onRoundStart.Invoke();
        foreach (var players in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players)
        {

            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && !NetworkData.Instance.players[players].isDead && PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {

                rumble = true;
            }
        }

        if ((MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Count == 0 && !rumble) && !NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead)
        {

            MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players.Remove(NetworkData.Instance.currentPlayer);
            PlayerMoveManager.Instance.playerCam.Follow = PlayerMoveManager.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform;
            mainMenuButtons.SetActive(true);
            
            PopUpManager.Instance.PerformPopUp(0);

            rollNum.gameObject.transform.parent.gameObject.SetActive(false);
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

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmBuffRpc(int player, int itemId, int inventoryNum)
    {


        NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][inventoryNum]);
        displayText.lines.Clear();

        display.CreateDisplay( player, inventoryNum);
        display.gameObject.SetActive(false);
        

        displayText.lines.Add(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].useText);
        StartCoroutine(usedItem());
        onItemUse.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmItemPickupRpc(int player, int itemId, int inventoryNum)
    {
        displayText.lines.Clear();
        //?displayText = ItemPickupDisplay.Instance.gameObject;
        if (NetworkData.Instance.playerInventories[player][inventoryNum].AddItem(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId]))
        {
            
            displayText.lines.Add("Obtained a <color=blue>" + NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].name + "</color>");
            StartCoroutine(displayItem());
        }
        else
        {
            displayText.lines.Add("You've got NO ROOM for that ish stoopid (hopefully in the future u can pick what u want)");
            StartCoroutine(displayItem());

        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void SyncEnemyRpc(int encounterId)
    {

        string encounterName = PlayerCombatManager.Instance.BattleSetUp(encounterId);
        combatPreview.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name.ToString();
       
        
        combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = encounterName;
              
      
        
        StartCoroutine(previewFight());
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void SyncEventRpc(int eventNum)
    {
        
        NetworkData.Instance.currentEvent = (PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId] as DefaultTile).events[eventNum];
        SceneChanger.Instance.loadClientScenesServerRpc(NetworkData.Instance.currentEvent.SceneToGoTo);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void WorldEventRpc()
    {
        if (WorldEventManager.Instance.eventsToActivate.Count > 0) { StartCoroutine(displayActivateEvent()); }
        else if (WorldEventManager.Instance.eventsToDeactivate.Count > 0) { StartCoroutine(displayDeactivateEvent()); }

    }
    /* [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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

    [Rpc(SendTo.ClientsAndHost,RequireOwnership = false)]
    public void DisplayDeadRpc()
    {
        displayText.lines.Clear();
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].progressDeath();
        displayText.lines.Add(NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name + " is dead for <color=red>" + (NetworkData.Instance.players[NetworkData.Instance.currentPlayer].tillRevive + 1) + "</color> turns");
        
        StartCoroutine(displayItem());
        
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void DeployTrapRpc(int tileId, int trapId)
    {
        MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][tileId].trapIds.Add(trapId);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void UseClassAbilityRpc()
    {
        displayText.lines.Clear();
        NetworkData.Instance.classDataBase.GetItem[NetworkData.Instance.GetCurrentPlayer().playerClass].ClassAction(NetworkData.Instance.GetCurrentPlayer());
        displayText.lines.Add(NetworkData.Instance.classDataBase.GetItem[NetworkData.Instance.GetCurrentPlayer().playerClass].actionUseText);
        
        StartCoroutine(usedAbility());
        onClassAbilityUse.Invoke();

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ActivateTrapsRpc()
    {
        StartCoroutine(TrapActivates());
    }
    private IEnumerator previewFight()
    {

        combatPreview.SetActive(true);
        yield return new WaitForSecondsRealtime(5f);
        SceneChanger.Instance.loadClientScenesServerRpc(PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.GetCurrentPlayer().curTileId].battleEnvironment);

    }
    private IEnumerator TrapActivates()
    {
        displayText.lines.Clear();
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        var trapcache = NetworkData.Instance.trapDataBase.GetTrap[MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds[0]];
        displayText.lines.Add(trapcache.TrapString());
        trapcache.TrapEffect(NetworkData.Instance.players[NetworkData.Instance.currentPlayer]);
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
    private IEnumerator displayItem()
    {
        
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {

            yield return null;
        }
        if (IsServer)
        PlayerMoveManager.Instance.NextTurnRpc();

    }
    private IEnumerator usedItem()
    {
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {
            
            yield return null;
        }
        
        display.gameObject.SetActive(true);
        
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
        WorldEventManager.Instance.eventsToActivate[0].OnActivate();
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        WorldEventManager.Instance.activeWorldEvents.Add(new WorldEventWrapper(WorldEventManager.Instance.worldDatabase.GetId[WorldEventManager.Instance.eventsToActivate[0]]));
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {
            yield return null;
        }
        worldEventImage.gameObject.SetActive(false);
        WorldEventManager.Instance.eventsToActivate.RemoveAt(0);
        if (WorldEventManager.Instance.eventsToActivate.Count > 0) { StartCoroutine(displayActivateEvent()); }

        else if (WorldEventManager.Instance.eventsToDeactivate.Count > 0 ) {  StartCoroutine(displayDeactivateEvent()); }

        else { TurnStartChecks(); }
    }
    private IEnumerator displayDeactivateEvent()
    {
        displayText.lines.Clear();
        displayText.lines.Add(WorldEventManager.Instance.eventsToDeactivate[0].DeactivateText);
        displayText.gameObject.SetActive(true);
        displayText.whoInControl = NetworkData.Instance.currentPlayer;
        WorldEventManager.Instance.eventsToDeactivate[0].OnDeactivate();
        displayText.Awake();
        while (displayText.gameObject.activeSelf)
        {
            yield return null;
        }
        for(int i = WorldEventManager.Instance.activeWorldEvents.Count - 1; i >= 0; i--)
        {
            if (WorldEventManager.Instance.activeWorldEvents[i].eventId == WorldEventManager.Instance.worldDatabase.GetId[WorldEventManager.Instance.eventsToDeactivate[0]]) 
            {
                WorldEventManager.Instance.activeWorldEvents.RemoveAt(i);
            }
        }

        WorldEventManager.Instance.eventsToDeactivate.RemoveAt(0);
        if (WorldEventManager.Instance.eventsToDeactivate.Count > 0) { StartCoroutine(displayDeactivateEvent()); }
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
        while (!SceneChanger.Instance.everyoneLoaded())
        {
            yield return null;
        }




        EveryoneLockInRpc();
    }
}