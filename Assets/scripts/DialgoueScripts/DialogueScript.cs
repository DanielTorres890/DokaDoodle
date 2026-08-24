using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Events;
using PrimeTween;

public class DialogueScript : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private Image background;
    [SerializeField] public List<string> lines;
    [SerializeField] public List<float> delays;
    public string nextScene = "Fake";
    [DoNotSerialize]public int whoInControl = 0;

    [SerializeField] float textSpeed;
    [SerializeField] bool startShown;
    public UnityEvent endEvent;
    public UnityEvent<int> onLineFinish;
    private int index;

    //if you're wondering what this is about its bc if make the string visible 1 at a time it excludes color codes </color=blue>
    //which is a problem since those ARE counted as a part of the string length so it makes you HAVE to click twice on any dialgoue
    //with colored text so i use this to keep track of every character not part of the actual displayed string
    private int charsToIgnore;
    public void Awake()
    {
        endEvent = new UnityEvent();
        startDialogue();
        
    }
    public override void OnNetworkSpawn()
    {
        var button = GetComponentInChildren<Button>();
        button.onClick.AddListener(delegate { contCutsceneServerRpc(); });
        if (!startShown)
        {
            gameObject.SetActive(false);
            StopAllCoroutines();
        }
        if (nextScene != "Fake")
        {
            if (IsHost)
                endEvent.AddListener(delegate { SceneChanger.Instance.loadClientScenesServerRpc(nextScene); });
        }
        else
        {
            endEvent.AddListener(delegate { gameObject.SetActive(false); });
        }
    }

    // Update is called once per frame



    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void contCutsceneServerRpc(RpcParams rpcstuff = default)
    {

        
        if (!NetworkData.Instance.IsAllowed(whoInControl,rpcstuff.Receive.SenderClientId)) { return; }
        contCutsceneClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void contCutsceneClientRpc()
    {
        if (textComponent.maxVisibleCharacters >= lines[index].Length - 1 - charsToIgnore)
        {
            
            NextLine();
        }
        else
        {
            
            StopAllCoroutines();
            textComponent.maxVisibleCharacters = lines[index].Length;
        }
    }
    public void startDialogue ()
    {
        StopAllCoroutines();

        textComponent.text = lines[0];
        textComponent.maxVisibleCharacters = 0;
        charsToIgnore = 0;

        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        

        while (textComponent.maxVisibleCharacters < lines[index].Length - charsToIgnore)
        {
            textComponent.maxVisibleCharacters += 1;
            if(textComponent.text[textComponent.maxVisibleCharacters + charsToIgnore - 1] == '<')
            {
                while (textComponent.text[textComponent.maxVisibleCharacters + charsToIgnore - 1] != '>')
                {
                    charsToIgnore += 1;
                }
            }
            
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        onLineFinish.Invoke(index);
        if(delays.Count > index && delays[index] > 0) 
        { 
            Tween.Delay(delays[index], BeginLine);
            gameObject.SetActive(false);
            return;
        }


        BeginLine();
    }
    void BeginLine()
    {
        
        if (index < lines.Count - 1)
        {
            gameObject.SetActive(true);
            index++;
            textComponent.text = lines[index];
            textComponent.maxVisibleCharacters = 0;
            charsToIgnore = 0;
            StartCoroutine(TypeLine());
        }
        else
        {


            if (NetworkData.Instance.IsAllowed(whoInControl, NetworkManager.Singleton.LocalClientId))
                EndEventRpc();

        }
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void EndEventRpc()
    {
        gameObject.SetActive(false);
        endEvent.Invoke();
    }
}
