using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewCombatManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;


    public List<GameObject> fricku = new List<GameObject>();
    public List<AbilityManager> allCombatants = new List<AbilityManager>();
    // Start is called before the first frame update

    public static NewCombatManager instance;

    private int xpHarvested;
    private int moneyHarvested;
    private List<ItemBase> itemsPicked = new List<ItemBase>();


    [DoNotSerialize] public bool fightOver = false;

    public List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();
    private int currentSpec = 0;

    public DialogueScript endBattleInfo;

    public PlayerInput playercontrol;

    
    public override void OnNetworkSpawn()
    {
        

        if (instance != null) { return;  }
            

        instance = this;
        if (SceneChanger.Instance.everyoneLoaded())
        {
                
                //b UT WHY DOES IT SPAWN DOUBLE IF A CLIENT HASNT LOADED IN YET BC ONLY THE SERVER SHOULDVE BEEN ALLOWED TO SPAWNS STUFF IN AND THE HOST AND SERVER AR ETHE SAME HOW DOES THAT EVEN MAKE SENSE SMD FRICK U EMA I AHTE U LMB EXPLODE
                //shoutout to onloadcomplete tho brother fixed that stupid problem
                SetUp();
    
            
            base.OnNetworkSpawn();

        }
            
        
         
    }
    

    private void SetUp()
    {
        if (!IsSpawned) { return; }
        SetUpRpc();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SetUpRpc()
    {
        
        
        int countbcisuck = 1;
        for (int i = 0; i < PlayerCombatManager.Instance.combatants.Count; i ++)
        {

            var entity = PlayerCombatManager.Instance.combatants[i];
           
            if (entity is playerData)
            {
                
                var player = entity as playerData;
                var playerfab = NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, (ulong)player.playerNumber, true);
           

                //var editor = NetworkData.Instance.playerSticks[player.playerNumber].GetComponent<characterEditor>();
                //editor.UpdateMaterial();
                /*playerfab.GetComponentInChildren<MeshRenderer>().material = editor.myMaterial;*/
               
                var abilitiyManage = playerfab.GetComponent<AbilityManager>();
                abilitiyManage.UpdateMaterialRpc(player.playerNumber);

                abilitiyManage.UpdateStatsRpc(i);
                SetNotSpectateRpc(countbcisuck,RpcTarget.Single((ulong)player.playerNumber, RpcTargetUse.Temp));
                Debug.Log("Number of cameras(in loop)" + cameras.Count);

               
                abilitiyManage.AssignAbilities();
                Debug.Log("PlayerSpawn: " + playerfab.GetComponent<NetworkObject>().NetworkObjectId);
                countbcisuck++;
            }
            else
            {
                var npc = entity as EnemyCombat;
                var npcfab = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[npc.enemyId].enemyPrefab);
                npcfab.GetComponent<NetworkObject>().Spawn(true);

                var abilitiyManage = npcfab.GetComponent<AbilityManager>();
                abilitiyManage.UpdateStatsRpc(i);
                
                
                Debug.Log("Enemy Spawn: " + npcfab.GetComponent<NetworkObject>().NetworkObjectId);

            }
            
        }
        Debug.Log("Number of cameras When done" + cameras.Count);
      
    }
    [Rpc(SendTo.SpecifiedInParams, RequireOwnership = false)]
    private void SetNotSpectateRpc(int whichone,RpcParams rpcStuff)
    {
        Cursor.lockState = CursorLockMode.Locked;
        playercontrol.SwitchCurrentActionMap("Player");
        cameras[0].Priority = 1;
        cameras[whichone].Priority = 10;

    }

    public void KILL(AbilityManager whoded)
    {
        fricku.Remove(whoded.gameObject);
        allCombatants.Remove(whoded);
        Debug.Log("wHO IS dead " + whoded.gameObject.name);
        if(whoded.stats is EnemyCombat)
        {
            EnemyCombat info = (EnemyCombat)whoded.stats;

            xpHarvested += PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[info.enemyId].droppedXp;
            moneyHarvested += PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[info.enemyId].droppedMoney;

            if (IsServer) 
            {
                int dropnum = PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[info.enemyId].rollItem();
                if (dropnum >= 0) { ItemDroppedRpc(dropnum, info.enemyId); }
                
                Destroy(whoded.gameObject);
            }

          
            
        }
        if (IsServer && CheckWin())
        {
            SetUpVictorRpc();
        }

    }
    


    private bool CheckWin()
    {
        bool didWin = true;
        

        for(int i = 0 ; i < allCombatants.Count; i++)
        {

            for (int j = 0 ; j < allCombatants.Count ; j++)
            {
                if (!allCombatants[i].stats.loyaltyTags.Intersect(allCombatants[j].stats.loyaltyTags).Any())
                {
                    didWin = false;
                    return didWin;
                }

       
            }
        }
        Debug.Log("THIS FIGHT IS OVER");
        return didWin;
    }

    private AbilityManager WhoWon()
    {
        AbilityManager enemy = allCombatants[0];

        for(int i = 0; i < allCombatants.Count; i++)
        {
            enemy = allCombatants[i];
            if (enemy.stats is playerData)
            {
                return enemy;
            }
            
        }

        return enemy;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ItemDroppedRpc(int item, int enemyId)
    {
        Debug.Log("ITEMDROPPED");
        itemsPicked.Add(PlayerCombatManager.Instance.EnemyDataBase.Enemies[enemyId].DroppedItems[item]);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetUpVictorRpc()
    {
        AbilityManager victor = WhoWon();
        Debug.Log("I AM THE WINNER " + victor.stats.name);
        playercontrol.SwitchCurrentActionMap("UI");
        fightOver = true;
        Cursor.lockState = CursorLockMode.None;
       
        if (victor.stats is playerData)
        {
            playerData player = (playerData)victor.stats;
            int levels = player.gainXp(xpHarvested);
            player.playerInfo["money"] += moneyHarvested;
            foreach(var item in  itemsPicked)
            {
                NetworkData.Instance.AddItemToInventory(player.playerNumber, item);
            }
            endBattleInfo.lines.Clear();
            endBattleInfo.lines.Add(player.name + " has gained <color=blue>" + xpHarvested + "</color> xp ");
            if(levels >  0)
            {
                endBattleInfo.lines[0] += " and they've leveled up " + levels + " times";
            }

            endBattleInfo.lines.Add(player.name + " has gained " + player.playerInfo["money"] + " money");

            if (itemsPicked.Count > 0)
            {
                string itemString = "You've picked up ";
                for(int i = 0; i < itemsPicked.Count; i++)
                {
                    if(i == itemsPicked.Count - 1 && itemsPicked.Count > 1)
                    {
                        itemString += " and a " + itemsPicked[i].name;

                    }
                    else
                    {
                        itemString += " a " + itemsPicked[i].name;
                        if (i != itemsPicked.Count - 1) { itemString += ", "; }
                    }
                }
                endBattleInfo.lines.Add(itemString);
            }
            Debug.Log("SHOULD BE ACTIVE");
            endBattleInfo.gameObject.SetActive(true);
            endBattleInfo.Awake();
            endBattleInfo.whoInControl = player.playerNumber;
            

        }
        NetworkData.Instance.setNextTurnNum();
        endBattleInfo.gameObject.GetComponentInChildren<Button>().Select();
      
        Debug.Log("It should be player " + NetworkData.Instance.currentPlayer);



    }
    public void RightSpec()
    {
        cameras[currentSpec].Priority = 1;
        if (currentSpec < cameras.Count - 1)
        {
            currentSpec += 1;
            
        }
        else
        {
            currentSpec = 0;
        }
        cameras[currentSpec].Priority = 10;
    }
    public void LeftSpec()
    {
        cameras[currentSpec].Priority = 1;
        if (currentSpec > 0)
        {
            currentSpec -= 1;
        }
        else
        {
            currentSpec = cameras.Count - 1;
        }
        cameras[currentSpec].Priority = 10;
    }
}
