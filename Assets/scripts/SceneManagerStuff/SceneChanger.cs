using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : NetworkBehaviour
{
   
    private int loadedPlayers = 0;

    public static SceneChanger Instance { get; set; }
    private void Awake()
    {
        Instance = this;
        
        
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void loadClientScenesServerRpc(string sceneName)
    {
        
        if (sceneName == "Fake") { return;  }
       
        ResetYoStuffRpc();
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ResetYoStuffRpc()
    {
        loadedPlayers = 0;
        Debug.Log("ResetLoaded ");
    }
   
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.SceneManager.OnLoadComplete += OnSceneLoaded;

        
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        loadedPlayers += 1;
        Debug.Log("Loaded PLayers " + loadedPlayers);
    }

   public bool everyoneLoaded()
    {
        return loadedPlayers >= NetworkData.Instance.playerCount;
    }
}
