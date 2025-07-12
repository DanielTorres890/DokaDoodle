using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewCombatManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;


    [DoNotSerialize] public List<GameObject> fricku = new List<GameObject>();
    [DoNotSerialize] public List<AbilityManager> allCombatants = new List<AbilityManager>();

    public playerLevelUpMnger levelUpUI;
    // Start is called before the first frame update

    [DoNotSerialize] public static NewCombatManager instance;

    [SerializeField] private PlayerUIManager playerUI;
    [SerializeField] private StatUIDisplay statUI;

    private int xpHarvested;
    private int moneyHarvested;
    private List<ItemBase> itemsPicked = new List<ItemBase>();

    public float combatTimer = 30;
    [SerializeField] private TextMeshProUGUI timerText;

    [DoNotSerialize] public bool fightOver = false;
    

    public List<CinemachineCamera> cameras = new List<CinemachineCamera>();
    [DoNotSerialize]  public int currentSpec = 0;


    [Tooltip("Center spawn point where all units will spawn around")][SerializeField] private Vector3 spawnPoint;
    [Tooltip("How far enemies will be staggered from each other")][SerializeField] private float zDistanceBetween;
    [Tooltip("How far enemies spawn from the spawn point")][SerializeField] private float distanceFromCenter;

    private bool alreadyDone = false;
    private float statusTick = 0;

    public DialogueScript endBattleInfo;

    public PlayerInput playercontrol;

    public UnityEvent onCombatEnd;
    private AudioSource AudioSource;
    private void Awake()
    {
       AudioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        
        combatTimer -= Time.deltaTime;

        statusTick += Time.deltaTime;

        timerText.text = Mathf.RoundToInt(combatTimer).ToString();
        if (!IsServer) { return; }

        if (!fightOver && statusTick >= 10)//Progress status effects every 10 seconds
        {
            TickCombatantStatusRpc();
            statusTick = 0;
        }
        if (!fightOver && combatTimer <= 0)//end fight
        {
            EarlyEndCombatRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (instance != null) { return; }

        Debug.Log("did me get instantiated");
        instance = this;
        
        foreach (var camera in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
        {
            cameras.Add(camera);
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("NewBattleArea"));
        
        //bc im dumb and didnt handle things earlier
        for (int i = 0; i < NetworkData.Instance.playerSticks.Count; i++)
        {
            NetworkData.Instance.playerSticks[i].transform.position = new Vector3(-1000 + i * 1000, -1000, -1000);
        }
        endBattleInfo.lines.Clear();

        
        if (IsHost)
        {
            //b UT WHY DOES IT SPAWN DOUBLE IF A CLIENT HASNT LOADED IN YET BC ONLY THE SERVER SHOULDVE BEEN ALLOWED TO SPAWNS STUFF IN AND THE HOST AND SERVER AR ETHE SAME HOW DOES THAT EVEN MAKE SENSE SMD FRICK U EMA I AHTE U LMB EXPLODE
            //shoutout to onloadcomplete tho brother fixed that stupid problem
            //also shoutout to ben bc lowkey what he said makes sense when each client loads in they spawn their own version of the network object which overrides previous ones (?) tho thats still a weird ahh thing
            StartCoroutine(WaitForLoadIn());
        }
         
    }
    
    private IEnumerator WaitForLoadIn()
    {
        while(!SceneChanger.Instance.everyoneLoaded())
        {
            yield return null;
        }
        SetUp();
    }

    private void SetUp()
    {
        if (!IsSpawned) { return; }
        SetUpRpc();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SetUpRpc()
    {
        if(alreadyDone) { return; }
        alreadyDone = true;
    
        int countbcisuck = 1;
        int sideMult = -1;

        Dictionary<string, List<EntityStats>> spawnGroups = new Dictionary<string, List<EntityStats>>();
        foreach(var combatant in PlayerCombatManager.Instance.combatants)
        {
            Debug.Log("This guy is in " + combatant.name);
            if (!spawnGroups.ContainsKey(combatant.loyaltyTags[0]))
            {
                spawnGroups.Add(combatant.loyaltyTags[0], new List<EntityStats>());
                
            }
            
            spawnGroups[combatant.loyaltyTags[0]].Add(combatant);
        }
        int counter = 0;
        float circleIncrement = 360f / spawnGroups.Keys.Count;
       
        circleIncrement = circleIncrement / 180 * Mathf.PI;
        foreach(var ctag in spawnGroups.Keys)
        {

            for (int i = 0; i < spawnGroups[ctag].Count; i++)
            {
                
                var entity = spawnGroups[ctag][i];

                if (entity is playerData)
                {

                    var player = entity as playerData;
                    var playerfab = Instantiate(playerPrefab);
                    playerfab.gameObject.transform.position = new Vector3(spawnPoint.x + Mathf.Cos(circleIncrement*counter) * distanceFromCenter, spawnPoint.y, sideMult * spawnPoint.z + (i * zDistanceBetween)  + (Mathf.Sin(circleIncrement * counter) * distanceFromCenter));

                    playerfab.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)player.playerNumber, true);


                    var abilitiyManage = playerfab.GetComponent<AbilityManager>();
                    abilitiyManage.UpdateMaterialRpc(player.playerNumber);

                    abilitiyManage.UpdateStatsRpc(PlayerCombatManager.Instance.combatants.IndexOf(entity));
                    SetNotSpectateRpc(countbcisuck, RpcTarget.Single((ulong)player.playerNumber, RpcTargetUse.Temp));

                    countbcisuck++;
                }
                else
                {
                    var npc = entity as EnemyCombat;
                    var npcfab = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetItem[npc.enemyId].enemyPrefab);
                    npcfab.transform.position = new Vector3(spawnPoint.x + Mathf.Cos(circleIncrement * counter) * distanceFromCenter, spawnPoint.y, sideMult * spawnPoint.z + (i * zDistanceBetween) + (Mathf.Sin(circleIncrement * counter) * distanceFromCenter));

                    npcfab.GetComponent<NetworkObject>().Spawn(true);

                    var abilitiyManage = npcfab.GetComponent<AbilityManager>();
                    abilitiyManage.UpdateStatsRpc(PlayerCombatManager.Instance.combatants.IndexOf(entity));


                   

                }
            }
            counter++;
        }
        for (int i = 0; i < PlayerCombatManager.Instance.combatants.Count; i ++)
        {
            
            
        }
        
      
    }
    [Rpc(SendTo.SpecifiedInParams, RequireOwnership = false)]
    private void SetNotSpectateRpc(int whichone,RpcParams rpcStuff)
    {
        //fricku[whichone].GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        playercontrol.SwitchCurrentActionMap("Player");
        cameras[0].Priority = 1;
        currentSpec = whichone;
        cameras[whichone].Priority = 10;
        
        playerUI.abilityManager = allCombatants[whichone-1]; //keep in mind that theres already a camera in the scene by default so its off by 1
        playerUI.SetUp();

        statUI.abilityManager = allCombatants[whichone - 1];
        statUI.SetUp();

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void TickCombatantStatusRpc()
    {
        foreach(var combat in allCombatants)
        {
            combat.stats.ProgressStatuses();
            combat.onStatus.Invoke();
            
        }

    }

    public void KILL(AbilityManager whoded)
    {
        fricku.Remove(whoded.gameObject);
        Debug.Log("wHO IS dead " + whoded.gameObject.name);
        if(whoded.stats is EnemyCombat)
        {
            EnemyCombat info = (EnemyCombat)whoded.stats;
            info.isDead = true;
            xpHarvested += PlayerCombatManager.Instance.EnemyDataBase.GetItem[info.enemyId].droppedXp;
            moneyHarvested += PlayerCombatManager.Instance.EnemyDataBase.GetItem[info.enemyId].droppedMoney;

            if (IsServer) 
            {
                int dropnum = PlayerCombatManager.Instance.EnemyDataBase.GetItem[info.enemyId].rollItem();
                if (dropnum >= 0) { ItemDroppedRpc(dropnum, info.enemyId); }
                
                Destroy(whoded.gameObject);
            }           
        }
        if (whoded.stats is playerData)
        {
            playerData info = (playerData)whoded.stats;
            info.isDead = true;
            Debug.Log(info.name + " did i die: " +info.isDead);
            
            if(whoded.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId) 
            {
                playercontrol.SwitchCurrentActionMap("Spectating");
                cameras[currentSpec].Priority = 1;
                cameras[0].Priority = 10;
                currentSpec = 0;
                
            }
          
                


            if (IsServer) { LinesToSyncRpc((info.name + " dropped " + info.playerInfo[PlayerInfo.money] / 2 + " moneys"), info.LoseSomething(), 3, info.playerNumber); }

            moneyHarvested = info.playerInfo[PlayerInfo.money] /= 2; 
            info.playerInfo[PlayerInfo.money] /= 2;
                
            

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
                    
                    didWin = false;
                    return didWin;
                }

       
            }
        }
        
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
        itemsPicked.Add(PlayerCombatManager.Instance.EnemyDataBase.GetItem[enemyId].DroppedItems[item]);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetUpVictorRpc()
    {
        AbilityManager victor = WhoWon();
        Debug.Log("I AM THE WINNER " + victor.stats.name);
        onCombatEnd.Invoke();
        playercontrol.SwitchCurrentActionMap("UI");
        fightOver = true;
        Cursor.lockState = CursorLockMode.None;
        var cache = MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];

        cache.tileEnemy.Clear();
        

        if (victor.stats is playerData)
        {
            playerData player = (playerData)victor.stats;
            int levels = player.gainXp(xpHarvested + cache.xpOnTile);
            player.playerInfo[PlayerInfo.money] += moneyHarvested + cache.moneyOnTile;
            foreach(var item in  itemsPicked)
            {
                NetworkData.Instance.AddItemToInventory(player.playerNumber, item);
            }
            
            endBattleInfo.lines.Add(player.name + " has gained <color=blue>" + (xpHarvested + cache.xpOnTile) + "</color> xp ");
            if(levels >  0)
            {
                endBattleInfo.lines[endBattleInfo.lines.Count-1] += " and they've leveled up " + levels + " times";
                endBattleInfo.endEvent.RemoveAllListeners();
                levelUpUI.statsToAllocate += levels * 3;
                levelUpUI.inControl = player.playerNumber;
                levelUpUI.playerWhoLevel = player;
                endBattleInfo.endEvent.AddListener(delegate { levelUpUI.Setup(); });
                
            }

            endBattleInfo.lines.Add(player.name + " has gained " + (moneyHarvested + cache.moneyOnTile) + " money");

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
            
       
            endBattleInfo.gameObject.SetActive(true);
            endBattleInfo.startDialogue();
            endBattleInfo.whoInControl = player.playerNumber;
            

        }
        else
        {
            
            endBattleInfo.lines.Add("Every player has been defeated");
            foreach(var combat in allCombatants)
            {
                if (combat.stats is playerData)
                {
                    var current = combat.stats as playerData;
                    
                    
       
                    if (IsServer) { LinesToSyncRpc((current.name + " dropped " + current.playerInfo[PlayerInfo.money] / 2 + " moneys"), current.LoseSomething(), 3, current.playerNumber); }
                    
                    current.playerInfo[PlayerInfo.money] /= 2;
                }
            }
            endBattleInfo.gameObject.SetActive(true);
            endBattleInfo.startDialogue();
            endBattleInfo.whoInControl = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber;

            cache.players.Clear();
            

        }
        
        PlayerCombatManager.Instance.combatants.Clear();
        RemoveDeadEntities();
        NetworkData.Instance.setNextTurnNum();
        endBattleInfo.gameObject.GetComponentInChildren<Button>().Select();
      
        Debug.Log("It should be player " + NetworkData.Instance.currentPlayer);

        cache.xpOnTile = 0;
        cache.moneyOnTile = 0;

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void EarlyEndCombatRpc()
    {
        playercontrol.SwitchCurrentActionMap("UI");
        onCombatEnd.Invoke();
        fightOver = true;
        Cursor.lockState = CursorLockMode.None;
        endBattleInfo.whoInControl = NetworkData.Instance.currentPlayer;

        endBattleInfo.lines.Add("NEXT TIME ON DRAGON BALL Z");
        endBattleInfo.gameObject.SetActive(true);
        endBattleInfo.startDialogue();
        RemoveDeadEntities();
                    
        NetworkData.Instance.setNextTurnNum();
        
        endBattleInfo.gameObject.GetComponentInChildren<Button>().Select();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LinesToSyncRpc(string lostmoney, string lostitem, int turnsDead, int playerNumber)
    {
        endBattleInfo.lines.Add(lostmoney);
        endBattleInfo.lines.Add(lostitem);
        Debug.Log("did u died?");
        //NetworkData.Instance.players[playerNumber].death(turnsDead, false);
    }

    private void RemoveDeadEntities() //Removes them from the database that stores all enemy info (it probably shouldn't be accessible all the time but fml
    {
        var tilereadCache = MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];

        tilereadCache.xpOnTile = xpHarvested;
        tilereadCache.moneyOnTile = moneyHarvested;
        for(int i = tilereadCache.tileEnemy.Count - 1; i >= 0; i--)
        {
            
            if (tilereadCache.tileEnemy[i].isDead)
            {
                tilereadCache.tileEnemy.RemoveAt(i);
            }
        }
        
        for (int i = tilereadCache.players.Count - 1; i >= 0; i--)
        {
            
            NetworkData.Instance.players[tilereadCache.players[i]].ClearCombatStatuses();
            if (NetworkData.Instance.players[tilereadCache.players[i]].isDead)
            {
                Debug.Log("I should be dead " + NetworkData.Instance.players[tilereadCache.players[i]].name);
                NetworkData.Instance.players[tilereadCache.players[i]].death(3);
                
            }
        }
     
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
