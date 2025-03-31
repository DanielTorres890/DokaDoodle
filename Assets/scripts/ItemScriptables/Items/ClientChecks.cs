using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClientChecks : NetworkBehaviour
{
    [SerializeField] private DisplayInventory display;


    public static ClientChecks Instance { get; private set; }

    public GameObject displayText;
    private TextMeshProUGUI displayTxt;

    public GameObject combatPreview;

    public override void OnNetworkSpawn()
    {
        
        NetworkData.Instance.ProgressStatus(NetworkData.Instance.currentPlayer);
        bool rumble = false;
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
           
                if(IsServer) { ClientChecks.Instance.DisplayDeadRpc(); }
                return;
            }

            
            PlayerMoveManager.Instance.gameMenu.SetActive(false);
            if (IsServer) { SyncEnemyRpc(0); }
            
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        

        displayTxt = displayText.GetComponentInChildren<TextMeshProUGUI>();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmBuffRpc(int player, int itemId, int inventoryNum)
    {


        NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].PerformItemEffect(player, NetworkData.Instance.playerInventories[player][inventoryNum]);

        
        display.CreateDisplay( player, inventoryNum);
        display.gameObject.SetActive(false);

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
        SceneChanger.Instance.loadClientScenesServerRpc("EventScreen");
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
        Debug.Log("does progressing death break u");
        
        Debug.Log("displaying def shouldn't");
        StartCoroutine(displayItem());
        
    }
    private IEnumerator previewFight()
    {

        combatPreview.SetActive(true);
        yield return new WaitForSecondsRealtime(5f);
        SceneChanger.Instance.loadClientScenesServerRpc("NewBattleArea");

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
    }
}