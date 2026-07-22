using Unity.Netcode;
using UnityEngine;

public class SpawnEvent : NetworkBehaviour
{
    [SerializeField]private GameObject spawnedObject;
    public void SpawnObject(AnimationEvent bello)
    {
        GameObject objectToSpawnPrefab = bello.objectReferenceParameter as GameObject;

        if (objectToSpawnPrefab != null)
        {
            // Instantiate the prefab at the current position and rotation of this GameObject
            if (spawnedObject != null) { Destroy(spawnedObject); }

            spawnedObject = Instantiate(objectToSpawnPrefab, transform);
            spawnedObject.transform.localPosition = new Vector3(0, 0, bello.floatParameter);
        }
        else
        {
            Debug.LogWarning("Object to Spawn Prefab is not assigned in AnimationSpawner script.");
        }
    }
    public void DestroySpawnedObject()
    {
        if(IsHost) { DestroySpawnedObjectRpc(); }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void DestroySpawnedObjectRpc()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
        }
    }
}
