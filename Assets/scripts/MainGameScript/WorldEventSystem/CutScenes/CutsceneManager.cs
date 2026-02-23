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

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void BeginCutsceneRPC()
    {
        
        dialogueBox.whoInControl = NetworkData.Instance.currentPlayer;
        Instantiate(WorldEventManager.Instance.currentCutscene.cutsceneBackground);
        dialogueBox.lines = new List<string>(WorldEventManager.Instance.currentCutscene.dialogue);
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
