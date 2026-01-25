using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveManager : NetworkBehaviour
{

    [SerializeField] public List<TileScript> mapTiles = new List<TileScript>();

    public TMP_Text rollNum;

    private List<GameObject> takenPath = new List<GameObject>();
    public List<GameObject> playerSticks = new List<GameObject>();
    private List<Animator> stickAnimators = new List<Animator>();

    public int mapNumber;
    public CinemachineCamera playerCam;
    public GameObject gameMenu;

    public bool canMove = false;
    public bool cameraMove = false;

    [Tooltip("DEBUG OPTION forces a number to be rolled")]
    public bool forceRoll;
    [Tooltip("if forceroll is on this is the number to be rolled")]
    public int forcedRollNum;
    

    int diceRoll = 0;

    [SerializeField] private float cameraSpeed = 5f;
    [SerializeField] private float moveSpeed = 500f;
    [Tooltip("When enemies are visually spawned on the overworld this is how far apart they'll be ")]
    public Vector2 enemyDistance;

    public AudioClip BGM;
    private Vector3 cameraMoveDirection;

    public static PlayerMoveManager Instance;
    public void Awake()
    {
        
        for(int i = 0; i < mapTiles.Count; i++)
        {
            mapTiles[i].tileId = i;
        }
        Instance = this;

        if(BGM) { BGMManager.instance.PlaySound(BGM); }
        
        
       
        
        setUpTileEnemies();
        playerSticks = GameObject.FindGameObjectWithTag("Data").GetComponent<NetworkData>().playerSticks;
        
        float xoffset = 0;
        int stagger = 1;
        for (int i = 0; i < NetworkData.Instance.players.Count; i++)
        {

           
            stickAnimators.Add(playerSticks[i].GetComponent<Animator>());
            playerSticks[i].transform.position = mapTiles[NetworkData.Instance.players[i].curTileId].transform.position;
            playerSticks[i].transform.position = new Vector3(playerSticks[i].transform.position.x + xoffset, playerSticks[i].transform.position.y, playerSticks[i].transform.position.z - 2 + .3f * stagger);
            xoffset += 1;
            stagger *= -1;
          
        }
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
        for(int i = 0; i < rollMultiplier; i++)
        {
            int randomNum = UnityEngine.Random.Range(0, 100);

            if (randomNum <= 3) { diceRoll = 0; }

            else { diceRoll = Convert.ToInt32(Math.Ceiling(randomNum / 14f)); }
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
        if(forceRoll)
        {
            SyncDiceRollServerRpc(forcedRollNum);//can force die roll with this
        }
        else
        {
            SyncDiceRollServerRpc(totalRoll); 
        }
        
        takenPath.Clear();
        mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].playersOnTile[NetworkData.Instance.currentPlayer] = false;
        takenPath.Add(mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].gameObject);
        
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

        if(!canMove) { return; }

        if (diceRoll <= 0)
        {

            
            canMove = false;
            SyncPlayerTileServerRpc(NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
            SetNextTurnServerRpc();

        }
    }
   

    
    public void MovePlayer(InputAction.CallbackContext action )
    {   
        
        if (!canMove) { return; }
        if (cameraMove) { return;}
        //just as a note to self taken path defaults to your current tile being in there after a roll so its always at least 1
        if (takenPath.Count <= 1 && diceRoll <= 0) { return; };

            var direction = action.action.ReadValue<Vector2>();
        var curTile = mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];

       
        if (direction == Vector2.up && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].upTile != null)
        {

          
            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.upTile)) && diceRoll > 0)
            {
                
                diceRoll--;
                takenPath.Add(curTile.upTile);

            }
            else if (takenPath[takenPath.Count - 2] == curTile.upTile)
            {
                diceRoll++;
                takenPath.RemoveAt(takenPath.Count - 1);
            }
            else { return; }
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = curTile.upTile.GetComponent<TileScript>().tileId;

        }
        if(direction == Vector2.down && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].downTile != null)
        {
            
            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.downTile)) && diceRoll > 0)
            {
                diceRoll--;
                takenPath.Add(curTile.downTile);

            }
            else if (takenPath[takenPath.Count - 2] == curTile.downTile)
            {
                diceRoll++;
                takenPath.RemoveAt(takenPath.Count - 1);
            }
            else { return; }
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = curTile.downTile.GetComponent<TileScript>().tileId;
        }

        if(direction == Vector2.right && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].rightTile != null)
        {
            
            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.rightTile)) && diceRoll > 0)
            {
                diceRoll--;
                takenPath.Add(curTile.rightTile);

            }
            else if (takenPath[takenPath.Count - 2] == curTile.rightTile)
            {
                diceRoll++;
                takenPath.RemoveAt(takenPath.Count - 1);
            }
            else { return; }
            NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = curTile.rightTile.GetComponent<TileScript>().tileId;
        }

        if(direction == Vector2.left && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].leftTile != null)
        {
            
            if (((takenPath.Count <= 1 && diceRoll > 0) || (takenPath[takenPath.Count - 2] != curTile.leftTile)) && diceRoll > 0)
            {
                diceRoll--;
                takenPath.Add(curTile.leftTile);

            }
            else if (takenPath[takenPath.Count - 2] == curTile.leftTile)
            {
                diceRoll++;
                takenPath.RemoveAt(takenPath.Count - 1);
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
        if (!canMove || takenPath.Count < 2 || !action.performed) { return; }

        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = takenPath[takenPath.Count-2].GetComponent<TileScript>().tileId;
        
        diceRoll++;
        takenPath.RemoveAt(takenPath.Count-1);
        SyncDiceRollServerRpc(diceRoll);
        ClientChecks.Instance.rollNum.text = diceRoll.ToString();
        StopAllCoroutines();
        StartCoroutine(playerMover(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId));
        PlayerMoverRpc(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SyncPlayerTileServerRpc (int id)
    {
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = id;
    }



    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void PlayerMoverRpc(float speed, int tildid, RpcParams rpcstuff = default)
    {
        if (rpcstuff.Receive.SenderClientId == NetworkManager.Singleton.LocalClientId) { return; }

        StopAllCoroutines();
       
        StartCoroutine(playerMover(speed, tildid));
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
        else {  mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].TileEvent(); }
        
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void NextTurnRpc()
    {

        NetworkData.Instance.setNextTurnNum();
        ClientChecks.Instance.PreturnStuff();
        

    }

    private IEnumerator playerMover(float speed, int tildId)
    {

        while (Vector3.Distance(playerSticks[NetworkData.Instance.currentPlayer].transform.position, mapTiles[tildId].gameObject.transform.position) > 0.01f )
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
            enemy.transform.position = new Vector3(enemy.transform.position.x + (enemyDistance.x * (i % (enemies.Count/2 + 1))), enemy.transform.position.y,enemy.transform.position.z+ (-enemyDistance.y * (i / ((enemies.Count / 2)+ 1))));
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

    public void FreeCamera()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!canMove) { return; }

        cameraMove = true;

        FreeMover.Instance.onUndoFree.AddListener(delegate { cameraMove = false; });
        FreeMover.Instance.FreeCamera();
    }
    public void UnfreeCamera()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!canMove) { return; }
        if(!cameraMove) { return; }

        FreeMover.Instance.EndFreeCamera();
    }


}
