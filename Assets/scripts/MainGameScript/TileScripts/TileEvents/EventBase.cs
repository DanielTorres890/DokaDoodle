using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class EventBase : ScriptableObject
{
    //random choice to make it list but i cant be bothered to go back and fix it rn
    public List<string> dialouge;
    public List<string> endDialouge;
    public string SceneToGoTo;


    public abstract void SetUpBg();
    public abstract void FireEvent();

    public abstract void RandomPassBack();

}
