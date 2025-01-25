using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event Object", menuName = "Events/FreeMoney")]
public class StatBuffEvent : EventBase
{
    [SerializeField] private GameObject entity;
    public override void SetUpBg()
    {
        
    }
    public override void FireEvent()
    {
        Debug.Log("WE DOIN STUFF");
    }

}
