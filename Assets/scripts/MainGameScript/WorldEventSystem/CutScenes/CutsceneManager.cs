using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CutsceneManager : NetworkBehaviour
{

    public DialogueScript dialogueBox;
  

    public override void OnNetworkSpawn()
    {

        if(IsHost) { StartCoroutine(awaitClients()); }
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void BeginCutsceneRPC()
    {
        if(WorldEventManager.Instance.currentCutscene.backgroundMusic)
        BGMManager.instance.PlaySound(WorldEventManager.Instance.currentCutscene.backgroundMusic);
        WorldEventManager.Instance.days -= 1;
        dialogueBox.whoInControl = NetworkData.Instance.currentPlayer;
        GameObject background = Instantiate(WorldEventManager.Instance.currentCutscene.cutsceneBackground);

        //not my favorite work around but like it is what it is
        if(background.TryGetComponent(out NetworkObject component))
        {
            if(IsHost)
            component.Spawn();
        }
        dialogueBox.lines = new List<string>(WorldEventManager.Instance.currentCutscene.dialogue);
        dialogueBox.delays = new List<float>(WorldEventManager.Instance.currentCutscene.delays);
        WorldEventManager.Instance.currentCutscene = null;
        dialogueBox.gameObject.SetActive(true);
        dialogueBox.startDialogue();

    }


    private IEnumerator awaitClients()
    {

        while(!SceneChanger.Instance.everyoneLoaded())
        {
            yield return null;
        }
        BeginCutsceneRPC();
    }
}
