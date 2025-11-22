using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float whenToDestroy;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        whenToDestroy -= Time.deltaTime;
        if( whenToDestroy < 0 )
        {
            Destroy(gameObject);
        }
    }
}
