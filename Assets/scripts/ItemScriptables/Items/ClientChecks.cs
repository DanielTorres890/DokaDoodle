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

        
        display.CreateDisplay(inventoryNum, player);
        display.gameObject.SetActive(false);

        displayTxt.text = NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].useText;
        StartCoroutine(usedItem());
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ConfirmItemPickupRpc(int player, int itemId, int inventoryNum)
    {
        NetworkData.Instance.playerInventories[player][inventoryNum].AddItem(NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId]);
        displayText.SetActive(true);
        displayText = ItemPickupDisplay.Instance.gameObject;
        displayTxt.text = "Obtained a <color=blue>" + NetworkData.Instance.playerInventories[player][inventoryNum].database.GetItem[itemId].name + "</color>";
        StartCoroutine(displayItem());
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void SyncEnemyRpc(int enemyId)
    {
        EnemyCombat enemy;

        //Pretty much everything that isn't these two is stuff from the old system
        PlayerCombatManager.Instance.combatants.Clear();
        PlayerCombatManager.Instance.combatants.Add(NetworkData.Instance.players[NetworkData.Instance.currentPlayer]);

        if ( MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy == null )
        {
            enemy = new EnemyCombat(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[enemyId]);
        }
        else
        {
            enemy = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy;
        }
        
        PlayerCombatManager.Instance.combatant1 = NetworkData.Instance.players[NetworkData.Instance.currentPlayer];
        PlayerCombatManager.Instance.combatant2 = enemy;
        PlayerCombatManager.Instance.combatants.Add(enemy);

        bool rumble = false;
        int counter = 0;
        foreach (var players in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players)
        {
            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {
                rumble = true;
                break;
            }
            counter++;
        }
        if (rumble)
        {
            PlayerCombatManager.Instance.combatant2 = NetworkData.Instance.players[counter];
        }
        //PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.currentPlayer].tileEnemy = enemy;



        combatPreview.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name.ToString();
        if (PlayerCombatManager.Instance.combatant2 is playerData)
        {
            combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = NetworkData.Instance.players[(PlayerCombatManager.Instance.combatant2 as playerData).playerNumber].name.ToString();
        }
        else
        {
            Debug.Log(PlayerCombatManager.Instance.combatant2.name.ToString());
            combatPreview.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant2.name.ToString();
            MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy = enemy;
            
        }
      
        
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
        displayTxt.text = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].name + " is dead for <color=red>" + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].tillRevive + "</color> turns";
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].progressDeath();
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