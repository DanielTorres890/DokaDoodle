using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : NetworkBehaviour
{
   
    private NetworkVariable<int> loadedPlayers = new NetworkVariable<int>();

    public static SceneChanger Instance { get; set; }
    private void Awake()
    {
        Instance = this;
        loadedPlayers = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void loadClientScenesServerRpc(string sceneName)
    {
        if (sceneName == "Fake") { return;  }
       
        loadedPlayers.Value = 0;
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName,LoadSceneMode.Single);
    }
    

   
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loadedPlayers.Value += 1;
    }
   
}
