
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event Object", menuName = "Events/MaybeGold")]
public class GoldOrNot : TwoChoiceEvent
{
    [SerializeField] private int MoneyToChange;
    [SerializeField] private List<string> failDoDialogue;
    public override void doNothing()
    {
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines = new List<string>(base.DoNotDialogue);
        TileEventManager.Instance.EndEvent();

    }

    public override void doSomething()
    {
        TileEventManager.Instance.rollRandom(3);
       
        
       

    }

    public override void RandomPassBack()
    {
        if (TileEventManager.Instance.rando == 0)
        {
            TileEventManager.Instance.dialogueScript.lines = new List<string>(failDoDialogue);
        }
        else
        {
            TileEventManager.Instance.dialogueScript.lines = new List<string>(base.DoDialogue);
        }
        TileEventManager.Instance.EndEvent();
    }
}
