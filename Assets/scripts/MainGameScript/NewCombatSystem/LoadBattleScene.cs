using System.Collections;
using UnityEngine;

public class LoadBattleScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NetworkData.Instance.IsHost)
        {
            StartCoroutine(AwaitPlayers());
        }
    }

    private IEnumerator AwaitPlayers()
    {
        while(!SceneChanger.Instance.everyoneLoaded())
        {
            yield return null;
        }
        SceneChanger.Instance.loadClientScenesAddidtiveRpc("NewBattleArea");
    }
    
}
