using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileEventManager : NetworkBehaviour
{
    public static TileEventManager Instance;
    [SerializeField] private GameObject dialogue;

    public DialogueScript dialogueScript;
    // Start is called before the first frame update
    public void Awake()
    {

        if (Instance == null)
            Instance = this;

        dialogueScript = dialogue.GetComponent<DialogueScript>();
        dialogue.SetActive(true);
        dialogueScript.lines = new List<string>(NetworkData.Instance.currentEvent.dialouge);
        dialogueScript.Awake();
        StartCoroutine(completeEvent());
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
        dialogueScript.Awake();
        StartCoroutine(additionalDialogue());
    }
    private IEnumerator additionalDialogue()
    {
        while (dialogue.activeSelf)
        {
            yield return null;
        }
        SceneChanger.Instance.loadClientScenesServerRpc("MainGameScene");
    }

    public List<GameObject> createOptions(GameObject button, int num = 2)
    {
        var options = new List<GameObject>();
        for (int i = 0; i < num; i++)
        {
            options.Add(Instantiate(button));
        }
        return options;
    }
}
