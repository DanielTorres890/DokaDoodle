using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : NetworkBehaviour
{
    // Start is called before the first frame update
    private GameObject fighter1;
    private GameObject fighter2;

  

    [SerializeField] private TextMeshProUGUI fighter1Text;
    [SerializeField] private TextMeshProUGUI fighter2Text;

    [SerializeField] private TextMeshProUGUI fighter1Name;
    [SerializeField] private TextMeshProUGUI fighter2Name;

    [SerializeField] private TextMeshProUGUI turnOrder1Text;
    [SerializeField] private TextMeshProUGUI turnOrder2Text;

    [SerializeField] private TextMeshProUGUI damageText;
    private int turnOrder;

    [SerializeField] private List<Button> order1Buttons; //Attack/Defend, Magic/MDefend, Flee/Special, Ultimate/Special
    [SerializeField] private List<Button> order2Buttons;


    private int totalAllowedTurns;
    private int totalTurnsTaken;
    private int fighter1Choice;
    private int fighter2Choice;
    public override void OnNetworkSpawn()
    {
        totalAllowedTurns = 2;
        totalTurnsTaken = 0;
        fighter1 = NetworkData.Instance.playerSticks[(PlayerCombatManager.Instance.combatant1 as playerData).playerNumber];
        fighter1.transform.position = new Vector3(-150 ,-80,200);
        fighter1.transform.localScale = new Vector3(10, 10, 1);

        if(PlayerCombatManager.Instance.combatant2 is playerData)
        {
            fighter2 = NetworkData.Instance.playerSticks[(PlayerCombatManager.Instance.combatant2 as playerData).playerNumber];
            fighter2.transform.localPosition = new Vector3(248, -146, -20);
        }
        else
        {
            var tmp = PlayerCombatManager.Instance.combatant2 as EnemyCombat;
            fighter2 = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[tmp.enemyId].enemyPrefab);
           
        }

        fighter2.transform.position = new Vector3(150, -80, 200);
        fighter2.transform.localScale = new Vector3(10, 10, 1);
        SetStatUI();
        PickOrder();
        SetButtonNames();
    }

    
    private void SetStatUI()
    {
        fighter1Name.text = PlayerCombatManager.Instance.combatant1.name;
        fighter2Name.text = PlayerCombatManager.Instance.combatant2.name;
        fighter1Text.text = "";
        fighter2Text.text = "";


        fighter1Text.text += "HP: " + PlayerCombatManager.Instance.combatant1.stats[Attributes.Health] + "/" + PlayerCombatManager.Instance.combatant1.stats[Attributes.MaxHealth] + "\n";
        fighter2Text.text += "HP: " + PlayerCombatManager.Instance.combatant2.stats[Attributes.Health] + "/" + PlayerCombatManager.Instance.combatant2.stats[Attributes.MaxHealth] + "\n";
        foreach (var temp in PlayerCombatManager.Instance.combatant1.stats.Keys)
        {
            if(temp == Attributes.Health || temp == Attributes.MaxHealth) { continue; }
            fighter1Text.text += temp.ToString().Remove(3)+ ": " + PlayerCombatManager.Instance.combatant1.stats[temp] + "\n";
            
        }
        foreach (var temp in PlayerCombatManager.Instance.combatant2.stats.Keys)
        {
            if (temp == Attributes.Health || temp == Attributes.MaxHealth) { continue; }
            fighter2Text.text += temp.ToString().Remove(3) + ": " + PlayerCombatManager.Instance.combatant2.stats[temp] + "\n";
        }

        
        
        
        
        
    }

  
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void setTurnUIRpc(int order)
    {
        turnOrder = order;
        if (turnOrder == 0)
        {
            turnOrder1Text.text = "First";
            turnOrder2Text.text = "Second";
            

            for (int i = 0; i < order1Buttons.Count; i++)
            {
                var tmp = i;
                order1Buttons[tmp].onClick.AddListener(delegate { setCombatantAction(0, tmp); });
                order2Buttons[tmp].onClick.AddListener(delegate { setCombatantAction(1, tmp); });
                Debug.Log("Button1 Event Set");
            }

           

        }
        else
        {
            turnOrder1Text.text = "Second";
            turnOrder2Text.text = "First";

            for (int i = 0; i < order1Buttons.Count; i++)
            {
                var tmp = i;
                order1Buttons[tmp].onClick.AddListener(delegate { setCombatantAction(0, tmp); });
                order2Buttons[tmp].onClick.AddListener(delegate { setCombatantAction(1, tmp); });
                Debug.Log("Button2 Event Set");
            }
        }
        StartCoroutine(Delay(5f));
    }

    private void setCombatantAction(int combatant, int action)
    {

        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        setCombatantActionRpc(combatant, action);
        
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void setCombatantActionRpc(int combatant, int action)
    {

        if (turnOrder == 0 && combatant == 0)
        {
            fighter1Choice = action;
            order2Buttons[0].gameObject.transform.parent.gameObject.SetActive(true);
            order1Buttons[0].gameObject.transform.parent.gameObject.SetActive(false);

            return;
        }
        if (turnOrder == 0 && combatant != 0)
        {
            fighter2Choice = action;
            order2Buttons[0].gameObject.transform.parent.gameObject.SetActive(false);
            Debug.Log("Did Attempt to do something");
            Attack();
        }

        if (turnOrder == 1 && combatant != 0)
        {
            fighter2Choice = action;
            order2Buttons[0].gameObject.transform.parent.gameObject.SetActive(false);
            order1Buttons[0].gameObject.transform.parent.gameObject.SetActive(true);
            return;
        }
        if (turnOrder == 1 && combatant == 0)
        {
            fighter1Choice = action;
 
            order1Buttons[0].gameObject.transform.parent.gameObject.SetActive(false);
            Attack();
        
        }
        
    }
    private void Attack()
    {
        float damageDealt = 0f;
        
       

       
        if (turnOrder == 0 )
        {
            playerData offense = PlayerCombatManager.Instance.combatant1 as playerData;
            if (PlayerCombatManager.Instance.combatant2 is playerData) 
            {
                playerData defense = PlayerCombatManager.Instance.combatant2 as playerData;

                
            }
            else
            {
                EnemyCombat defense = PlayerCombatManager.Instance.combatant2 as EnemyCombat;

                
                    //foreach (var multipliers in (NetworkData.Instance.playerInventories[0][1].database.GetItem[offense.equipItems[ItemType.Weapon]] as WeaponItem).attack.multipliers)
                for (int i = 0; i < (NetworkData.Instance.playerInventories[0][1].database.GetItem[offense.equipItems[ItemType.Weapon]] as WeaponItem).attack.multipliers.Length; i++ )    
                {
                    var multipliers = (NetworkData.Instance.playerInventories[0][1].database.GetItem[offense.equipItems[ItemType.Weapon]] as WeaponItem).attack.multipliers[i];
                    var antiguardMultipliers = (NetworkData.Instance.playerInventories[0][1].database.GetItem[offense.equipItems[ItemType.Weapon]] as WeaponItem).attack.antiGuardMultipliers[i];
                    
                    //Key terms off = attacking entity, defr = defending entity, 
                    // (offatk.multiplier * offstat) - (defstat * defguardmult * offstatpierce)
                    damageDealt += multipliers.mult * offense.stats[multipliers.attribute] - defense.stats[multipliers.attribute] * PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[defense.enemyId].Defend[fighter2Choice].multipliers[i].mult * antiguardMultipliers.mult;
                }

                if (fighter1Choice == fighter2Choice)
                {
                    damageDealt *= 1 -  (PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[defense.enemyId].Defend[fighter2Choice].defendPercentage/100);
                }
                else { damageDealt *= 1 - (PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[defense.enemyId].Defend[fighter2Choice].defendPercentage / 100) * 0.5f; }


            }
            PlayerCombatManager.Instance.combatant2.stats[Attributes.Health] -=  Mathf.RoundToInt(damageDealt);
        }
        else
        {
            playerData defense = PlayerCombatManager.Instance.combatant1 as playerData;

            if (PlayerCombatManager.Instance.combatant2 is playerData)
            {
                playerData offense = PlayerCombatManager.Instance.combatant2 as playerData;


            }
            else
            {
                EnemyCombat offense = PlayerCombatManager.Instance.combatant2 as EnemyCombat;


                //foreach (var multipliers in (NetworkData.Instance.playerInventories[0][1].database.GetItem[offense.equipItems[ItemType.Weapon]] as WeaponItem).attack.multipliers)
                for (int i = 0; i < PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[offense.enemyId].Defend[fighter2Choice].multipliers.Length; i++)
                {
                    var multipliers = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[offense.enemyId].Defend[fighter2Choice].multipliers[i];
                    var antiguardMultipliers = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[offense.enemyId].Defend[fighter2Choice].antiGuardMultipliers[i];

                    
                    //defending attribute is what stat is used as the defending stat since it won't always be one stat against another
                    var defendingAttribute = multipliers.attribute;
                    if (multipliers.attribute == Attributes.Attack)
                    {
                        defendingAttribute = Attributes.Defense;
                    }
                    else if (multipliers.attribute == Attributes.Magic) 
                    {
                        defendingAttribute = Attributes.MDefense;
                    }


                    //Key terms off = attacking entity, defr = defending entity,
                    // (offatk.multiplier * offstat) - (defstat * defguardmult * offstatpierce)
                    damageDealt += multipliers.mult * offense.stats[multipliers.attribute] - defense.stats[defendingAttribute] * (NetworkData.Instance.playerInventories[0][1].database.GetItem[defense.equipItems[ItemType.Shield]] as WeaponItem).attack.multipliers[i].mult * antiguardMultipliers.mult;
                }

                if (fighter1Choice == fighter2Choice)
                {
                    damageDealt *= 1 - (((NetworkData.Instance.playerInventories[0][1].database.GetItem[defense.equipItems[ItemType.Shield]] as WeaponItem).attack as DefenseBase).defendPercentage / 100 );
                }
                else { damageDealt *= 1 - ((NetworkData.Instance.playerInventories[0][1].database.GetItem[defense.equipItems[ItemType.Shield]] as WeaponItem).attack as DefenseBase).defendPercentage * 0.5f; }


            }

            PlayerCombatManager.Instance.combatant1.stats[Attributes.Health] -= zeroMinimum(Mathf.RoundToInt(damageDealt));
        }

        var finaldmgdealt = zeroMinimum(Mathf.RoundToInt(damageDealt));
        damageText.text = "Dealt" + finaldmgdealt + "damage";


        
        StartCoroutine(NextTurn());


    }
    
    private void SetButtonNames()
    {
        var player1 = (PlayerCombatManager.Instance.combatant1 as playerData);
        if (turnOrder == 0)
        {
            order1Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][1].database.GetItem[player1.equipItems[ItemType.Weapon]] as WeaponItem).attack.attackName;
            order1Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][2].database.GetItem[player1.equipItems[ItemType.Magic]] as WeaponItem).attack.attackName;

            if (PlayerCombatManager.Instance.combatant2 is playerData)
            {
                var player2 = (PlayerCombatManager.Instance.combatant2 as playerData);
                order2Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][1].database.GetItem[player2.equipItems[ItemType.Shield]] as WeaponItem).attack.attackName;
                order2Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][2].database.GetItem[player2.equipItems[ItemType.MagicGuard]] as WeaponItem).attack.attackName;
            }
            else
            {
                var player2 = (PlayerCombatManager.Instance.combatant2 as EnemyCombat);
                order2Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[player2.enemyId].Defend[0].attackName;
                order2Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[player2.enemyId].Defend[1].attackName;
            }
        }
        else
        {
            order2Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][1].database.GetItem[player1.equipItems[ItemType.Weapon]] as WeaponItem).attack.attackName;
            order2Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][2].database.GetItem[player1.equipItems[ItemType.Magic]] as WeaponItem).attack.attackName;

            if (PlayerCombatManager.Instance.combatant2 is playerData)
            {
                var player2 = (PlayerCombatManager.Instance.combatant2 as playerData);
                order1Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][1].database.GetItem[player2.equipItems[ItemType.Shield]] as WeaponItem).attack.attackName;
                order1Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = (NetworkData.Instance.playerInventories[0][2].database.GetItem[player2.equipItems[ItemType.MagicGuard]] as WeaponItem).attack.attackName;
            }
            else
            {
                var player2 = (PlayerCombatManager.Instance.combatant2 as EnemyCombat);
                order1Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[player2.enemyId].Defend[0].attackName;
                order1Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[player2.enemyId].Defend[1].attackName;
            }

        }
      

    }
    private void PickOrder(int preset = -1)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        if (preset == -1)
        {
            setTurnUIRpc(Random.Range(0, 2));
            return;
        }
        setTurnUIRpc(turnOrder);
        //
    }
    private IEnumerator Delay(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        turnOrder1Text.gameObject.SetActive(false);
        turnOrder2Text.gameObject.SetActive(false);

        if (turnOrder == 0 )
        order1Buttons[0].gameObject.transform.parent.gameObject.SetActive(true);
        else
        order2Buttons[0].gameObject.transform.parent.gameObject.SetActive(true);
    }


    private IEnumerator NextTurn()
    {
        damageText.transform.parent.gameObject.SetActive(true);
        
        
        
        while (damageText.transform.parent.gameObject.activeSelf)
        {
           
            yield return null;
        }
         Debug.Log(damageText.transform.parent.gameObject.activeSelf);
        if (turnOrder == 0) 
        { 
            turnOrder = 1;
            print("TURN CHANGED to 0");
        }
        
        else 
        { 
            turnOrder = 0;
            print("TURN CHANGED to 1");
        }

        if (totalTurnsTaken >= totalAllowedTurns)
        {
            fighter1.transform.localScale = new Vector3(100, 100, 1);
            NetworkData.Instance.setNextTurnNum();

            SceneChanger.Instance.loadClientScenesServerRpc("MainGameScene");
        }
        totalTurnsTaken += 1;
        SetStatUI();
        PickOrder(turnOrder);
        SetButtonNames();

    }
    private void CheckDeath()
    {
        if (PlayerCombatManager.Instance.combatant2.stats[Attributes.Health] <= 0)
        {

            (PlayerCombatManager.Instance.combatant1 as playerData).totalXp += PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.currentPlayer][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.enemyId].droppedXp;
            MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.currentPlayer][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy = null;
        }
        if (PlayerCombatManager.Instance.combatant1.stats[Attributes.Health] <= 0)
        {

        }
        
    }
    private int zeroMinimum(int numToCheck)
    {
        if (numToCheck < 0) 
            return 0;
        else 
            return numToCheck;
    }
}
