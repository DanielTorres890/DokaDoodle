using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TileEventManager : NetworkBehaviour
{
    public static TileEventManager Instance;
    [SerializeField] private GameObject dialogue;

    private DialogueScript DialogueScript;
    // Start is called before the first frame update
    public void Awake()
    {

        if (Instance == null)
            Instance = this;

        DialogueScript = dialogue.GetComponent<DialogueScript>();
        dialogue.SetActive(true);
        DialogueScript.lines = NetworkData.Instance.currentEvent.dialouge;
        DialogueScript.Awake();
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
}
