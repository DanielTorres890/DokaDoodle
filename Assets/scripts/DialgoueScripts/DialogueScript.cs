using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class DialogueScript : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image background;
    [SerializeField] public List<string> lines;

    public string nextScene = "Fake";
    [DoNotSerialize]public int whoInControl = 0;

    [SerializeField] float textSpeed;
    [SerializeField] bool startShown;

    private int index;
    public void Awake()
    {
        whoInControl = NetworkData.Instance.currentPlayer;
        startDialogue();
    }
    public override void OnNetworkSpawn()
    {
        if(!startShown)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame


    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void contCutsceneServerRpc()
    {

        if (!NetworkData.Instance.IsAllowed(whoInControl,NetworkManager.Singleton.LocalClientId)) { return; }

        contCutsceneClientRpc();
    }

    [ClientRpc]
    private void contCutsceneClientRpc()
    {
        Debug.Log("Am I happening twice");
        if (textComponent.text == lines[index])
        {
            NextLine();
        }
        else
        {
            
            StopAllCoroutines();
            textComponent.text = lines[index];
        }
    }
    void startDialogue ()
    {
        StopAllCoroutines();
        textComponent.text = string.Empty;

        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Count -1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        } 
        else
        {
            gameObject.SetActive(false);
            background.gameObject.SetActive(false);
            if (!nextScene.Equals("Fake"))
            {
                SceneChanger.Instance.loadClientScenesServerRpc(nextScene);
            }
            else
            {
                gameObject.SetActive(false);
            }
            
        }
    }
}
