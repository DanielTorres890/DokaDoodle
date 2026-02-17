using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneChanger : NetworkBehaviour
{
   
    [SerializeField]private int loadedPlayers = 0;
    private bool LoadComplete;
    public Image fadeInOut;
    public float fadeInTime;
    public float fadeOutTime;
    public SceneEventProgressStatus status;
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
        ResetYoStuffRpc(sceneName);
        
        
    }
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void loadClientScenesAddidtiveRpc(string sceneName)
    {
        if (sceneName == "Fake") { return; }

        Debug.Log("I am NOT finished loading yet ");
        LoadComplete = false;
        
        loadedPlayers = 0;
        ResetYoStuffAddRpc(sceneName);
        status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {sceneName} " +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }

    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void UnloadClientScenesRpc(string sceneName)
    {
        
        NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ResetYoStuffRpc(string scenename)
    {
        loadedPlayers = 0;
        StartCoroutine(FadeIn(scenename, LoadSceneMode.Single));
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void ResetYoStuffAddRpc(string scenename)
    {
        loadedPlayers = 0;
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsHost)
        NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;

        
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("I finished loading this scene " +sceneName);

        LoadComplete = true;
        status = SceneEventProgressStatus.None;
        FadeOutRpc();
    }


    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void FadeOutRpc()
    {
        StartCoroutine(FadeOut());
    }

    public bool everyoneLoaded()
    {
        return LoadComplete;
    }
   
    IEnumerator FadeIn(string sceneName, LoadSceneMode loadmode)
    {
        float t = 0f;
        Color c = fadeInOut.color;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            c.a = t/fadeOutTime;
            fadeInOut.color = c;
            yield return null;
        }
        if(IsHost)
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, loadmode);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {sceneName} " +
                      $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        }
    }
    IEnumerator FadeOut()
    {
        float t = 0f;
        Color c = fadeInOut.color;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            c.a = 1f - (t / fadeOutTime);
            fadeInOut.color = c;
            yield return null;
        }
        
    }
}
