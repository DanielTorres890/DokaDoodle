using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Burst.Intrinsics.X86.Avx;

public class PlayerMoveManager : NetworkBehaviour
{

    [SerializeField] public List<TileScript> mapTiles = new List<TileScript>();

    [SerializeField] private TMP_Text rollNum;

    private List<GameObject> takenPath = new List<GameObject>();
    private List<GameObject> playerSticks = new List<GameObject>();
    private List<Animator> stickAnimators = new List<Animator>();

    public int mapNumber;
    public GameObject playerCam;
    public GameObject gameMenu;

    public bool canMove = false;
    public NetworkVariable<bool> cameraMove = new NetworkVariable<bool>();

    int diceRoll = 0;

    [SerializeField] private float cameraSpeed = 5f;
    [SerializeField] private float moveSpeed = 500f;
    private Vector3 cameraMoveDirection;

    public static PlayerMoveManager Instance;
    public override void OnNetworkSpawn()
    {
        
        Instance = this;

        Debug.Log("Setting up player " + NetworkData.Instance.currentPlayer);
        setUpTileEnemies();
        playerSticks = GameObject.FindGameObjectWithTag("Data").GetComponent<NetworkData>().playerSticks;

        int xoffset = 0;
        int stagger = 1;
        for (int i = 0; i < NetworkData.Instance.players.Count; i++)
        {


            stickAnimators.Add(playerSticks[i].GetComponent<Animator>());
            playerSticks[i].transform.position = mapTiles[NetworkData.Instance.players[i].curTileId].transform.position;
            playerSticks[i].transform.position = new Vector3(playerSticks[i].transform.position.x + xoffset, playerSticks[i].transform.position.y, playerSticks[i].transform.position.z + 60 * stagger);
            xoffset += 120;
            stagger *= -1;
            playerCam.transform.position = playerSticks[i].transform.position + new Vector3(95, 797, -1054);
        }

        FightOrNot();



    }
    
    private void Update()
    {
        if (!cameraMove.Value) { playerCam.transform.position = playerSticks[NetworkData.Instance.currentPlayer].transform.position + new Vector3(95, 797, -1054);  }
        
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

            stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", false);
            canMove = false;
            SyncPlayerTileServerRpc(NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
            SetNextTurnServerRpc();

        }
    }
    public void cameraMoverEnable(InputAction.CallbackContext action)
    {
        if(!canMove) { return; }
        cameraMoverEnableServerRpc();
        canMove = false;
    }
    [ServerRpc (RequireOwnership = false)]
    private void cameraMoverEnableServerRpc ()
    {
        cameraMove.Value = true;
    }

    public void cameraMoverDisable(InputAction.CallbackContext action)
    {
        if (canMove) { return; };
        cameraMoverDisableServerRpc();
        canMove = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void cameraMoverDisableServerRpc()
    {
        cameraMove.Value = false;
    }
    public void cameraMover(InputAction.CallbackContext action)
    {
        if(!cameraMove.Value) { return; }
        if (NetworkData.Instance.currentPlayer != Convert.ToInt32(NetworkManager.Singleton.LocalClientId) && !IsHost) { return; }
        cameraMoverServerRpc(action.action.ReadValue<Vector3>());
    }
    [ServerRpc(RequireOwnership = false)]
    public void cameraMoverServerRpc(Vector3 action)
    {
        cameraMoveDirection = action;
    }
    public void MovePlayer(InputAction.CallbackContext action )
    {   
        
        if (!canMove) { return; }
        
        var curTile = mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId];

        
        if (action.action.ReadValue<Vector2>() == Vector2.up && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].upTile != null)
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
        if (action.action.ReadValue<Vector2>() == Vector2.down && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].downTile != null)
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

        if (action.action.ReadValue<Vector2>() == Vector2.right && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].rightTile != null)
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

        if (action.action.ReadValue<Vector2>() == Vector2.left && mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].leftTile != null)
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
        PlayerMoverServerRpc(moveSpeed, NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId);
    }



    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SyncPlayerTileServerRpc (int id)
    {
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = id;
  
    }



    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void PlayerMoverServerRpc(float speed, int tildid)
    {
        StopAllCoroutines();
        Debug.Log("IM TRYING TO MOVE TOWAREDS THIS" + tildid);
        StartCoroutine(playerMover(speed, tildid));
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SetNextTurnServerRpc()
    {

        rollNum.transform.parent.gameObject.SetActive(true);
        MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].players.Add(NetworkData.Instance.currentPlayer);
        Debug.Log("Something should happen?");
        mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].TileEvent();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void NextTurnRpc()
    {
        NetworkData.Instance.setNextTurnNum();
        FightOrNot();
        

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
                if (MapTileSpecialEvents.Instance.mapTiles[mapNumber][i].tileEnemy != null)
                {
                    var enemy = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[MapTileSpecialEvents.Instance.mapTiles[mapNumber][i].tileEnemy.enemyId].enemyPrefab);
                    enemy.transform.position = mapTiles[i].transform.position;
                    enemy.transform.position = new Vector3(enemy.transform.position.x - 120, enemy.transform.position.y, enemy.transform.position.z + 60);
                    enemy.transform.localScale = new Vector3(50, 50, 1);
                    Debug.Log("OVERWORLD ENEMY SPAWNED");
                }

            }

        }

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
