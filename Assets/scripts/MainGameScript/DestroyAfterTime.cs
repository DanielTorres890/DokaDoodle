using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float duration;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if(duration < 0 )
        Destroy(gameObject);
    }
}
