using UnityEngine;

public class SpawnEvent : MonoBehaviour
{
    [SerializeField]private GameObject spawnedObject;
    public void SpawnObject(AnimationEvent bello)
    {
        GameObject objectToSpawnPrefab = bello.objectReferenceParameter as GameObject;

        if (objectToSpawnPrefab != null)
        {
            // Instantiate the prefab at the current position and rotation of this GameObject
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
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
        }
       
    }
}
