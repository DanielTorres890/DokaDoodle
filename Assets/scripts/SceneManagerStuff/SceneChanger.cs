using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : NetworkBehaviour
{
   
    [SerializeField]private int loadedPlayers = 0;
    private bool LoadComplete;
    public static SceneChanger Instance { get; set; }
    private void Awake()
    {
        Instance = this;
        
        
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void loadClientScenesServerRpc(string sceneName)
    {
        
        if (sceneName == "Fake") { return;  }
        LoadComplete = false;
        loadedPlayers = 0;
        ResetYoStuffRpc();
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
        
    }
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void loadClientScenesAddidtiveRpc(string sceneName)
    {
        if (sceneName == "Fake") { return; }

        LoadComplete = false;
        
        loadedPlayers = 0;
        ResetYoStuffRpc();
        var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {sceneName} " +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }

    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void UnloadClientScenesRpc(string sceneName)
    {
        Debug.Log("I better not be happening or ill crash out");
        
        NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ResetYoStuffRpc()
    {
        loadedPlayers = 0;
        
    }
   
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;

        
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        LoadComplete = true;

    }

    

    public bool everyoneLoaded()
    {
        return LoadComplete;
    }
   
    
}
