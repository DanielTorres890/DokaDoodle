using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.Experimental.GraphView;
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
        Instance = this;
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
        var enemy = new EnemyCombat(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[enemyId]);   
        PlayerCombatManager.Instance.combatant1 = NetworkData.Instance.players[NetworkData.Instance.currentPlayer];
        PlayerCombatManager.Instance.combatant2 = enemy;
        PlayerMoveManager.Instance.mapTiles[NetworkData.Instance.currentPlayer].tileEnemy = enemy;


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

    private IEnumerator previewFight()
    {

        combatPreview.SetActive(true);
        yield return new WaitForSecondsRealtime(5f);
        SceneChanger.Instance.loadClientScenesServerRpc("BattleScene");

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