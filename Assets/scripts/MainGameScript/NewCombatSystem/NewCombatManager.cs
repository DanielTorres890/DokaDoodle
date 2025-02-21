using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
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
    

    public List<CinemachineCamera> cameras = new List<CinemachineCamera>();
    private int currentSpec = 0;

    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private float zDistanceBetween;

    public DialogueScript endBattleInfo;

    public PlayerInput playercontrol;

    
    public override void OnNetworkSpawn()
    {
        //bc im dumb and didnt handle things earlier
        for (int i = 0; i < NetworkData.Instance.playerSticks.Count; i++)
        {
            NetworkData.Instance.playerSticks[i].transform.position = new Vector3(-1000 + i * 1000, -1000, -1000);
        }


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
        int sideMult = -1;
        for (int i = 0; i < PlayerCombatManager.Instance.combatants.Count; i ++)
        {
            sideMult *= -1;
            var entity = PlayerCombatManager.Instance.combatants[i];
           
            if (entity is playerData)
            {
                
                var player = entity as playerData;
                var playerfab = Instantiate(playerPrefab);
                playerfab.gameObject.transform.position = new Vector3(spawnPoint.x * sideMult, spawnPoint.y, sideMult * spawnPoint.z + i * zDistanceBetween * -sideMult);
                playerfab.GetComponent<NetworkObject>().SpawnWithOwnership( (ulong)player.playerNumber, true);
                //var editor = NetworkData.Instance.playerSticks[player.playerNumber].GetComponent<characterEditor>();
                //editor.UpdateMaterial();
                /*playerfab.GetComponentInChildren<MeshRenderer>().material = editor.myMaterial;*/
               
                var abilitiyManage = playerfab.GetComponent<AbilityManager>();
                abilitiyManage.UpdateMaterialRpc(player.playerNumber);

                abilitiyManage.UpdateStatsRpc(i);
                SetNotSpectateRpc(countbcisuck,RpcTarget.Single((ulong)player.playerNumber, RpcTargetUse.Temp));
                Debug.Log("Number of cameras(in loop)" + cameras.Count);

            
                Debug.Log("PlayerSpawn: " + playerfab.GetComponent<NetworkObject>().NetworkObjectId);
                countbcisuck++;
            }
            else
            {
                var npc = entity as EnemyCombat;
                var npcfab = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[npc.enemyId].enemyPrefab);
                npcfab.transform.position = new Vector3(spawnPoint.x * sideMult, spawnPoint.y, sideMult * spawnPoint.z + i * zDistanceBetween * -sideMult);

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
        //fricku[whichone].GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        playercontrol.SwitchCurrentActionMap("Player");
        cameras[0].Priority = 1;
        cameras[whichone].Priority = 10;

    }

    public void KILL(AbilityManager whoded)
    {
        fricku.Remove(whoded.gameObject);
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
            if (allCombatants[i].stats.isDead) { continue; }

            for (int j = 0 ; j < allCombatants.Count ; j++)
            {
                if (allCombatants[j].stats.isDead) { continue; }

                if (!allCombatants[i].stats.loyaltyTags.Intersect(allCombatants[j].stats.loyaltyTags).Any())
                {
                    Debug.Log(allCombatants[i].stats.name);
                    Debug.Log("excuse me tf " + allCombatants[i].stats.loyaltyTags.Count);
                    Debug.Log("nah u lying " + allCombatants[j].stats.loyaltyTags[0]);
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
            
            
            if (enemy.stats.isDead) { enemy = allCombatants[i]; }
            if (!(enemy.stats is playerData) && !enemy.stats.isDead)
            {
                enemy = allCombatants[i];
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
            foreach (var combat in allCombatants)
            {
                if (combat.stats is playerData && combat.stats.isDead)
                {
                    var current = combat.stats as playerData;



                    if (IsServer) { LinesToSyncRpc((current.name + " dropped " + current.playerInfo["money"] / 2 + " moneys"), current.LoseSomething(), 3, current.playerNumber); }

                    current.playerInfo["money"] /= 2;
                }
            }
            Debug.Log("SHOULD BE ACTIVE");
            endBattleInfo.gameObject.SetActive(true);
            endBattleInfo.Awake();
            endBattleInfo.whoInControl = player.playerNumber;
            

        }
        else
        {
            endBattleInfo.lines.Clear();
            endBattleInfo.lines.Add("Every player has been defeated");
            foreach(var combat in allCombatants)
            {
                if (combat.stats is playerData)
                {
                    var current = combat.stats as playerData;
                    
                    
       
                    if (IsServer) { LinesToSyncRpc((current.name + " dropped " + current.playerInfo["money"] / 2 + " moneys"), current.LoseSomething(), 3, current.playerNumber); }
                    
                    current.playerInfo["money"] /= 2;
                }
            }
            endBattleInfo.gameObject.SetActive(true);
            endBattleInfo.Awake();
            endBattleInfo.whoInControl = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber;

        }
        MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy.Clear();
        NetworkData.Instance.setNextTurnNum();
        endBattleInfo.gameObject.GetComponentInChildren<Button>().Select();
      
        Debug.Log("It should be player " + NetworkData.Instance.currentPlayer);



    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LinesToSyncRpc(string lostmoney, string lostitem, int turnsDead, int playerNumber)
    {
        endBattleInfo.lines.Add(lostmoney);
        endBattleInfo.lines.Add(lostitem);
        Debug.Log("did u died?");
        NetworkData.Instance.players[playerNumber].death(turnsDead);
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
