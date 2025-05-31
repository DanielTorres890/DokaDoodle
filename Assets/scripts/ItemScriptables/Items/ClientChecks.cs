using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientChecks : NetworkBehaviour
{
    [SerializeField] private DisplayInventory display;


    public static ClientChecks Instance { get; private set; }

    public GameObject displayText;
    public GameObject mainMenuButtons;
    private TextMeshProUGUI displayTxt;

    public UnityEvent onRoundStart;
    public GameObject combatPreview;

    public override void OnNetworkSpawn()
    {
        
        if(SceneChanger.Instance.everyoneLoaded())
        {
            PreturnStuff();
            Debug.Log("fmcl bruh WHY DOES IT DO IT MULTIPLE TIMES FOR EACH CLIENT THAT LOADS IN ON THE SERVER ");
        }
        
        
        
    }

 
    public void PreturnStuff()
    {
        NetworkData.Instance.GetCurrentPlayer().playerInfo[PlayerInfo.classCd] -= 1;
        NetworkData.Instance.ProgressStatus(NetworkData.Instance.currentPlayer);
        WorldEventManager.Instance.ProgressDay();
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        onRoundStart = new UnityEvent();
        displayTxt = displayText.GetComponentInChildren<TextMeshProUGUI>();
    }

    
    public void TurnStartChecks()
    {
        bool rumble = false;
        onRoundStart.Invoke();
        foreach (var players in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players)
        {

            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && !NetworkData.Instance.players[players].isDead && PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {
                Debug.Log("we tried to fight even tho we can't");
                rumble = true;
            }
        }

        if ((MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Count == 0 && !rumble) && !NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead)
        {

            MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players.Remove(NetworkData.Instance.currentPlayer);
            PlayerMoveManager.Instance.playerCam.Follow = PlayerMoveManager.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform;
            PlayerMoveManager.Instance.gameMenu.SetActive(true);
            PlayerMoveManager.Instance.rollNum.gameObject.transform.parent.gameObject.SetActive(false);
        }

        else
        {

            if (NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead)
            {
                PlayerMoveManager.Instance.gameMenu.SetActive(false);

                if (IsServer) { ClientChecks.Instance.DisplayDeadRpc(); }
                return;
            }


            PlayerMoveManager.Instance.gameMenu.SetActive(false);
            if (IsServer) { SyncEnemyRpc(0); }

        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmBuffRpc(int player, int itemId, int inventoryNum)
    {


        NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][inventoryNum]);

        
        display.CreateDisplay( player, inventoryNum);
        display.gameObject.SetActive(false);
        Debug.Log("i should be hidden");

        displayTxt.text = NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].useText;
        StartCoroutine(usedItem());
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmItemPickupRpc(int player, int itemId, int inventoryNum)
    {
        displayText.SetActive(true);
        displayText = ItemPickupDisplay.Instance.gameObject;
        if (NetworkData.Instance.playerInventories[player][inventoryNum].AddItem(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId]))
        {
            
            
            displayTxt.text = "Obtained a <color=blue>" + NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].name + "</color>";
            StartCoroutine(displayItem());
        }
        else
        {
            displayTxt.text = "You've got NO ROOM for that ish stoopid (hopefully in the future u can pick what u want)";
            StartCoroutine(displayItem());

        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void SyncEnemyRpc(int enemyId)
    {
        PlayerCombatManager.Instance.combatants.Clear();
        PlayerCombatManager.Instance.combatants.Add(NetworkData.Instance.players[NetworkData.Instance.currentPlayer]);
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].setCombatActions();


        bool rumble = false;
        
        foreach (var players in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players)
        {
            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {
                PlayerCombatManager.Instance.combatants.Add(NetworkData.Instance.players[players]);
                NetworkData.Instance.players[players].setCombatActions();
                rumble = true;
                
            }
            
        }
        
        //Pretty much everything that isn't these two is stuff from the old system
        
            if (MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Count == 0)
            {
                if (!rumble) 
                {
                    var temp = new EnemyCombat(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[enemyId]);
                    PlayerCombatManager.Instance.combatants.Add(temp);
                    MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Add(temp);
                }
            
                
            }
            else
            {
                foreach (var enemyy in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy)
                {
                    PlayerCombatManager.Instance.combatants.Add(enemyy);
                }

            }
        
      

        combatPreview.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name.ToString();
       
        
        combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatants[1].name.ToString();
           
            
        
      
        
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
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].progressDeath();
        displayTxt.text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name + " is dead for <color=red>" + (NetworkData.Instance.players[NetworkData.Instance.currentPlayer].tillRevive + 1) + "</color> turns";
        
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
        Debug.Log("I happened ");
        NetworkData.Instance.classDataBase.GetClass[NetworkData.Instance.GetCurrentPlayer().playerClass].ClassAction(NetworkData.Instance.GetCurrentPlayer());
        displayTxt.text = NetworkData.Instance.classDataBase.GetClass[NetworkData.Instance.GetCurrentPlayer().playerClass].actionUseText;
        StartCoroutine(usedAbility());

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
        SceneChanger.Instance.loadClientScenesServerRpc("NewBattleArea");

    }
    private IEnumerator TrapActivates()
    {
        var trapcache = NetworkData.Instance.trapDataBase.GetTrap[MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds[0]];
        displayTxt.text = trapcache.TrapString();
        trapcache.TrapEffect(NetworkData.Instance.players[NetworkData.Instance.currentPlayer]);
        MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds.RemoveAt(0);
        displayText.SetActive(true);
        while (displayText.activeSelf)
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
        
        displayText.SetActive(true);
        while (displayText.activeSelf)
        {

            yield return null;
        }
        if (IsServer)
        PlayerMoveManager.Instance.NextTurnRpc();

    }
    private IEnumerator usedItem()
    {
        displayText.SetActive(true);
        
        while (displayText.activeSelf)
        {
            
            
            yield return null;
        }
        
        display.gameObject.SetActive(true);
        Debug.Log(" ive been seened");
    }
    private IEnumerator usedAbility()
    {
        mainMenuButtons.SetActive(false);
        displayText.SetActive(true);

        mainMenuButtons.transform.GetChild(3).gameObject.GetComponent<ClassAbility>().setButtonText(); //nasty work i shouuld redo this frfr
        while (displayText.activeSelf)
        {


            yield return null;
        }

        mainMenuButtons.SetActive(true);
    }

    private IEnumerator displayActivateEvent()
    {
        displayTxt.text = WorldEventManager.Instance.eventsToActivate[0].ActivateText;
        WorldEventManager.Instance.eventsToActivate[0].OnActivate();
        displayText.SetActive(true);
        WorldEventManager.Instance.activeWorldEvents.Add(new WorldEventWrapper(WorldEventManager.Instance.worldDatabase.GetId[WorldEventManager.Instance.eventsToActivate[0]]));
        while (displayText.activeSelf)
        {
            yield return null;
        }
        WorldEventManager.Instance.eventsToActivate.RemoveAt(0);
        if (WorldEventManager.Instance.eventsToActivate.Count > 0) { StartCoroutine(displayActivateEvent()); }

        else if (WorldEventManager.Instance.eventsToDeactivate.Count > 0 ) {  StartCoroutine(displayDeactivateEvent()); }

        else { TurnStartChecks(); }
    }
    private IEnumerator displayDeactivateEvent()
    {
        displayTxt.text = WorldEventManager.Instance.eventsToDeactivate[0].DeactivateText;
        displayText.SetActive(true);
        WorldEventManager.Instance.eventsToActivate[0].OnDeactivate();
        
        while (displayText.activeSelf)
        {
            yield return null;
        }
        WorldEventManager.Instance.eventsToDeactivate.RemoveAt(0);
        if (WorldEventManager.Instance.eventsToDeactivate.Count > 0) { StartCoroutine(displayDeactivateEvent()); }
        else { TurnStartChecks(); }
    }
}