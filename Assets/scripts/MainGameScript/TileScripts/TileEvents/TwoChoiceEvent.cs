using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TwoChoiceEvent : EventBase
{
    public GameObject button;

    public List<string> DoDialogue;
    public List<string> DoNotDialogue;
    public override void FireEvent()
    {
        TileEventManager.Instance.buttons = TileEventManager.Instance.createOptions(button);
        TileEventManager.Instance.buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = "DO";
        TileEventManager.Instance.buttons[1].GetComponentInChildren<TextMeshProUGUI>().text = "DO NOT";
         
        TileEventManager.Instance.buttons[0].GetComponent<Button>().onClick.AddListener (delegate { TileEventManager.Instance.DoSomething(); });
     

        TileEventManager.Instance.buttons[1].GetComponent<Button>().onClick.AddListener(delegate { TileEventManager.Instance.DoNothing(); });

    }

    public override void SetUpBg()
    {
        
    }

    public abstract void doSomething();

    public abstract void doNothing();

    
}
