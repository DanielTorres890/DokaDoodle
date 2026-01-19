using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TileEventManager : NetworkBehaviour
{
    public static TileEventManager Instance;
    [SerializeField] private GameObject dialogue;
    public int rando;
    public List<GameObject> buttons = new List<GameObject>();
    public DialogueScript dialogueScript;

    [SerializeField] private float spaceBetweenButtons;
    [SerializeField] private float startButtonY;
    [SerializeField] private float startButtonX;
    [SerializeField] private AudioSource soundSource;
    [SerializeField] private bool inScene = false; // who are you

    // Start is called before the first frame update
    public void Awake()
    {

        if (Instance == null)
            Instance = this;

        dialogueScript = dialogue.GetComponent<DialogueScript>();
        soundSource = GetComponent<AudioSource>();
        
        dialogue.SetActive(true);
        dialogueScript.whoInControl = NetworkData.Instance.currentPlayer;
        dialogueScript.lines = new List<string>(NetworkData.Instance.currentEvent.dialouge);
        dialogueScript.Awake();
        StartCoroutine(completeEvent());

        if (NetworkData.Instance.currentEvent.backgroundMusic)
        {
            soundSource.resource = NetworkData.Instance.currentEvent.backgroundMusic;
            soundSource.volume = SettingsManager.instance.volume;
            SettingsManager.instance.onBackgroundVolumeChange.AddListener(delegate { soundSource.volume = SettingsManager.instance.volume; });
            soundSource.Play();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator completeEvent()
    {
        while (dialogue.activeSelf)
        {
            yield return null;
        }


        NetworkData.Instance.currentEvent.FireEvent();
    }

    public void EndEvent()
    {
        dialogue.SetActive(true);
        dialogueScript.startDialogue();
        StartCoroutine(additionalDialogue());
    }
    private IEnumerator additionalDialogue()
    {
        while (dialogue.activeSelf)
        {
            yield return null;
        }
        NetworkData.Instance.setNextTurnNum();
        SceneChanger.Instance.loadClientScenesServerRpc("MainGameUI");
    }

    public List<GameObject> createOptions(GameObject button, int num = 2)
    {
        var options = new List<GameObject>();
        for (int i = 0; i < num; i++)
        {
            var temp = Instantiate(button, Vector3.zero, Quaternion.identity, dialogue.transform.parent);
            
            options.Add(temp);
            temp.transform.position = new Vector3(startButtonX, startButtonY - (spaceBetweenButtons * i));
        }
        return options;
    }
    public void rollRandom(int amount)
    {
        if (!NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
            return;
        RandomSyncRpc(Random.Range(0, amount));
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void RandomSyncRpc(int num)
    {
        rando = num;
        NetworkData.Instance.currentEvent.RandomPassBack();
    }

    public void DoNothing()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            DoNothingRpc();
        }
    }
    public void DoSomething()
    {
        if (NetworkData.Instance.IsAllowed(NetworkData.Instance.currentPlayer, NetworkManager.Singleton.LocalClientId))
        {
            DoSomethingRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void DoNothingRpc()
    {
        (NetworkData.Instance.currentEvent as TwoChoiceEvent).doNothing();
        DestroyButtons();
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void DoSomethingRpc()
    {
        (NetworkData.Instance.currentEvent as TwoChoiceEvent).doSomething();
        DestroyButtons();
    }
   
    private void DestroyButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Destroy(buttons[i]);
        }
        buttons.Clear();
    }
}
