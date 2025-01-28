using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event Object", menuName = "Events/FreeStats")]
public class StatBuffEvent : EventBase
{
    [SerializeField] private GameObject entity;

    [SerializeField] private ItemBuff[] buffs;
    public override void SetUpBg()
    {
        
    }
    public override void FireEvent()
    {
    
        TileEventManager.Instance.dialogueScript.lines.Clear();
        TileEventManager.Instance.dialogueScript.lines.Add("Imagine u got some juice");
        string textToAdd = "You Gained ";
        if (buffs.Length > 0)
        {
            Debug.Log("SHOULD GAINS STATS?");
            foreach (ItemBuff buff in buffs)
            {
                NetworkData.Instance.players[NetworkData.Instance.currentPlayer].stats[buff.attribute] += buff.value;
                textToAdd += buff.value + " " + NetworkData.Instance.attributeStrings[buff.attribute];
                if (buff != buffs[buffs.Length - 1])
                {
                    textToAdd += ", ";
                }
            }

        }
        else
        {
            //In the future add randomized buffs incase its empty
        }
        TileEventManager.Instance.dialogueScript.lines.Add(textToAdd);

        TileEventManager.Instance.EndEvent();
    }

}
