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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void loadClientScenesServerRpc(string sceneName)
    {
        
        if (sceneName == "Fake") { return;  }
        LoadComplete = false;
        
        ResetYoStuffRpc(sceneName);
        
        
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void loadClientScenesAddidtiveRpc(string sceneName)
    {
        if (sceneName == "Fake") { return; }

       
        LoadComplete = false;
        
 
        ResetYoStuffAddRpc(sceneName);
        status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {sceneName} " +
                  $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }

    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UnloadClientScenesRpc(string sceneName)
    {
        
        NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ResetYoStuffRpc(string scenename)
    {
      
        StartCoroutine(FadeIn(scenename, LoadSceneMode.Single));
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void ResetYoStuffAddRpc(string scenename)
    {

        
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


    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void FadeOutRpc()
    {
        StartCoroutine(FadeOut());
        LoadComplete = true;
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
