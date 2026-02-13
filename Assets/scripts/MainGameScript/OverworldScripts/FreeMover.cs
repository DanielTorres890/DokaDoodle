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
        var oldpos = transform.position;

        transform.position += new Vector3(move.x * speed,0,move.y * speed);

        if(Vector3.Distance(transform.position,oldpos) > needToMove)
        {
            SyncTranformRpc(transform.position);
        }

    }


    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
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
            Debug.Log("okay im trying to shoot u now");
            SelectTileRpc(baseTile.tileId);

        }
        
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SelectTileRpc(int tileId)
    {
        Debug.Log("TRAPPED");
        onTileSelect.Invoke(tileId);
    
    }
    public void FreeCamera()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }

        FreeCameraRpc();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void FreeCameraRpc(RpcParams paramys = default)
    {
        gameObject.SetActive(true);
        ClientChecks.Instance.mainMenuButtons.SetActive(false);
        ClientChecks.Instance.cameraControlDisplay.SetActive(true);
        gameObject.transform.position = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform.position;

        onBeginFree.Invoke();
        playerCam.Follow = gameObject.transform;
        if (IsServer)
        {
            gameObject.GetComponent<NetworkObject>().ChangeOwnership(paramys.Receive.SenderClientId);
        }

    }

    public void EndFreeCamera()
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId)) { return; }
        EndFreeCameraRpc();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void EndFreeCameraRpc(RpcParams paramys = default)
    {
        
        gameObject.SetActive(false);
        ClientChecks.Instance.cameraControlDisplay.SetActive(false);
        onUndoFree.Invoke();
        onTileSelect.RemoveAllListeners();
        onUndoFree.RemoveAllListeners();
        playerCam.Follow = NetworkData.Instance.playerSticks[NetworkData.Instance.currentPlayer].transform;
        if (IsServer)
        {
            gameObject.GetComponent<NetworkObject>().ChangeOwnership(0);
        }
    }


}
