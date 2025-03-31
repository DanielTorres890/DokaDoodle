using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeMover : NetworkBehaviour
{
    public float speed;

    public Vector2 move;
    public TileScript baseTile;


    public void moveAround(InputAction.CallbackContext action)
    {

        move = action.action.ReadValue<Vector2>();
    }

    public void Update()
    {
        if(!IsOwner || !gameObject.activeSelf) { return; }
        transform.position += new Vector3(move.x * speed,0,move.y * speed);

    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("I found something");
        if (!gameObject.activeSelf) { return; }
        if (other.gameObject.TryGetComponent(out baseTile))
        {
            Debug.Log("I FOUND U");
            
        }
    }
    public int ThisOne()
    {
        if(baseTile != null) { return baseTile.tileId; }

        return -1;
    }

   

}
