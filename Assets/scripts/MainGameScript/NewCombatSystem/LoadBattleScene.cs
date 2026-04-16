using System.Collections;
using UnityEngine;

public class LoadBattleScene : MonoBehaviour
{
    public float spawnRadius = 30f;
    void Start()
    {
        PlayerCombatManager.Instance.spawnRadius = spawnRadius;
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
