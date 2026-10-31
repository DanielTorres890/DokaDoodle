using System.Data;
using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class FreeMover : NetworkBehaviour
{
    public float speed;

    public Vector2 move;
    public TileScript baseTile;

    public CinemachineCamera playerCam;


    public UnityEvent onBeginFree;
    public UnityEvent<int> onTileSelect;
    public UnityEvent onUndoFree;

    public static FreeMover Instance;

    public float needToMove;

    public Vector3 followOffset;


    public Vector3 minPositions, maxPositions;

    private Vector3 oldpos;
    public void Awake()
    {
        if(Instance == null) { Instance = this; }


        onTileSelect = new UnityEvent<int>();
        

    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.SetActive(false);
    }
    public void moveAround(InputAction.CallbackContext action)
    {

        move = action.action.ReadValue<Vector2>();

    }

    public void Update()
    {
        if(!IsOwner || !gameObject.activeSelf) { return; }
       

        transform.position += new Vector3(move.x * speed,0,move.y * speed) * Time.deltaTime;
        
        if(transform.position.x < minPositions.x || transform.position.x > maxPositions.x) { transform.position -= new Vector3(move.x * speed, 0, move.y * speed) * Time.deltaTime; }

        if (transform.position.z < minPositions.z || transform.position.z > maxPositions.z) { transform.position -= new Vector3(move.x * speed, 0, move.y * speed) * Time.deltaTime; }


        if (Vector3.Distance(transform.position,oldpos) > needToMove)
        {
            oldpos = transform.position;
            SyncTranformRpc(transform.position);
        }

    }


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SyncTranformRpc(Vector3 newPos)
    {
        transform.position = newPos;
        
    }
    public void OnTriggerEnter(Collider other)
    {
        
        if (!gameObject.activeSelf) { return; }
        if (other.gameObject.TryGetComponent(out baseTile))
        {
          
            
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (!gameObject.activeSelf) { return; }
        if (other.gameObject.TryGetComponent(out TileScript no))
        {
            baseTile = null;

        }
    }
    public void SelectTile(InputAction.CallbackContext action)
    {

        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId) && IsOwner) { return; }
        if (baseTile != null && action.started)
        {
         
            SelectTileRpc(baseTile.tileId);

        }
        
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SelectTileRpc(int tileId)
    {
      
        onTileSelect.Invoke(tileId);
    
    }


    public void FreeCamera(TileArrowRules rule = TileArrowRules.None)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        FreeCameraRpc(rule);
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void FreeCameraRpc(TileArrowRules rule, RpcParams paramys = default)
    {
        gameObject.SetActive(true);
        ClientChecks.Instance.mainMenuButtons.SetActive(false);
        ClientChecks.Instance.cameraControlDisplay.SetActive(true);
        gameObject.transform.position = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform.position;

        onBeginFree.Invoke();
        playerCam.Follow = gameObject.transform;

        playerCam.GetComponent<CinemachineFollow>().FollowOffset += followOffset;
        if (IsServer)
        {
            gameObject.GetComponent<NetworkObject>().ChangeOwnership(paramys.Receive.SenderClientId);
        }
        if(rule == TileArrowRules.None) { return; }
        if(rule == TileArrowRules.AllEnemies) 
        {
            int index = 0;
            foreach(var tile in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber])
            {
                foreach(var enemy in tile.tileEnemy)
                {
                    if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
                    {
                        PlayerMoveManager.Instance.mapTiles[index].ArrowChange(true);
                        break;
                    }
                }
                foreach(var player in tile.players)
                {
                    var enemy = NetworkData.Instance.players[player];
                    if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
                    {
                        PlayerMoveManager.Instance.mapTiles[index].ArrowChange(true);
                        break;
                    }
                }
                foreach (var enemy in tile.partyMembers)
                {
                    if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
                    {
                        PlayerMoveManager.Instance.mapTiles[index].ArrowChange(true);
                        break;
                    }
                }
                index++;
            }
            
        }
    }

    public void EndFreeCamera(TileArrowRules rule = TileArrowRules.None)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        EndFreeCameraRpc(rule);
    }
    public void EndFreeCamera(InputAction.CallbackContext action)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        if (!action.started) { return; }
        EndFreeCameraRpc();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void EndFreeCameraRpc(TileArrowRules rule = TileArrowRules.None, RpcParams paramys = default)
    {

        gameObject.SetActive(false);
        ClientChecks.Instance.cameraControlDisplay.SetActive(false);
        onUndoFree.Invoke();
        onTileSelect.RemoveAllListeners();
        onUndoFree.RemoveAllListeners();
        playerCam.Follow = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform;

        playerCam.GetComponent<CinemachineFollow>().FollowOffset -= followOffset;

        if (IsServer)
        {
            gameObject.GetComponent<NetworkObject>().ChangeOwnership(0);
        }
        if (rule == TileArrowRules.None) { return; }
        if (rule == TileArrowRules.AllEnemies)
        {
            int index = 0;
            foreach (var tile in MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber])
            {
                foreach (var enemy in tile.tileEnemy)
                {
                    if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
                    {
                        PlayerMoveManager.Instance.mapTiles[index].ArrowChange(false);
                        break;
                    }
                }
                foreach (var player in tile.players)
                {
                    var enemy = NetworkData.Instance.players[player];
                    if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
                    {
                        PlayerMoveManager.Instance.mapTiles[index].ArrowChange(false);
                        break;
                    }
                }
                foreach (var enemy in tile.partyMembers)
                {
                    if (!enemy.loyaltyTags.Intersect(NetworkData.Instance.GetCurrentPlayer().loyaltyTags).Any())
                    {
                        PlayerMoveManager.Instance.mapTiles[index].ArrowChange(false);
                        break;
                    }
                }
                index++;
            }
        }
    }

}

public enum TileArrowRules
{
    None,
    AllEnemies

}
