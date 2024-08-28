using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveManager : NetworkBehaviour
{
    [SerializeField] private List<TileScript> mapTiles = new List<TileScript>();

    [SerializeField] private TMP_Text rollNum;

    private List<GameObject> takenPath = new List<GameObject>();
    private List<GameObject> playerSticks = new List<GameObject>();
    private List<Animator> stickAnimators = new List<Animator>();


    public GameObject playerCam;
    public GameObject gameMenu;

    public bool canMove = false;
    public NetworkVariable<bool> cameraMove = new NetworkVariable<bool>();

    int diceRoll = 0;

    [SerializeField] private float cameraSpeed = 5f;
    [SerializeField] private float moveSpeed = 500f;
    private Vector3 cameraMoveDirection;

    public static PlayerMoveManager Instance;
    private void Awake()
    {
        Instance = this;
        playerSticks = GameObject.FindGameObjectWithTag("Data").GetComponent<NetworkData>().playerSticks;

        int xoffset = 0;
        int stagger = 1;
        for(int i = 0; i < playerSticks.Count; i++)
        {
            stickAnimators.Add(playerSticks[i].GetComponent<Animator>());
            playerSticks[i].transform.position = mapTiles[NetworkData.Instance.players[i].curTileId].transform.position;
            playerSticks[i].transform.position = new Vector3(playerSticks[i].transform.position.x + xoffset, playerSticks[i].transform.position.y, playerSticks[i].transform.position.z + 60 * stagger);
            xoffset += 120;
            stagger *= -1;
            playerCam.transform.position = playerSticks[i].transform.position + new Vector3(95,797,-1054);
        }
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
        SyncDiceRollServerRpc(diceRoll);
        takenPath.Clear();
        takenPath.Add(mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].gameObject);
        
    }
    [ServerRpc (RequireOwnership = false)]
    private void SyncDiceRollServerRpc(int num)
    {
        stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", true);
        diceRoll = num;
        SyncDiceRollClientRpc(num);
    }
    [ClientRpc (RequireOwnership = false)]
    private void SyncDiceRollClientRpc(int num)
    {
        
        diceRoll = num;
        rollNum.text = diceRoll.ToString();
        
    }

    public void confirmMove(InputAction.CallbackContext action)
    {

        if(!canMove) { return; }

        if (diceRoll <= 0)
        {
            stickAnimators[NetworkData.Instance.currentPlayer].SetBool("Walking", false);
            canMove = false;
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
            SyncPlayerTileServerRpc(curTile.upTile.GetComponent<TileScript>().tileId);
          
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
            SyncPlayerTileServerRpc(curTile.downTile.GetComponent<TileScript>().tileId);
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
            SyncPlayerTileServerRpc(curTile.rightTile.GetComponent<TileScript>().tileId);
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
            SyncPlayerTileServerRpc(curTile.leftTile.GetComponent<TileScript>().tileId);
        }

        SyncDiceRollServerRpc(diceRoll);
        rollNum.text = diceRoll.ToString();
        PlayerMoverServerRpc(moveSpeed);
    }



    [ServerRpc(RequireOwnership = false)]
    private void SyncPlayerTileServerRpc (int id)
    {
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = id;
        SyncPlayerTileClientRpc(id);
    }
    [ClientRpc(RequireOwnership = false)]
    private void SyncPlayerTileClientRpc(int id)
    {
        NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId = id;
    }



    [ServerRpc (RequireOwnership = false)]
    private void PlayerMoverServerRpc(float speed)
    {
        
        StartCoroutine(playerMover(speed));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNextTurnServerRpc()
    {
        
        SetNextTurnClientRpc();
    }



    [ClientRpc(RequireOwnership =false)]
    private void SetNextTurnClientRpc()
    {
        mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].TileEvent();
        
        
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void NextTurnRpc()
    {
        if (NetworkData.Instance.currentPlayer < 3) { NetworkData.Instance.currentPlayer += 1; }

        else { NetworkData.Instance.currentPlayer = 0; }
        gameMenu.SetActive(true);
    }

    private IEnumerator playerMover(float speed)
    {

        while (Vector3.Distance(playerSticks[NetworkData.Instance.currentPlayer].transform.position, mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].gameObject.transform.position) > 0.01f )
        {
            playerSticks[NetworkData.Instance.currentPlayer].transform.position =
        Vector3.MoveTowards(playerSticks[NetworkData.Instance.currentPlayer].transform.position, mapTiles[NetworkData.Instance.players[NetworkData.Instance.currentPlayer].curTileId].gameObject.transform.position, speed * Time.deltaTime);
            yield return null;
        }
        
    }


}
