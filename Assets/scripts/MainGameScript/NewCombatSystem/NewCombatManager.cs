using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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
    [DoNotSerialize] public int currentSpec = 0;


    [Tooltip("Center spawn point where all units will spawn around")]
    [SerializeField] private Vector3 spawnPoint;

    [Tooltip("How far enemies will be staggered from each other")]
    [SerializeField] private float zDistanceBetween;

    [Tooltip("How far enemies spawn from the spawn point")]
    [SerializeField] private float distanceFromCenter;

    [Tooltip("The maximum amount you can zoom in and out")]
    [SerializeField] private float maxSpecZoomIn, maxSpecZoomOut;
    [SerializeField] private float scrollSpeed = 5.0f, mouseSpeed = 5.0f;
    [SerializeField] private float minXCam = 5.0f, maxXCam = 5.0f;
    private Vector2 scrolling, mouseMove;
    private bool alreadyDone = false;

    public DialogueScript endBattleInfo;

    public PlayerInput playercontrol;

    public PVPVictory pvpVictory;
    public LoseItemManager dropItem;

    public UnityEvent onCombatEnd;
    private AudioSource AudioSource;


    public GameObject spectateUI;
    public GameObject inCombatUI;
    public DashCdDisplay dashCdDisplay;

    public List<AllyAIWrapper> allyPrefabs;
   
    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
        Cursor.lockState = CursorLockMode.Locked;
        instance = this;



    }
    private void Update()
    {

        combatTimer -= Time.deltaTime;


        timerText.text = "Time Remaining: " + Mathf.RoundToInt(combatTimer).ToString();

        
        if (!IsServer) { return; }

        //lowkey im braindamaged why would i put this here
        if (!fightOver && combatTimer <= 0)//end fight
        {
            EarlyEndCombatRpc();
        }
    }

    public override void OnNetworkSpawn()
    {
        

        foreach (var camera in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
        {
            cameras.Add(camera);
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("NewBattleArea"));
        

        StartBGM();

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
        
        while (!SceneChanger.Instance.everyoneLoaded())
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
        if (alreadyDone) { return; }
        alreadyDone = true;
        Debug.Log("Im spawning in entities now");
        int countbcisuck = 1;
        int sideMult = -1;

        Dictionary<string, List<EntityStats>> spawnGroups = new Dictionary<string, List<EntityStats>>();
        foreach (var combatant in PlayerCombatManager.Instance.combatants)
        {
            
            if (!spawnGroups.ContainsKey(combatant.loyaltyTags[0]))
            {
                spawnGroups.Add(combatant.loyaltyTags[0], new List<EntityStats>());

            }

            spawnGroups[combatant.loyaltyTags[0]].Add(combatant);
        }
        int counter = 0;
        float circleIncrement = 360f / spawnGroups.Keys.Count;

        circleIncrement = circleIncrement / 180 * Mathf.PI;
        foreach (var ctag in spawnGroups.Keys)
        {

            for (int i = 0; i < spawnGroups[ctag].Count; i++)
            {

                var entity = spawnGroups[ctag][i];

                if (entity is playerData)
                {

                    var player = entity as playerData;
                    var playerfab = Instantiate(playerPrefab);
                    playerfab.gameObject.transform.position = new Vector3(spawnPoint.x + Mathf.Cos(circleIncrement * counter) * distanceFromCenter, spawnPoint.y, sideMult * spawnPoint.z + (i * zDistanceBetween) + (Mathf.Sin(circleIncrement * counter) * distanceFromCenter));
                    playerfab.transform.LookAt(spawnPoint);
                    playerfab.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)player.playerNumber, true);


                    var abilitiyManage = playerfab.GetComponent<AbilityManager>();
                    abilitiyManage.UpdateMaterialRpc(player.playerNumber);

                    abilitiyManage.UpdateStatsRpc(PlayerCombatManager.Instance.combatants.IndexOf(entity));
                    SetNotSpectateRpc(countbcisuck, RpcTarget.Single((ulong)player.playerNumber, RpcTargetUse.Temp));

                    
                }
                else if(entity is EnemyCombat)
                {
                    var npc = entity as EnemyCombat;
                    var npcfab = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetItem[npc.enemyId].enemyPrefab);
                    npcfab.transform.position = new Vector3(spawnPoint.x + Mathf.Cos(circleIncrement * counter) * distanceFromCenter, spawnPoint.y, sideMult * spawnPoint.z + (i * zDistanceBetween) + (Mathf.Sin(circleIncrement * counter) * distanceFromCenter));

                    npcfab.GetComponent<NetworkObject>().Spawn(true);

                    var abilitiyManage = npcfab.GetComponent<AbilityManager>();
                    
                    abilitiyManage.UpdateStatsRpc(PlayerCombatManager.Instance.combatants.IndexOf(entity));
                    



                }
                else if(entity is PartyMember)
                {
                    var ally = entity as PartyMember;
                    GameObject allyfab = allyPrefabs[0].prefab;
                    foreach(var prefabs in allyPrefabs)
                    {
                        if(prefabs.type != NetworkData.Instance.classDataBase.GetItem[ally.allyClass].AIType) { continue; }
                        allyfab = Instantiate(prefabs.prefab);

                    }
                    allyfab.transform.position = new Vector3(spawnPoint.x + Mathf.Cos(circleIncrement * counter) * distanceFromCenter, spawnPoint.y, sideMult * spawnPoint.z + (i * zDistanceBetween) + (Mathf.Sin(circleIncrement * counter) * distanceFromCenter));

                    allyfab.GetComponent<NetworkObject>().Spawn(true);

                    
                    var abilitiyManage = allyfab.GetComponent<AbilityManager>();
                    abilitiyManage.UpdateStatsRpc(PlayerCombatManager.Instance.combatants.IndexOf(entity));
                    abilitiyManage.UpdateMyLooksRpc();


                }
                countbcisuck++;
            }
            counter++;
        }
        for (int i = 0; i < PlayerCombatManager.Instance.combatants.Count; i++)
        {


        }


    }
    [Rpc(SendTo.SpecifiedInParams, RequireOwnership = false)]
    private void SetNotSpectateRpc(int whichone, RpcParams rpcStuff)
    {
        //fricku[whichone].GetComponent<PlayerInput>();
      
        Cursor.lockState = CursorLockMode.Locked;
        playercontrol.SwitchCurrentActionMap("Player");
        spectateUI.SetActive(false);
        inCombatUI.SetActive(true);
        cameras[0].Priority = 1;
        currentSpec = whichone;
        cameras[whichone].Priority = 10;


        Debug.Log("which one did i spawn in? " + whichone);
        Debug.Log("but but this dont make sense? " + allCombatants.Count);
        Debug.Log("okay so technically it could be that they're spawning in the wrong one? " + allCombatants[whichone - 1].stats.name);
        playerUI.abilityManager = allCombatants[whichone - 1]; //keep in mind that theres already a camera in the scene by default so its off by 1
        playerUI.SetUp();

        statUI.abilityManager = allCombatants[whichone - 1];
        statUI.SetUp();


        dashCdDisplay.manager = allCombatants[whichone - 1];
        dashCdDisplay.playerMovement = allCombatants[whichone - 1].gameObject.GetComponent<CombatantMovement>();

        dashCdDisplay.playerMovement.SetUp();
        dashCdDisplay.SetUp();
        
    }

    public void KILL(AbilityManager whoded)
    {
        fricku.Remove(whoded.gameObject);
        if (whoded.stats is EnemyCombat)
        {
            EnemyCombat info = (EnemyCombat)whoded.stats;
            info.isDead = true;
            xpHarvested += PlayerCombatManager.Instance.EnemyDataBase.GetItem[info.enemyId].droppedXp;
            moneyHarvested += PlayerCombatManager.Instance.EnemyDataBase.GetItem[info.enemyId].droppedMoney;

            if (IsServer)
            {
                int dropnum = PlayerCombatManager.Instance.EnemyDataBase.GetItem[info.enemyId].rollItem();
                if (dropnum >= 0) { ItemDroppedRpc(dropnum, info.enemyId); }

                
            }
            var rigid = whoded.GetComponent<Rigidbody>();
            rigid.constraints = RigidbodyConstraints.None;
            rigid.isKinematic = false;
            rigid.AddForce(rigid.transform.TransformDirection(Vector3.back) * 10, ForceMode.Impulse);
        }
        if (whoded.stats is playerData)
        {
            playerData info = (playerData)whoded.stats;
            info.isDead = true;
            whoded.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            whoded.GetComponent<Rigidbody>().AddForce(whoded.transform.TransformDirection(Vector3.back) * 10, ForceMode.Impulse);
            Debug.Log(info.name + " did i die: " + info.isDead);

            if (whoded.gameObject.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                spectateUI.SetActive(true);
                inCombatUI.SetActive(false);
                playercontrol.SwitchCurrentActionMap("Spectating");

                cameras[currentSpec].Priority = 1;
                cameras[0].Priority = 10;
                currentSpec = 0;

            }

        }

        if (whoded.stats is PartyMember)
        {
            PartyMember info = (PartyMember)whoded.stats;
            info.isDead = true;
            xpHarvested += info.allyInfo[PlayerInfo.xp];
            whoded.GetComponent<NavMeshAgent>().enabled = false;


            var rigid = whoded.GetComponent<Rigidbody>();
            rigid.constraints = RigidbodyConstraints.None;
            rigid.isKinematic = false;
            rigid.AddForce(rigid.transform.TransformDirection(Vector3.back) * 10, ForceMode.Impulse);

        }

        CleanUpCams();

        if (IsServer && CheckWin())
        {
            SetUpVictorRpc();
        }

    }



    private bool CheckWin()
    {
        bool didWin = true;


        for (int i = 0; i < allCombatants.Count; i++)
        {
            if (allCombatants[i].stats.isDead) { continue; }

            for (int j = 0; j < allCombatants.Count; j++)
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

        for (int i = 0; i < allCombatants.Count; i++)
        {

           
            if (enemy.stats.isDead) { enemy = allCombatants[i]; }
            if (!allCombatants[i].stats.isDead)
            {
                Debug.Log("Im not dead and i could be the winner " + enemy.name);
                enemy = allCombatants[i];
            }

        }

        return enemy;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ItemDroppedRpc(int item, int enemyId)
    {
      
        itemsPicked.Add(PlayerCombatManager.Instance.EnemyDataBase.GetItem[enemyId].DroppedItems[item]);
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PlayerItemDroppedRpc(int item, int itemType)
    {
        itemsPicked.Add(NetworkData.Instance.playerInventories[0][itemType].database.GetItem[item]);
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
        for(int i = cache.tileEnemy.Count - 1; i >= 0; i--)
        {
            if (!cache.tileEnemy[i].persistant)
            {
                cache.tileEnemy.RemoveAt(i);
            }
        }


        //if player win any dead allies should survive at 1 and vice versa
        victor = SaveEntities(victor);



        List<int> deadPlayers = RemoveDeadEntities();
        if (victor.stats is playerData)
        {
            playerData player = (playerData)victor.stats;

            List<int> partyLevelsGained = new List<int>();
            //its implied that if combat ends the only ones left would be your allies
            foreach(var partyMember in cache.partyMembers)
            {
                int levelsGained = partyMember.gainXp(cache.xpOnTile / cache.partyMembers.Count + 1);
                partyLevelsGained.Add(levelsGained);
            }
            int levels = player.gainXp(cache.xpOnTile / (cache.partyMembers.Count + 1));
            bool gainedClassLevel = player.gainClassXp(cache.xpOnTile) > 0;

            player.GainMoney(moneyHarvested + cache.moneyOnTile);

           
            if (cache.townId != -1 && cache.tileOwner != player.playerNumber)
            {
                
                NetworkData.Instance.players[player.playerNumber].GainTown(cache);
            }

            bool isFull = false;
            bool leveledUp = levels > 0;
            bool partyLevelUp = partyLevelsGained.Count > 0;
            int itemType = -1;

            foreach(var item in  itemsPicked)
            {
                if(NetworkData.Instance.AddItemToInventory(player.playerNumber, item))
                {
                    isFull = true;
                    itemType = item.determineType();
                }
            }
            
            endBattleInfo.lines.Add(player.name + " has gained <color=blue>" + (cache.xpOnTile) + "</color> xp ");
            if(cache.partyMembers.Count > 0) { endBattleInfo.lines[endBattleInfo.lines.Count - 1] += "(split between you and your allies)"; }


            if(leveledUp)
            {
                endBattleInfo.lines[endBattleInfo.lines.Count-1] += " and they've leveled up <color=blue>" + levels + "</color> times";
                
                levelUpUI.statsToAllocate += levels * NetworkData.Instance.statsPerLevel;
                levelUpUI.inControl = player.playerNumber;
                levelUpUI.playerWhoLevel = player;
                
                
            }
            if(partyLevelUp)
            {
                for(int i = 0; i < partyLevelsGained.Count; i ++)
                {
                    if (partyLevelsGained[i] <= 0) { continue; }
                    endBattleInfo.lines.Add(cache.partyMembers[i].name + "(ally) has leveled up <color=green>" + partyLevelsGained[i].ToString() + "</color> times");

                }
            }
            if (gainedClassLevel)
            {
                endBattleInfo.lines.Add("You're now a level <color=blue>" + player.playerClassProgress[player.playerClass].level + "</color> " + NetworkData.Instance.classDataBase.GetItem[player.playerClass].className);
            }
            endBattleInfo.lines.Add(player.name + " has gained " + (cache.moneyOnTile) + " money");

            if (itemsPicked.Count > 0)
            {
                string itemString = "You've picked up ";
                for(int i = 0; i < itemsPicked.Count; i++)
                {
                    if(i == itemsPicked.Count - 1 && itemsPicked.Count > 1)
                    {
                        itemString += " and a <color=blue>" + itemsPicked[i].name + "</color>";

                    }
                    else
                    {
                        itemString += "a<color=blue> " + itemsPicked[i].name;
                        if (i != itemsPicked.Count - 1) { itemString += "</color>, "; }
                    }
                }
                
                endBattleInfo.lines.Add(itemString);
            }
            //okay so this logic is mickey mouse but free me bru it cant be that deep
            bool pvpWin = deadPlayers.Count > 0;
            
            //LEMME MAKE THIS REAL CLEAR I KNOW I COULD IMPLEMENT SOME KIND OF QUEUE BUT LORD THAT SOUNDS LIKE A LOT OF THINKING
            //AND ITS 4 AM AND IM TIRED ANDF THISLL DO FRICK U
            if(leveledUp || isFull || pvpWin) 
            { 
                endBattleInfo.endEvent.RemoveAllListeners();
                endBattleInfo.endEvent.AddListener(delegate { endBattleInfo.gameObject.SetActive(false); });
            }
            if (leveledUp)
            {

                endBattleInfo.endEvent.AddListener(delegate { levelUpUI.Setup(); });
                levelUpUI.onFinishLevelUp.AddListener(delegate { SceneChanger.Instance.loadClientScenesServerRpc("MainGameUI"); });
            }
            if (isFull)
            {
                if(leveledUp)
                {
                    levelUpUI.onFinishLevelUp.RemoveAllListeners();
                    if(IsHost)
                    levelUpUI.onFinishLevelUp.AddListener(delegate { dropItem.SetUp(player.playerNumber, itemType); });

                }
                else
                {
                    if (IsHost)
                    endBattleInfo.endEvent.AddListener(delegate { dropItem.SetUp(player.playerNumber, itemType); });
                }
                dropItem.finishLose.AddListener(delegate { SceneChanger.Instance.loadClientScenesServerRpc("MainGameUI"); });
            }
            if (pvpWin)
            {
                //this strongly suggests i should fix my flow of things but man do i not want to
                if (isFull)
                {
                    dropItem.finishLose.RemoveAllListeners();
                    dropItem.finishLose.AddListener(delegate { pvpVictory.SetUp(deadPlayers[0], player.playerNumber); });

                }
                else if(leveledUp)
                {
                    levelUpUI.onFinishLevelUp.RemoveAllListeners();
                    levelUpUI.onFinishLevelUp.AddListener(delegate { pvpVictory.SetUp(deadPlayers[0], player.playerNumber); });

                }
                else
                {
                    endBattleInfo.endEvent.AddListener(delegate { pvpVictory.SetUp(deadPlayers[0], player.playerNumber); });
                }
                
            }
            

           

            
       
            
            endBattleInfo.gameObject.SetActive(true);
            endBattleInfo.startDialogue();
            endBattleInfo.whoInControl = player.playerNumber;
            

        }
        else if (victor.stats is EnemyCombat || victor.stats is PartyMember)
        {
            
            endBattleInfo.lines.Add("Every player (in this combat) has been defeated");

            if(victor.stats is EnemyCombat)
            {
                if ((victor.stats as EnemyCombat).persistant && cache.townId != -1 & cache.tileOwner != -1)
                {
                    NetworkData.Instance.players[cache.tileOwner].LoseTown(cache);
                }
            }
            
            foreach (var combat in allCombatants)
            {
                if (combat.stats is playerData)
                {
                    var current = combat.stats as playerData;
                    
                    
                    current.GainMoney(-current.playerInfo[PlayerInfo.money] / 2);
                    
                }
            }
            endBattleInfo.gameObject.SetActive(true);
            endBattleInfo.startDialogue();
            endBattleInfo.whoInControl = NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber;

            

        }
        
        PlayerCombatManager.Instance.combatants.Clear(); 
        statUI.gameObject.SetActive(false);
        NetworkData.Instance.setNextTurnNum();
        endBattleInfo.gameObject.GetComponentInChildren<Button>().Select();
      

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
        List<int> deadPlayers = RemoveDeadEntities();
        
        
        if(IsHost)
        {
            foreach(int player in deadPlayers)
            {
                EarlyCombatEndPlayerLoss(player);
            }
            EndDialogueRpc();
        }
      
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void LinesToSyncRpc(string stringToAdd, int turnsDead, int playerNumber)
    {
        endBattleInfo.lines.Add(stringToAdd);
        //NetworkData.Instance.players[playerNumber].death(turnsDead, false);
    }

    private List<int> RemoveDeadEntities() //Removes them from the database that stores all enemy info (it probably shouldn't be accessible all the time but fml
    {
        var tilereadCache = MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];

        tilereadCache.xpOnTile += xpHarvested;
        tilereadCache.moneyOnTile += moneyHarvested;

        for(int i = tilereadCache.partyMembers.Count -1; i >= 0; i--)
        {
            if (tilereadCache.partyMembers[i].isDead)
            {
                tilereadCache.partyMembers[i].Die();
            }
        }

        for(int i = tilereadCache.tileEnemy.Count - 1; i >= 0; i--)
        {
            
            if (tilereadCache.tileEnemy[i].isDead)
            {
                tilereadCache.tileEnemy.RemoveAt(i);
            }
        }
        List<int> deadPlayer = new List<int>();
        for (int i = tilereadCache.players.Count - 1; i >= 0; i--)
        {
            
            NetworkData.Instance.players[tilereadCache.players[i]].ClearCombatStatuses();
            if (NetworkData.Instance.players[tilereadCache.players[i]].isDead)
            {
                deadPlayer.Add(tilereadCache.players[i]);
                NetworkData.Instance.players[tilereadCache.players[i]].death(1);

            }
        }

        return deadPlayer;
    }
    
    private void EarlyCombatEndPlayerLoss(int playerId)
    {
        playerData current = NetworkData.Instance.players[playerId];
        string lostString = (current.name + " dropped " + current.playerInfo[PlayerInfo.money] / 2 + " moneys");
        LinesToSyncRpc(lostString, 3, current.playerNumber);

        ItemBase lostItem = current.LoseSomething();
        if (lostItem != null)
        {
            int itemType = lostItem.determineType();
            lostString = "Lost <color=red>" + lostItem.itemName + "</color>";
            LinesToSyncRpc(lostString, 3, current.playerNumber);
            PlayerItemDroppedRpc(NetworkData.Instance.playerInventories[0][itemType].database.GetId[lostItem], itemType);
        }
        else
        {
            LinesToSyncRpc("nothing was lost u (" + current.name + ") lucky son of a gun", 3, current.playerNumber);
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void EndDialogueRpc()
    {
        endBattleInfo.lines.Add("NEXT TIME ON DRAGON BALL Z");
        endBattleInfo.gameObject.SetActive(true);
        endBattleInfo.startDialogue();
        NetworkData.Instance.setNextTurnNum();

        endBattleInfo.gameObject.GetComponentInChildren<Button>().Select();
    }
    public void RightSpec(InputAction.CallbackContext action)
    {
        if(!action.started) { return; }
        Debug.Log("Im shifting left ");
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
    public void LeftSpec(InputAction.CallbackContext action)
    {
        if (!action.started) { return; }
        Debug.Log("Im shifting left ");
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

    
    private void UpdateVolume()
    {
        AudioSource.volume = SettingsManager.instance.volume; 
    }
    public void SpecScroll(InputAction.CallbackContext action)
    {
        scrolling = action.ReadValue<Vector2>().normalized;

    }
    public void SpecMouseMove(InputAction.CallbackContext action)
    {
        
        mouseMove = action.ReadValue<Vector2>();
    }
    private void LateUpdate()
    {
        if(playercontrol.currentActionMap.name != "Spectating") { return; }

        cameras[currentSpec].transform.position += cameras[currentSpec].transform.TransformDirection(Vector3.forward) * scrolling.y * Time.deltaTime * scrollSpeed;
        if(cameras[currentSpec].transform.parent != null)
        {
            

            cameras[currentSpec].transform.parent.transform.Rotate(new Vector3(-mouseMove.y * mouseSpeed * Time.deltaTime, 0, 0));

            if (cameras[currentSpec].transform.parent.transform.eulerAngles.x % 360 < 360 + minXCam && cameras[currentSpec].transform.parent.transform.eulerAngles.x % 360 > maxXCam)
            {
                cameras[currentSpec].transform.parent.transform.Rotate(new Vector3(mouseMove.y * mouseSpeed, 0, 0));
            }
        }
    }


    private void StartBGM()
    {
        SettingsManager.instance.onBackgroundVolumeChange.AddListener(UpdateVolume);

        if (PlayerCombatManager.Instance.currentEncounter.battleMusic)
        {
            AudioSource.resource = PlayerCombatManager.Instance.currentEncounter.battleMusic;
            AudioSource.volume = SettingsManager.instance.volume;

            AudioSource.Play();
        }


        foreach (var entity in PlayerCombatManager.Instance.combatants)
        {
            if (entity is EnemyCombat)
            {
                var enemy = entity as EnemyCombat;
                var soundCache = PlayerCombatManager.Instance.EnemyDataBase.GetItem[enemy.enemyId].SpecialMusic;
                if (soundCache)
                {
                    AudioSource.resource = soundCache;
                    AudioSource.volume = SettingsManager.instance.volume;
                    AudioSource.Play();
                }
            }
        }
    }

    private void CleanUpCams()
    {

        for (int i = cameras.Count - 1; i >= 0; i--)
        {
            if (cameras[i] == null || cameras[i].gameObject == null)
            {
                if(currentSpec == i)
                {
                    currentSpec = 0;
                }
                cameras.RemoveAt(i);
                Debug.Log("I cleaned up a camera ");
            }
        }

        
    }

    private AbilityManager SaveEntities(AbilityManager victor)
    {
        AbilityManager winner = victor;
        if (victor.stats is PartyMember)
        {
            PartyMember GOAT = (PartyMember)victor.stats;
            
            
            foreach (var combatant in allCombatants)
            {
                if (combatant.stats is not playerData) { continue; }
                if ((combatant.stats as playerData).playerNumber == GOAT.allyOwner)
                {
                    winner = combatant;
                    if (NetworkData.Instance.players[GOAT.allyOwner].isDead)
                    {
                        NetworkData.Instance.players[GOAT.allyOwner].stats[Attributes.Health] = 1;
                        NetworkData.Instance.players[GOAT.allyOwner].isDead = false;
                    }
                    break;
                }
            }

            Debug.Log("Who wonned " + winner.stats.name);
            

            
        }
        var tilereadCache = MapTileSpecialEvents.Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curMap][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];

        if(victor.stats is playerData)
        {
            var partyWinner = victor.stats as playerData;

            for (int i = tilereadCache.partyMembers.Count - 1; i >= 0; i--)
            {
                if (!tilereadCache.partyMembers[i].isDead) { continue; }
                if (tilereadCache.partyMembers[i].allyOwner == partyWinner.playerNumber)
                {
                    tilereadCache.partyMembers[i].stats[Attributes.Health] = 1;
                    tilereadCache.partyMembers[i].isDead = false;

                }
                else
                {
                    tilereadCache.partyMembers[i].Die();
                }


            }

        }
        return winner;
    }
}


[System.Serializable]
public class AllyAIWrapper
{
    public PartyAITypes type;
    public GameObject prefab;
}