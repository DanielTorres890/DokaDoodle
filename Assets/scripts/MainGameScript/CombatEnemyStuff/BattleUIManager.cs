using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : NetworkBehaviour
{
    // Prettty much everything and anything to do with the stuff that happens during a fight which relies a lot on the PlayerCombat manager singleton (that tracks who is fighting)
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


    private bool displayDrop;
    private bool displayXp;
    public override void OnNetworkSpawn()
    {
        displayDrop = true;
        displayXp = true;
        totalAllowedTurns = 2;
        totalTurnsTaken = -1;
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
           
            }
        }
        SetButtonNames();
        totalTurnsTaken += 1;

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
        
       
        //this is kinda butt rn but could change entity class to hold attacks/defenses array and can populate the array on item equip might as well
       
        if (turnOrder == 0 )
        {
            
                    //foreach (var multipliers in (NetworkData.Instance.playerInventories[0][1].database.GetItem[offense.equipItems[ItemType.Weapon]] as WeaponItem).attack.multipliers)
                for (int i = 0; i < PlayerCombatManager.Instance.combatant1.attacks[fighter1Choice].multipliers.Length; i++ )    
                {
 
                    var multipliers = PlayerCombatManager.Instance.combatant1.attacks[fighter1Choice].multipliers[i];
                    var antiguardMultipliers = PlayerCombatManager.Instance.combatant1.attacks[fighter1Choice].antiGuardMultipliers[i];
                    
                    //Key terms off = attacking entity, defr = defending entity, 
                    // (offatk.multiplier * offstat) - (defstat * defguardmult * offstatpierce)
                    damageDealt += multipliers.mult * PlayerCombatManager.Instance.combatant1.stats[multipliers.attribute] - PlayerCombatManager.Instance.combatant2.stats[multipliers.attribute] * PlayerCombatManager.Instance.combatant2.defenses[fighter2Choice].multipliers[i].mult * antiguardMultipliers.mult;
                }

                if (fighter1Choice == fighter2Choice)
                {
                    damageDealt *= 1 -  (PlayerCombatManager.Instance.combatant2.defenses[fighter2Choice].defendPercentage/100);
                }
                else { damageDealt *= 1 - (PlayerCombatManager.Instance.combatant2.defenses[fighter2Choice].defendPercentage / 100) * 0.5f; }


            
            PlayerCombatManager.Instance.combatant2.stats[Attributes.Health] -=  Mathf.RoundToInt(damageDealt);
        }
        else
        {
                //foreach (var multipliers in (NetworkData.Instance.playerInventories[0][1].database.GetItem[offense.equipItems[ItemType.Weapon]] as WeaponItem).attack.multipliers)
                for (int i = 0; i < PlayerCombatManager.Instance.combatant2.defenses[fighter2Choice].multipliers.Length; i++)
                {
                    var multipliers = PlayerCombatManager.Instance.combatant2.attacks[fighter2Choice].multipliers[i];
                    var antiguardMultipliers = PlayerCombatManager.Instance.combatant2.attacks[fighter2Choice].antiGuardMultipliers[i];

                    
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
                    damageDealt += multipliers.mult * PlayerCombatManager.Instance.combatant2.stats[multipliers.attribute] - PlayerCombatManager.Instance.combatant1.stats[defendingAttribute] * PlayerCombatManager.Instance.combatant1.attacks[fighter1Choice].multipliers[i].mult * antiguardMultipliers.mult;
                }

                if (fighter1Choice == fighter2Choice)
                {
                    damageDealt *= 1 - (PlayerCombatManager.Instance.combatant1.defenses[fighter1Choice].defendPercentage / 100 );
                }
                else { damageDealt *= 1 - PlayerCombatManager.Instance.combatant1.defenses[fighter1Choice] .defendPercentage / 100 * 0.5f; }


            

            PlayerCombatManager.Instance.combatant1.stats[Attributes.Health] -= zeroMinimum(Mathf.RoundToInt(damageDealt));
        }

        var finaldmgdealt = zeroMinimum(Mathf.RoundToInt(damageDealt));
        damageText.text = "Dealt" + finaldmgdealt + "damage";
        damageText.transform.parent.gameObject.SetActive(true);
        SetStatUI();
        CheckDeath();

        
        


    }
    
    private void SetButtonNames()
    {
       
        if (turnOrder == 0)
        {
            order1Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant1.attacks[0].name;
            order1Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant1.attacks[1].name;

            order2Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant2.defenses[0].name;
            order2Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant2.defenses[1].name;
        }
        
        else
        {
            order1Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant1.defenses[0].name;
            order1Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant1.defenses[0].name;

            order2Buttons[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant2.attacks[0].name;
            order2Buttons[1].gameObject.GetComponentInChildren<TextMeshProUGUI>().text = PlayerCombatManager.Instance.combatant2.attacks[1].name;

        }
      

    }
    private void PickOrder(int preset = -1)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId) && !NetworkManager.Singleton.IsHost) { return; }

        if (preset == -1)
        {
            setTurnUIRpc(Random.Range(1, 2));
            return;
        }
        setTurnUIRpc(turnOrder);
        //
    }
    private IEnumerator Delay(float time)
    {
       
        if (totalTurnsTaken == 0)
        yield return new WaitForSecondsRealtime(time);

        turnOrder1Text.gameObject.SetActive(false);
        turnOrder2Text.gameObject.SetActive(false);
        Debug.Log("Delay Turn Order " + turnOrder);
        if (turnOrder == 0 )
        order1Buttons[0].gameObject.transform.parent.gameObject.SetActive(true);
        else
        order2Buttons[0].gameObject.transform.parent.gameObject.SetActive(true);
    }


    private IEnumerator NextTurn()
    {
        
        
        
        
        while (damageText.transform.parent.gameObject.activeSelf)
        {
           
            yield return null;
        }
         Debug.Log("Turn Order: " + turnOrder);
        if(NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            if (turnOrder == 0)
            {
                turnOrder = 1;

            }

            else
            {
                turnOrder = 0;
            }
        }
       

        if (totalTurnsTaken >= totalAllowedTurns)
        {
            fighter1.transform.localScale = new Vector3(100, 100, 1);
            if (PlayerCombatManager.Instance.combatant2 is playerData) fighter2.transform.localScale = new Vector3(100, 100, 1);

            NetworkData.Instance.setNextTurnNum();

            SceneChanger.Instance.loadClientScenesServerRpc("MainGameScene");
        }
       
        
        SetStatUI();
        PickOrder(turnOrder);
       

    }
    private void CheckDeath()
    {

        if (PlayerCombatManager.Instance.combatant2.stats[Attributes.Health] <= 0)
        {
            if (PlayerCombatManager.Instance.combatant2 is EnemyCombat)
            {
                var player = PlayerCombatManager.Instance.combatant1 as playerData;
                var enemy = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.currentPlayer][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.enemyId];

                int droppedItem = enemy.rollItem();
                if (droppedItem != -1 && NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
                {
                    AddEnemyDropRpc(droppedItem);
                }
                displayXp = true;
                player.totalXp += PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.currentPlayer][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.enemyId].droppedXp;

                StartCoroutine(rewardsDisplay(droppedItem));
            }
            else
            {
                var player = PlayerCombatManager.Instance.combatant1 as playerData;
                var player2 = PlayerCombatManager.Instance.combatant2 as playerData;
                int gainedxp = zeroMinimum(player2.totalXp - player.totalXp);
                
                StartCoroutine(playerDefeatDisplay(0, gainedxp));

            }

        }
        else if (PlayerCombatManager.Instance.combatant1.stats[Attributes.Health] <= 0)
        {
            if (PlayerCombatManager.Instance.combatant1 is EnemyCombat)
            {
                var player = PlayerCombatManager.Instance.combatant1 as playerData;
                var enemy = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.currentPlayer][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.enemyId];

                int droppedItem = enemy.rollItem();
                if (droppedItem != -1 && NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
                {
                    AddEnemyDropRpc(droppedItem);
                }
                displayXp = true;
             

                StartCoroutine(rewardsDisplay(droppedItem));
            }
            else
            {
                playerData player = PlayerCombatManager.Instance.combatant2 as playerData;
                
                if(PlayerCombatManager.Instance.combatant2 is playerData)
                {
                    var player2 = PlayerCombatManager.Instance.combatant1 as playerData;
                    int gainedxp = zeroMinimum(player2.totalXp - player.totalXp);
                    player.totalXp += gainedxp;
                    StartCoroutine(playerDefeatDisplay(1, gainedxp));
                    return;
                }
                StartCoroutine(playerDefeatDisplay(1, -1));


            }
        }
        else
        {
            Debug.Log("If I died should not be here");
            StartCoroutine(NextTurn());
        }
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void AddEnemyDropRpc(int dropNumber)
    {
        var player = PlayerCombatManager.Instance.combatant1 as playerData;
        var enemy = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.enemyId];

        NetworkData.Instance.playerInventories[player.playerNumber][enemy.DroppedItems[dropNumber].determineType()].AddItem(enemy.DroppedItems[dropNumber]);
        displayDrop = true;

    }
    private IEnumerator rewardsDisplay(int dropNumber = -1)
    {

        while (damageText.transform.parent.gameObject.activeSelf)
        {
           
            yield return null;
        }

        var player = PlayerCombatManager.Instance.combatant1 as playerData;
        var enemy = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.currentPlayer][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.enemyId];
        
        if (displayDrop)
        {
            displayDrop = false;
            damageText.transform.parent.gameObject.SetActive(true);
            damageText.text = "Obtained a <color=blue>" + NetworkData.Instance.playerInventories[player.playerNumber][enemy.DroppedItems[dropNumber].determineType()].database.GetItem[dropNumber].name + "</color>";
        }
      
        while (damageText.transform.parent.gameObject.activeSelf)
        {
           
            yield return null;
        }
        if (displayXp)
        {
            displayXp = false;
            damageText.transform.parent.gameObject.SetActive(true);
            bool leveled = player.gainXp(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.currentPlayer][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.enemyId].droppedXp);
            damageText.text = "Gained <color=green>" + enemy.droppedXp + "</color> xp";
            if (leveled)
            {
                damageText.text += " ALSO u leveld up (this is not a permanent message)";
            }

        }

        while (damageText.transform.parent.gameObject.activeSelf)
        {
            yield return null;
        }
        fighter1.transform.localScale = new Vector3(100, 100, 1);
        

        MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy = null;

        NetworkData.Instance.setNextTurnNum();

        SceneChanger.Instance.loadClientScenesServerRpc("MainGameScene");
    }
    private IEnumerator playerDefeatDisplay(int winner, int gainedxp)
    {
        playerData win = null;
        playerData loser;
        if (winner == 0 )
        {
            win = PlayerCombatManager.Instance.combatant1 as playerData;
            loser = PlayerCombatManager.Instance.combatant2 as playerData;
        
        }
        else
        {
            loser = PlayerCombatManager.Instance.combatant1 as playerData;
            if (PlayerCombatManager.Instance.combatant2 is playerData)
            {
                win = PlayerCombatManager.Instance.combatant2 as playerData;
            }
                
            else
                displayXp = false;
        }
        damageText.text = loser.LoseSomething();
        loser.death();
        Debug.Log(loser.name + " should be dead");


        while (damageText.transform.parent.gameObject.activeSelf)
        {

            yield return null;
        }

        
        if (displayXp && win != null)
        {
            displayXp = false;
            damageText.transform.parent.gameObject.SetActive(true);
            bool leveled = win.gainXp(gainedxp);
            damageText.text = win.name + " Gained <color=green>" + gainedxp + "</color> xp";
            if( leveled )
            {
                damageText.text += " ALSO u leveld up (this is not a permanent message)";
            }

        }

        while (damageText.transform.parent.gameObject.activeSelf)
        {
            yield return null;
        }
        fighter1.transform.localScale = new Vector3(100, 100, 1);
        if (PlayerCombatManager.Instance.combatant2 is playerData) fighter2.transform.localScale = new Vector3(100, 100, 1);


        MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy = null;

        NetworkData.Instance.setNextTurnNum();

        SceneChanger.Instance.loadClientScenesServerRpc("MainGameScene");
    }
    private int zeroMinimum(int numToCheck)
    {
        if (numToCheck < 0) 
            return 0;
        else 
            return numToCheck;
    }
}
