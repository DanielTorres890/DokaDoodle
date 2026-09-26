using UnityEngine;

public class BirdSpawner : MonoBehaviour
{
    public GameObject birdPrefab;
    public Transform[] spawnPositions;
    public float spawnFrequency;
    public float variance;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            GameObject bird = Instantiate(birdPrefab);
            int randomPos = Random.Range(0, spawnPositions.Length);
            bird.transform.position = spawnPositions[randomPos].position;
            bird.transform.rotation = spawnPositions[randomPos].rotation;
            timer = spawnFrequency + Random.Range(-variance, variance);
        }
    }
}
