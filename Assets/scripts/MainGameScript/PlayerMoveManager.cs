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
using static Unity.Burst.Intrinsics.X86.Avx;

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



    int diceRoll = 0;

    [SerializeField] private float cameraSpeed = 5f;
    [SerializeField] private float moveSpeed = 500f;
    private Vector3 cameraMoveDirection;

    public static PlayerMoveManager Instance;
    public void Awake()
    {
        
        for(int i = 0; i < mapTiles.Count; i++)
        {
            mapTiles[i].tileId = i;
        }
        Instance = this;

        

       
        
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
  



    }
    
    private void Update()
    {
        if (!cameraMove) {  }
        
        else {  playerCam.transform.position += cameraMoveDirection * cameraSpeed * Time.deltaTime; }
    }




    public void rollDice()
    {
        if (NetworkData.Instance.currentPlayer != Convert.ToInt32(NetworkManager.Singleton.LocalClientId) && !IsHost) { return; }
        int randomNum = UnityEngine.Random.Range(0, 100);

        if (randomNum <= 3) { diceRoll = 1; }

        else { diceRoll = Convert.ToInt32(Math.Ceiling(randomNum / 14f));  }


        canMove = true;
        SyncDiceRollServerRpc(3);
        takenPath.Clear();
        mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].playersOnTile[NetworkData.Instance.currentPlayer] = false;
        takenPath.Add(mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].gameObject);
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SyncDiceRollServerRpc(int num)
    {
        stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", true);
        diceRoll = num;

        diceRoll = num;
        rollNum.text = diceRoll.ToString();
        rollNum.transform.parent.gameObject.SetActive(true);
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
        rollNum.text = diceRoll.ToString();
        StopAllCoroutines();
        StartCoroutine(playerMover(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId));
        PlayerMoverRpc(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
    }


    public void UndoMove(InputAction.CallbackContext action)
    {
        if (!canMove || takenPath.Count < 2 || !action.performed) { return; }

        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = takenPath[takenPath.Count-2].GetComponent<TileScript>().tileId;
        Debug.Log("who tf u think u is " + NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
        diceRoll++;
        takenPath.RemoveAt(takenPath.Count-1);
        SyncDiceRollServerRpc(diceRoll);
        rollNum.text = diceRoll.ToString();
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
        rollNum.transform.parent.gameObject.SetActive(true);
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
        ClientChecks.Instance.OnNetworkSpawn();
        

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

    private void setUpTileEnemies()
    {
        

        if (MapTileSpecialEvents.Instance.mapTiles[mapNumber] == null)
        {
            MapTileSpecialEvents.Instance.mapTiles[mapNumber] = new SpecialTileEventHold[mapTiles.Count];
            for (int i = 0; i < MapTileSpecialEvents.Instance.mapTiles[mapNumber].Length; i++)
            {
                MapTileSpecialEvents.Instance.mapTiles[mapNumber][i] = new SpecialTileEventHold();
            }
        }
        else
        {

            for (int i = 0; i < MapTileSpecialEvents.Instance.mapTiles[mapNumber].Length; i++)
            {
                if (MapTileSpecialEvents.Instance.mapTiles[mapNumber][i].tileEnemy.Count != 0)
                {
                    foreach (var enemies in MapTileSpecialEvents.Instance.mapTiles[mapNumber][i].tileEnemy)
                    {
                        spawnEnemyOverworld(i, enemies.enemyId);
                    }
                    
                }

            }

        }

    }

    public void spawnEnemyOverworld(int tileId, int enemyId)
    {
        var enemy = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[enemyId].enemyNonCombatPrefab);
        enemy.transform.position = mapTiles[tileId].transform.position;
        enemy.transform.position = new Vector3(enemy.transform.position.x - 0, enemy.transform.position.y, enemy.transform.position.z + 0);
        enemy.transform.localScale = new Vector3(1, 1, 1);
        Debug.Log("OVERWORLD ENEMY SPAWNED AT " + tileId);
    }
    private void FightOrNot()
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
    }

    
}
