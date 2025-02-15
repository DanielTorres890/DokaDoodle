using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class fricku : MonoBehaviour
{
    [SerializeField] private GameObject ihateu;
    
    void Start()
    {
      
        if (!NetworkManager.Singleton.IsServer) { return; }
        
        //frick u
    }
    public void DoSomething()
    {
        var temp = Instantiate(ihateu);
        temp.GetComponent<NetworkObject>().Spawn();
    }
    
}
