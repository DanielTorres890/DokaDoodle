using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;

using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerMoveManager : NetworkBehaviour
{

    [SerializeField] public List<TileScript> mapTiles = new List<TileScript>();

    public TMP_Text rollNum;

    [SerializeField] private List<GameObject> takenPath = new List<GameObject>();
    public List<GameObject> playerSticks = new List<GameObject>();
    private List<Animator> stickAnimators = new List<Animator>();

    public int mapNumber;
    public CinemachineCamera playerCam;
    public GameObject gameMenu;

    public bool canMove = false;
    public bool cameraMove = false;
    public bool autoMoving = false;

    [Tooltip("DEBUG OPTION forces a number to be rolled")]
    public bool forceRoll;
    [Tooltip("if forceroll is on this is the number to be rolled")]
    public int forcedRollNum;


    int diceRoll = 0;


    [SerializeField] private float moveSpeed = 500f;
    [SerializeField] private float autoMoveTime = 0.2f;
    [SerializeField] private float autoMoveSpeed = 0.2f;
    [Tooltip("When enemies are visually spawned on the overworld this is how far apart they'll be ")]
    public Vector2 enemyDistance;

    public AudioClip BGM;
    private Vector3 cameraMoveDirection;
    [SerializeField] private List<TileScript> possibleEndTiles = new List<TileScript>();
    [SerializeField] List<PathWrapper> allPaths = new List<PathWrapper>();

    [SerializeField] List<TileScript> bestPath = new List<TileScript>();

    public static PlayerMoveManager Instance;


    public GameObject allyPrefab;
    public float AllyDistance;

    //DICTIONARIES SOLVE EVERYTHING HOLYYY
    private List<Dictionary<PartyMember, GameObject>> playerAllies = new List<Dictionary<PartyMember, GameObject>>();
    private Coroutine activeRoutine;

    public void Awake()
    {

        for (int i = 0; i < mapTiles.Count; i++)
        {
            mapTiles[i].tileId = i;
        }
        Instance = this;

        if (BGM) { BGMManager.instance.PlaySound(BGM); }




        setUpTileEnemies();
        playerSticks = GameObject.FindGameObjectWithTag("Data").GetComponent<NetworkData>().playerSticks;

        //mostly an artifact of old system but w/e
        for (int i = 0; i < NetworkData.Instance.players.Count; i++)
        {
            stickAnimators.Add(playerSticks[i].GetComponent<Animator>());
        }
        StickPlacer();
        PartyMemberSpawner();
        playerCam.Follow = playerSticks[NetworkData.Instance.currentPlayer].transform;
        FreeMover.Instance.playerCam = playerCam;


    }

    private void Update()
    {
    }




    public void rollDice()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        int rollMultiplier = 1;


        foreach (var status in NetworkData.Instance.GetCurrentPlayer().statuses)
        {
            var currentBuff = NetworkData.Instance.buffDataBase.GetItem[status.buffId];
            if (currentBuff is RollBuff)
            {

                rollMultiplier = (currentBuff as RollBuff).rollMultiplier;
                break;
            }
        }

        int totalRoll = 0;
        for (int i = 0; i < rollMultiplier; i++)
        {

            int randomNum = UnityEngine.Random.Range(0, 101);

            if (randomNum <= 0) { diceRoll = 0; }

            else { diceRoll = (randomNum % 7) + 1; } 
            totalRoll += diceRoll;
        }


        foreach (var status in NetworkData.Instance.GetCurrentPlayer().statuses)
        {
            var currentBuff = NetworkData.Instance.buffDataBase.GetItem[status.buffId];
            if (currentBuff is ForceRollBuff)
            {
                totalRoll = (currentBuff as ForceRollBuff).forcedNumber;
                break;
            }
        }


        canMove = true;
        if (forceRoll)
        {
            SyncDiceRollServerRpc(forcedRollNum);//can force die roll with this
        }
        else
        {
            SyncDiceRollServerRpc(totalRoll);
        }

        takenPath.Clear();
        mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].playersOnTile[NetworkData.Instance.currentPlayer] = false;
        AddPathRpc(NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void AddPathRpc(int tileId)
    {
        takenPath.Add(mapTiles[tileId].gameObject);
        NetworkData.Instance.GetCurrentPlayer().curTileId = tileId;
        SetFollowingMembersToCurTile();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void RemovePathRpc()
    {
        takenPath.RemoveAt(takenPath.Count - 1);
        NetworkData.Instance.GetCurrentPlayer().curTileId = takenPath[takenPath.Count - 1].GetComponent<TileScript>().tileId;
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SyncDiceRollServerRpc(int num)
    {
        
        stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", true);
        SetAllyAnimator(true);

        diceRoll = num;
        //id like to say that while this is not the most beautiful thing in the world i cant hate it
        ClientChecks.Instance.rollNum.text = diceRoll.ToString();
        ClientChecks.Instance.rollNum.transform.parent.gameObject.SetActive(true);
    }


    public void confirmMove(InputAction.CallbackContext action)
    {

        if (!canMove) { return; }

        if (diceRoll <= 0)
        {


            canMove = false;

            SyncPlayerTileServerRpc(NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
            SetNextTurnServerRpc();

        }
    }



    public void MovePlayer(InputAction.CallbackContext action)
    {
        if(autoMoving) { return; }
        if (!canMove) { return; }
        if (cameraMove) { return; }
        //just as a note to self taken path defaults to your current tile being in there after a roll so its always at least 1
        if (takenPath.Count <= 1 && diceRoll <= 0) { return; };

        var direction = action.action.ReadValue<Vector2>();
        var curTile = mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];
        var thisMapSpecial = MapTileSpecialEvents.Instance.mapTiles[mapNumber];

        
        if (direction == Vector2.up && mapTiles[curTile.tileId].upTile != null && thisMapSpecial[mapTiles[curTile.tileId].upTile.GetComponent<TileScript>().tileId].passable)
        {


            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.upTile)) && diceRoll > 0)
            {

                diceRoll--;
                AddPathRpc(curTile.upTile.GetComponent<TileScript>().tileId);


            }
            else if (takenPath[takenPath.Count - 2] == curTile.upTile)
            {
                diceRoll++;
                RemovePathRpc();
            }
            else { return; }
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = curTile.upTile.GetComponent<TileScript>().tileId;

        }
        if (direction == Vector2.down && mapTiles[curTile.tileId].downTile != null && thisMapSpecial[mapTiles[curTile.tileId].downTile.GetComponent<TileScript>().tileId].passable)
        {

            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.downTile)) && diceRoll > 0)
            {
                diceRoll--;
                AddPathRpc(curTile.downTile.GetComponent<TileScript>().tileId);

            }
            else if (takenPath[takenPath.Count - 2] == curTile.downTile)
            {
                diceRoll++;
                RemovePathRpc();
            }
            else { return; }
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = curTile.downTile.GetComponent<TileScript>().tileId;
        }

        if (direction == Vector2.right && mapTiles[curTile.tileId].rightTile != null && thisMapSpecial[mapTiles[curTile.tileId].rightTile.GetComponent<TileScript>().tileId].passable)
        {

            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.rightTile)) && diceRoll > 0)
            {
                diceRoll--;
                AddPathRpc(curTile.rightTile.GetComponent<TileScript>().tileId);

            }
            else if (takenPath[takenPath.Count - 2] == curTile.rightTile)
            {
                diceRoll++;
                RemovePathRpc();
            }
            else { return; }
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = curTile.rightTile.GetComponent<TileScript>().tileId;
        }

        if (direction == Vector2.left && mapTiles[curTile.tileId].leftTile != null && thisMapSpecial[mapTiles[curTile.tileId].leftTile.GetComponent<TileScript>().tileId].passable)
        {

            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.leftTile)) && diceRoll > 0)
            {
                diceRoll--;
                AddPathRpc(curTile.leftTile.GetComponent<TileScript>().tileId);

            }
            else if (takenPath[takenPath.Count - 2] == curTile.leftTile)
            {
                diceRoll++;
                RemovePathRpc();
            }
            else { return; };
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = curTile.leftTile.GetComponent<TileScript>().tileId;
        }

        SyncDiceRollServerRpc(diceRoll);
        ClientChecks.Instance.rollNum.text = diceRoll.ToString();
        StopAllCoroutines();
        StartCoroutine(playerMover(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId));
        PlayerMoverRpc(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
    }


    public void UndoMove(InputAction.CallbackContext action)
    {
        if (!canMove || takenPath.Count < 2 || !action.performed || autoMoving) { return; }

        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = takenPath[takenPath.Count - 2].GetComponent<TileScript>().tileId;

        diceRoll++;
        RemovePathRpc();
        SyncDiceRollServerRpc(diceRoll);
        ClientChecks.Instance.rollNum.text = diceRoll.ToString();
        StopAllCoroutines();
        StartCoroutine(playerMover(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId));
        PlayerMoverRpc(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
        


    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SyncPlayerTileServerRpc(int id)
    {
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = id;
        SetFollowingMembersToCurTile();

    }



    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayerMoverRpc(float speed, int tildid, RpcParams rpcstuff = default)
    {
        if (rpcstuff.Receive.SenderClientId == NetworkManager.Singleton.LocalClientId) { return; }

        if(activeRoutine != null)
        StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(playerMover(speed, tildid));
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetNextTurnServerRpc()
    {
        PerformAllyMoves();
        SetAllyAnimator(false);

        stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", false);
        ClientChecks.Instance.rollNum.transform.parent.gameObject.SetActive(true);
        MapTileSpecialEvents.Instance.mapTiles[mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players.Add(NetworkData.Instance.currentPlayer);
        
        if (MapTileSpecialEvents.Instance.mapTiles[mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds.Count > 0 && IsServer)
        {
            ClientChecks.Instance.ActivateTrapsRpc();
        }
        else 
        { 
            mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].TileEvent(); 
        }

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void NextTurnRpc()
    {
        NetworkData.Instance.setNextTurnNum();
        if (IsHost)
        {
            SceneChanger.Instance.loadClientScenesServerRpc("MainGameUI");
        }


    }

    private IEnumerator playerMover(float speed, int tildId)
    {
        var currentPartyMembers = NetworkData.Instance.GetCurrentPlayer().partyMembers;

        int curPlayerIndex = NetworkData.Instance.currentPlayer;
        while (Vector3.Distance(playerSticks[curPlayerIndex].transform.position, mapTiles[tildId].gameObject.transform.position) > 0.01f)
        {
            var tilePos = mapTiles[tildId].gameObject.transform.position;
            playerSticks[NetworkData.Instance.currentPlayer].transform.position =
        Vector3.MoveTowards(playerSticks[curPlayerIndex].transform.position, new Vector3(tilePos.x, tilePos.y + 3, tilePos.z) , speed * Time.deltaTime);

            int memberCount = 0;
            for(int i = 0; i < currentPartyMembers.Count; i++)
            {
                if (currentPartyMembers[i].curMap != mapNumber || currentPartyMembers[i].boardMovementState != PlayerFollowingStates.WithOwner) { continue; }

                playerAllies[curPlayerIndex][currentPartyMembers[i]].transform.position = new Vector3(playerSticks[curPlayerIndex].transform.position.x - 0.5f + memberCount * AllyDistance, playerSticks[curPlayerIndex].transform.position.y , playerSticks[curPlayerIndex].transform.position.z - .5f);
                memberCount++;
            }
            yield return null;
        }

    }

    //id like to say that in an ideal world id be able to directly set up a lot of these things in the inspector
    //but it doesnt support 2d data structures
    private void setUpTileEnemies()
    {

        if (MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber] == null)
        {
            MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber] = new SpecialTileEventHold[PlayerMoveManager.Instance.mapTiles.Count];
            for (int i = 0; i < MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber].Length; i++)
            {
                
                MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][i] = new SpecialTileEventHold
                {
                    battleArea = PlayerMoveManager.Instance.mapTiles[i].battleEnvironment
                };


                var thisTile = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][i];
                thisTile.passable = Instance.mapTiles[i].initiallyPassable;

                if (mapNumber == 0 && i == 0)
                {
                    
                    for (int j = 0; j < NetworkData.Instance.maxPlayers; j++)
                    {
                        thisTile.players.Add(j);
                    }
                }
                //id like to say that im not super happy about this but things are getting messy
                //they NEED to know their town id right away otherwise its really unintuitive
                if (PlayerMoveManager.Instance.mapTiles[i] is TownTile)
                {
                    thisTile.townId = NetworkData.Instance.TownInfoDataBase.GetId[(PlayerMoveManager.Instance.mapTiles[i] as TownTile).Info];
                    foreach (var enemy in Instance.mapTiles[i].defaultTileEnemies.enemies)
                    {
                        var enemystats = new EnemyCombat(enemy);
                        enemystats.persistant = true;
                        thisTile.tileEnemy.Add(enemystats);
                    }

                }
                else
                {
                    thisTile.townId = -1;
                }
            }
        }

        for (int i = 0; i < MapTileSpecialEvents.Instance.mapTiles[mapNumber].Length; i++)
        {
            if (MapTileSpecialEvents.Instance.mapTiles[mapNumber][i].tileEnemy.Count != 0)
            {
                spawnEnemyOverworld(i, MapTileSpecialEvents.Instance.mapTiles[mapNumber][i].tileEnemy);

            }

        }

    }

    public void spawnEnemyOverworld(int tileId, List<EnemyCombat> enemies)
    {

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetItem[enemies[i].enemyId].enemyNonCombatPrefab);
            enemy.transform.position = mapTiles[tileId].transform.position;
            enemy.transform.position = new Vector3(enemy.transform.position.x + (enemyDistance.x * (i % Mathf.CeilToInt(Mathf.Sqrt(enemies.Count)))) - 1.5f, enemy.transform.position.y + 3, enemy.transform.position.z + (-enemyDistance.y * (i / Mathf.CeilToInt(Mathf.Sqrt(enemies.Count)))));
            enemy.transform.localScale = new Vector3(1, 1, 1);

        }

    }
    /*private void FightOrNot()
    {
        bool rumble = false;
        foreach (var players in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players)
        {
            if (players != NetworkData.Instance.players[NetworkData.Instance.currentPlayer].playerNumber && Instance.mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].canFight)
            {
                rumble = true;
            }
        }

        if ((MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].tileEnemy == null || !rumble) && !NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead)
        {
            playerCam.Follow = playerSticks[NetworkData.Instance.currentPlayer].transform;
            gameMenu.SetActive(true);
            rollNum.gameObject.transform.parent.gameObject.SetActive(false);
        }
            
        else
        {

            if (NetworkData.Instance.players[NetworkData.Instance.currentPlayer].isDead)
            {
                gameMenu.SetActive(false);

                ClientChecks.Instance.DisplayDeadRpc();
                return;
            }

            gameMenu.SetActive(false);

            NextTurnRpc();
        }
    }*/

    public void FreeCamera(InputAction.CallbackContext action)
    {

        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        if (!canMove) { return; }

        if (!action.performed) { return; }


        FreeCameraRpc();

        FreeMover.Instance.onUndoFree.AddListener(delegate { cameraMove = false; });
        FreeMover.Instance.FreeCamera();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void FreeCameraRpc()
    {
        cameraMove = true;
        possibleEndTiles.Clear();
        var currrentPlayer = NetworkData.Instance.GetCurrentPlayer();

        allPaths.Clear();
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
            FreeMover.Instance.onTileSelect.AddListener(GoToTile);

        int lookback = 1;
        if(takenPath.Count > 1) { lookback = 2; }
        PossibleTiles(mapTiles[currrentPlayer.curTileId], diceRoll, takenPath[takenPath.Count-lookback].GetComponent<TileScript>(), ref allPaths, new List<TileScript>());

    }
    public void UnfreeCamera()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!canMove) { return; }
        if (!cameraMove) { return; }

        FreeMover.Instance.EndFreeCamera();
        UnfreeCameraRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void UnfreeCameraRpc()
    {
        
        foreach (var tile in possibleEndTiles)
        {
            tile.ArrowChange(false);
        }
    }

    public void GoToTile(int TileId)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (possibleEndTiles.Contains(mapTiles[TileId]))
        {
            //the index of the tile in list of possiblendtiles (ik amazing names here)
            int finishTile = 0;
            foreach(var tile in possibleEndTiles)
            {
                if(tile == mapTiles[TileId]) { break; }
                finishTile++;
            }
            
            
            FreeMover.Instance.EndFreeCamera();
            UnfreeCameraRpc();

            StartCoroutine(ToEachTileInPath(finishTile));

        }
    }

    //there's definitely a way to optimize this to not need previous tile but do it matter i think not
    //pt 2 after making it track the path i gotta say creating a new object on every iteration feels bad too but idk how else you would do it since each one needs to track its own list
    //maybe some optimization where if multiple tiles share the same path (which can and does happen) they could use the same prior path or something but not needed i dont think
    private void PossibleTiles(TileScript tile, int rollLeft, TileScript previousTile, ref List<PathWrapper> takenTilePath, List<TileScript> thisPath)
    {
        //
        if (rollLeft == 0)
        {
            if (possibleEndTiles.Contains(tile)) { return; }
            possibleEndTiles.Add(tile);
            thisPath.Add(tile);
            takenTilePath.Add(new PathWrapper(thisPath));
            tile.ArrowChange(true);
            return;
        }

        thisPath.Add(tile);
        if (tile.upTile && tile.upTile != previousTile.gameObject && MapTileSpecialEvents.Instance.mapTiles[mapNumber][tile.upTile.GetComponent<TileScript>().tileId].passable) 
        { PossibleTiles(tile.upTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }

        if (tile.downTile && tile.downTile != previousTile.gameObject && MapTileSpecialEvents.Instance.mapTiles[mapNumber][tile.downTile.GetComponent<TileScript>().tileId].passable) 
        { PossibleTiles(tile.downTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }

        if (tile.rightTile && tile.rightTile != previousTile.gameObject && MapTileSpecialEvents.Instance.mapTiles[mapNumber][tile.rightTile.GetComponent<TileScript>().tileId].passable) 
        { PossibleTiles(tile.rightTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }

        if (tile.leftTile && tile.leftTile != previousTile.gameObject && MapTileSpecialEvents.Instance.mapTiles[mapNumber][tile.leftTile.GetComponent<TileScript>().tileId].passable) 
        { PossibleTiles(tile.leftTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }

    }



    private void BestPath(TileScript tile, TileScript TargetTile)
    {
        List<TileScript> thisPath = new List<TileScript>();
        bestPath.Clear();


        List<int> distancedPath = Enumerable.Repeat(-1,mapTiles.Count).ToList();
        TileScript[] parentNodes = new TileScript[mapTiles.Count];

      
        distancedPath[tile.tileId] = 0;

        thisPath.Add(tile);

        int loopProtector = 0;
        while (thisPath.Count > 0)
        {
            var currentVertex = thisPath[0];
            thisPath.RemoveAt(0);

            if(currentVertex.upTile)
            {
                var lookAheadTile = currentVertex.upTile.GetComponent<TileScript>();
                if (distancedPath[lookAheadTile.tileId] == -1)
                {
                    parentNodes[lookAheadTile.tileId] = currentVertex;
                    distancedPath[lookAheadTile.tileId] = distancedPath[currentVertex.tileId] + 1;
                    thisPath.Add(lookAheadTile);
                }

            }


            if (currentVertex.leftTile)
            {
                var lookAheadTile = currentVertex.leftTile.GetComponent<TileScript>();
                if (distancedPath[lookAheadTile.tileId] == -1)
                {
                    parentNodes[lookAheadTile.tileId] = currentVertex;
                    distancedPath[lookAheadTile.tileId] = distancedPath[currentVertex.tileId] + 1;
                    thisPath.Add(lookAheadTile);
                }

            }

            if (currentVertex.rightTile)
            {
                var lookAheadTile = currentVertex.rightTile.GetComponent<TileScript>();
                if (distancedPath[lookAheadTile.tileId] == -1)
                {
                    parentNodes[lookAheadTile.tileId] = currentVertex;
                    distancedPath[lookAheadTile.tileId] = distancedPath[currentVertex.tileId] + 1;
                    thisPath.Add(lookAheadTile);
                }

            }

            if (currentVertex.downTile)
            {
                var lookAheadTile = currentVertex.downTile.GetComponent<TileScript>();
                if (distancedPath[lookAheadTile.tileId] == -1)
                {
                    parentNodes[lookAheadTile.tileId] = currentVertex;
                    distancedPath[lookAheadTile.tileId] = distancedPath[currentVertex.tileId] + 1;
                    thisPath.Add(lookAheadTile);
                }

            }
            loopProtector += 1;

            if(loopProtector> 1000) {
                Debug.Log("infinite 1");
                break; }
        }


        if (distancedPath[TargetTile.tileId] == -1)
        {
            Debug.Log("No possible path!");
            return;
        }

        
        int currentNode = TargetTile.tileId;
        bestPath.Add(TargetTile);

        loopProtector = 0;
        while (parentNodes[currentNode] != null)
        {
            bestPath.Add(parentNodes[currentNode]);
            currentNode = parentNodes[currentNode].tileId;
            loopProtector += 1;
            
        }

        bestPath.Reverse();

    }

    public IEnumerator ToEachTileInPath(int finishTile)
    {
        
        int startingTileIndex = 1;
        autoMoving = true;
        while (startingTileIndex < allPaths[finishTile].takenPath.Count)
        {

            
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = allPaths[finishTile].takenPath[startingTileIndex].tileId;
            SetFollowingMembersToCurTile();



            activeRoutine = StartCoroutine(playerMover(autoMoveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId));
            PlayerMoverRpc(autoMoveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
            startingTileIndex++;
            SyncDiceRollServerRpc(diceRoll-1);
            yield return new WaitForSeconds(autoMoveTime);


        }

        autoMoving = false;
        confirmMove(new InputAction.CallbackContext());
    }

    private void StickPlacer()
    {
        float xoffset = 0;
        
        for (int i = 0; i < NetworkData.Instance.players.Count; i++)
        {
            playerSticks[i].SetActive(true);
            if (mapNumber != NetworkData.Instance.players[i].curMap)
            {
                playerSticks[i].SetActive(false);
                return;
            }
                

           


                int playersOnTile = MapTileSpecialEvents.Instance.mapTiles[mapNumber][NetworkData.Instance.players[i].curTileId].players.Count;

            int intIndex = 0;
            foreach (var playerId in MapTileSpecialEvents.Instance.mapTiles[mapNumber][NetworkData.Instance.players[i].curTileId].players)
            {
                if (playerId == i) { break; }
                intIndex += 1;
            }
            int stagger = 1;

            if(intIndex > 1) { stagger = -1; }
            
            playerSticks[i].transform.position = mapTiles[NetworkData.Instance.players[i].curTileId].transform.position;
            playerSticks[i].transform.position = new Vector3(playerSticks[i].transform.position.x + intIndex % 2, playerSticks[i].transform.position.y + 3, playerSticks[i].transform.position.z - 2 + 1f * stagger);
                        
            xoffset += 1;
            
        }
    }

    private void SetAllyAnimator(bool state)
    {
        var currentPartyMembers = NetworkData.Instance.GetCurrentPlayer().partyMembers;
        int memberCount = 0;
        int curPlayerIndex = NetworkData.Instance.currentPlayer;
        for (int i = 0; i < currentPartyMembers.Count; i++)
        {
            if (currentPartyMembers[i].curMap != mapNumber || currentPartyMembers[i].boardMovementState != PlayerFollowingStates.WithOwner) { continue; }
            playerAllies[curPlayerIndex][currentPartyMembers[i]].GetComponent<Animator>().SetBool("Walking", state);
            memberCount++;
        }
    }
    private void PartyMemberSpawner()
    {
        

        int memberCount = 0;
        for (int i = 0; i < NetworkData.Instance.players.Count; i++)
        {
            Dictionary<PartyMember, GameObject> allyGameObjects = new Dictionary<PartyMember, GameObject>();
            foreach (var member in NetworkData.Instance.players[i].partyMembers)
            {
                if (member.curMap != mapNumber) { continue; }
                GameObject allyGameObject = Instantiate(allyPrefab);
                member.SetPrefab(allyGameObject);
                allyGameObjects.Add(member, allyGameObject);
                memberCount += 1;
            }
            playerAllies.Add(allyGameObjects);
        }
        PositionAllies();
    }

    private void SetFollowingMembersToCurTile()
    {

        foreach (var member in NetworkData.Instance.GetCurrentPlayer().partyMembers)
        {
            if (member.boardMovementState != PlayerFollowingStates.WithOwner || member.curMap != mapNumber) { continue; }

            MapTileSpecialEvents.Instance.mapTiles[mapNumber][member.curTileId].partyMembers.Remove(member);
            member.curTileId = NetworkData.Instance.GetCurrentPlayer().curTileId;
            MapTileSpecialEvents.Instance.mapTiles[mapNumber][member.curTileId].partyMembers.Add(member);
            
            

        }

    }

    private void PerformAllyMoves()
    {

        foreach (var member in NetworkData.Instance.GetCurrentPlayer().partyMembers)
        {
       
            if (member.boardMovementState == PlayerFollowingStates.WithOwner || member.curMap != mapNumber) { continue; }

       
            int targetTileId = member.targetTile;

            if(member.boardMovementState == PlayerFollowingStates.FollowingOwner) { targetTileId = NetworkData.Instance.players[member.allyOwner].curTileId; }

            BestPath(mapTiles[member.curTileId], mapTiles[targetTileId]);

            if (bestPath.Count == 0) { continue; }
  
            if(bestPath.Count <= 4)
            {
                MapTileSpecialEvents.Instance.mapTiles[mapNumber][member.curTileId].partyMembers.Remove(member);          
                member.curTileId = targetTileId;
                MapTileSpecialEvents.Instance.mapTiles[mapNumber][member.curTileId].partyMembers.Add(member);
                if (member.boardMovementState == PlayerFollowingStates.FollowingOwner) { member.boardMovementState = PlayerFollowingStates.WithOwner; }
                continue;

            }

            MapTileSpecialEvents.Instance.mapTiles[mapNumber][member.curTileId].partyMembers.Remove(member);
            member.curTileId = bestPath[3].tileId;
            MapTileSpecialEvents.Instance.mapTiles[mapNumber][member.curTileId].partyMembers.Add(member);

        }
        if (IsHost)
        {
            AllyActionsRpc(UnityEngine.Random.Range(0, 100));
        }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void AllyActionsRpc(int randomNum)
    {
        
        for(int i = NetworkData.Instance.GetCurrentPlayer().partyMembers.Count - 1; i >= 0 ; i--)
        {
            var member = NetworkData.Instance.GetCurrentPlayer().partyMembers[i];

            if (member.boardMovementState == PlayerFollowingStates.WithOwner || member.curMap != mapNumber) { continue; }
            

            if (mapTiles[member.curTileId] is not DefaultTile && mapTiles[member.curTileId] is not TownTile) { continue; }
            
            var memberTile = MapTileSpecialEvents.Instance.mapTiles[member.curMap][member.curTileId];
            var potentialEnemies = new List<EntityStats>(memberTile.tileEnemy);
            foreach(var ally in memberTile.partyMembers)
            {
                if(ally.allyOwner == member.allyOwner) { continue; }
                potentialEnemies.Add(ally);
            }

            if(potentialEnemies.Count > 0)
            {
                Debug.Log("I did box a special enemy");
                var died = member.OffScreenCombat(potentialEnemies);
                foreach (var enemy in potentialEnemies)
                {  
                    Debug.Log("am i potentialman?");
                    if (enemy.isDead)
                    {
                        Debug.Log("im dead");
                        if(enemy is EnemyCombat) 
                        {
                            Debug.Log("This guy should be toast");
                            memberTile.tileEnemy.Remove(enemy as EnemyCombat); 
                        }
                        if(enemy is PartyMember) 
                        { 
                            (enemy as PartyMember).Die(); 
                        }
                    }
                }
                if(!died && (mapTiles[member.curTileId] is TownTile))
                {
                    NetworkData.Instance.players[member.allyOwner].GainTown(memberTile);
                }

            }
            else
            {
                if (mapTiles[member.curTileId] is DefaultTile)
                {
                    
                    var defaultT = mapTiles[member.curTileId] as DefaultTile;

                    List<EntityStats> enemyList = new List<EntityStats>();


                    foreach (var enemy in defaultT.enemies[randomNum % defaultT.enemies.Length].enemies)
                    {

                        enemyList.Add(new EnemyCombat(enemy));
                    }
                    bool dead = member.OffScreenCombat(enemyList);

                    if(dead)
                    {
                        member.Die();
                    }

                }
                else
                {
                    var townT = mapTiles[member.curTileId] as TownTile;
                    member.gainXp(townT.Info.baseXpGeneration * memberTile.unitLevel);

                }
                
            }
            
        }
    }
    private void PositionAllies()
    {
        int memberCount = 0;
        for (int i = 0; i < NetworkData.Instance.players.Count; i++)
        {
            
            foreach (var member in playerAllies[i])
            {
                if(member.Key.boardMovementState == PlayerFollowingStates.WithOwner)
                {
                    GameObject allyGameObject = member.Value;
                    Debug.Log("I just positioned you ");
                    allyGameObject.transform.position = new Vector3(playerSticks[i].transform.position.x - 0.5f + memberCount * AllyDistance, playerSticks[i].transform.position.y, playerSticks[i].transform.position.z - .5f);
                    
                }
                else
                {
                    GameObject allyGameObject = member.Value;
                    allyGameObject.transform.position = new Vector3(mapTiles[member.Key.curTileId].transform.position.x - 0.5f + memberCount * AllyDistance, mapTiles[member.Key.curTileId].transform.position.y, mapTiles[member.Key.curTileId].transform.position.z - .5f);

                }
                memberCount += 1;
            }
        }
    }
}

[System.Serializable]
public class PathWrapper
{
    [SerializeField] public List<TileScript> takenPath;

    public PathWrapper(List<TileScript> path) { takenPath = path; }
}
