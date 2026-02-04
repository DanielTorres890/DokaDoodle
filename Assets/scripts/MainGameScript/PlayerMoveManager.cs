using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private float cameraSpeed = 5f;
    [SerializeField] private float moveSpeed = 500f;
    [SerializeField] private float autoMoveTime = 0.2f;
    [SerializeField] private float autoMoveSpeed = 0.2f;
    [Tooltip("When enemies are visually spawned on the overworld this is how far apart they'll be ")]
    public Vector2 enemyDistance;

    public AudioClip BGM;
    private Vector3 cameraMoveDirection;
    [SerializeField] private List<TileScript> possibleEndTiles = new List<TileScript>();
    [SerializeField] List<PathWrapper> allPaths = new List<PathWrapper>();

    public static PlayerMoveManager Instance;


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

            int randomNum = UnityEngine.Random.Range(0, 100);

            if (randomNum <= 3) { diceRoll = 0; }

            else { diceRoll = Convert.ToInt32(Math.Ceiling(randomNum / 15f)); }
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
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void AddPathRpc(int tileId)
    {
        takenPath.Add(mapTiles[tileId].gameObject);
        NetworkData.Instance.GetCurrentPlayer().curTileId = tileId;
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void RemovePathRpc()
    {
        takenPath.RemoveAt(takenPath.Count - 1);
        NetworkData.Instance.GetCurrentPlayer().curTileId = takenPath[takenPath.Count - 1].GetComponent<TileScript>().tileId;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SyncDiceRollServerRpc(int num)
    {
        stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", true);

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


        if (direction == Vector2.up && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].upTile != null)
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
        if (direction == Vector2.down && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].downTile != null)
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

        if (direction == Vector2.right && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].rightTile != null)
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

        if (direction == Vector2.left && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].leftTile != null)
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
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SyncPlayerTileServerRpc(int id)
    {
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = id;
    }



    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PlayerMoverRpc(float speed, int tildid, RpcParams rpcstuff = default)
    {
        if (rpcstuff.Receive.SenderClientId == NetworkManager.Singleton.LocalClientId) { return; }

        if(activeRoutine != null)
        StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(playerMover(speed, tildid));
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetNextTurnServerRpc()
    {
        stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", false);
        ClientChecks.Instance.rollNum.transform.parent.gameObject.SetActive(true);
        MapTileSpecialEvents.Instance.mapTiles[mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players.Add(NetworkData.Instance.currentPlayer);
        
        if (MapTileSpecialEvents.Instance.mapTiles[mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].trapIds.Count > 0 && IsServer)
        {
            ClientChecks.Instance.ActivateTrapsRpc();
        }
        else { mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].TileEvent(); }

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void NextTurnRpc()
    {
        StickPlacer();
        NetworkData.Instance.setNextTurnNum();
        ClientChecks.Instance.PreturnStuff();


    }

    private IEnumerator playerMover(float speed, int tildId)
    {

        while (Vector3.Distance(playerSticks[NetworkData.Instance.currentPlayer].transform.position, mapTiles[tildId].gameObject.transform.position) > 0.01f)
        {
            playerSticks[NetworkData.Instance.currentPlayer].transform.position =
        Vector3.MoveTowards(playerSticks[NetworkData.Instance.currentPlayer].transform.position, mapTiles[tildId].gameObject.transform.position, speed * Time.deltaTime);
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

                if (mapNumber == 0 && i == 0)
                {
                    Debug.Log("Either i set up or im missing vital information ");
                    for (int j = 0; j < NetworkData.Instance.maxPlayers; j++)
                    {
                        MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][i].players.Add(j);
                    }
                }
                //id like to say that im not super happy about this but things are getting messy
                //they NEED to know their town id right away otherwise its really unintuitive
                if (PlayerMoveManager.Instance.mapTiles[i] is TownTile)
                {
                    MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][i].townId = NetworkData.Instance.TownInfoDataBase.GetId[(PlayerMoveManager.Instance.mapTiles[i] as TownTile).Info];
                    foreach (var enemy in Instance.mapTiles[i].defaultTileEnemies.enemies)
                    {
                        var enemystats = new EnemyCombat(enemy);
                        enemystats.persistant = true;
                        MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][i].tileEnemy.Add(enemystats);
                    }

                }
                else
                {
                    MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][i].townId = -1;
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
            enemy.transform.position = new Vector3(enemy.transform.position.x + (enemyDistance.x * (i % (enemies.Count / 2 + 1))), enemy.transform.position.y, enemy.transform.position.z + (-enemyDistance.y * (i / ((enemies.Count / 2) + 1))));
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

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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
        if (tile.upTile && tile.upTile != previousTile.gameObject) { PossibleTiles(tile.upTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }
        if (tile.downTile && tile.downTile != previousTile.gameObject) { PossibleTiles(tile.downTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }
        if (tile.rightTile && tile.rightTile != previousTile.gameObject) { PossibleTiles(tile.rightTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }
        if (tile.leftTile && tile.leftTile != previousTile.gameObject) { PossibleTiles(tile.leftTile.GetComponent<TileScript>(), rollLeft - 1, tile, ref takenTilePath, new List<TileScript>(thisPath)); }


    }


    public IEnumerator ToEachTileInPath(int finishTile)
    {
        
        int startingTileIndex = 1;
        autoMoving = true;
        while (startingTileIndex < allPaths[finishTile].takenPath.Count)
        {

            
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = allPaths[finishTile].takenPath[startingTileIndex].tileId;
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
            playerSticks[i].transform.position = new Vector3(playerSticks[i].transform.position.x + intIndex % 2, playerSticks[i].transform.position.y, playerSticks[i].transform.position.z - 2 + .3f * stagger);
                        
            xoffset += 1;
            

        }
    }
}

[System.Serializable]
public class PathWrapper
{
    [SerializeField] public List<TileScript> takenPath;

    public PathWrapper(List<TileScript> path) { takenPath = path; }
}
