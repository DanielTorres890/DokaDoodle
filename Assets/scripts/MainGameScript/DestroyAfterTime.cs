using Unity.Netcode;
using UnityEngine;

public class DestroyAfterTime : NetworkBehaviour
{
    public float duration;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsHost) { return; }
        duration -= Time.deltaTime;
        if(duration < 0 )
        Destroy(gameObject);
    }
}
