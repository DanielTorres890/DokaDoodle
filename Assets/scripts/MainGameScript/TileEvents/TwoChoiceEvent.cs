using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class TwoChoiceEvent : EventBase
{
    public GameObject button;
    public List<GameObject> buttons = new List<GameObject>();
    public override void FireEvent()
    {
        buttons = TileEventManager.Instance.createOptions(button);
        buttons[0].GetComponent<Button>().onClick.AddListener (delegate { doSomething(); });
        buttons[1].GetComponent<Button>().onClick.AddListener(delegate { doNothing(); });
    }

    public override void SetUpBg()
    {
        
    }

    public abstract void doSomething();

    public abstract void doNothing();
}
