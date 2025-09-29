using Unity.Netcode;
using UnityEngine;
[CreateAssetMenu(fileName = "New Event Object", menuName = "Events/TownEvent")]
public class TownEvent : EventBase
{
    public TownInfo townInfo;
    
    public override void FireEvent()
    {
        //wild bruh wtf
    }
    public override void RandomPassBack()
    {
        throw new System.NotImplementedException();
    }

    public override void SetUpBg()
    {
        throw new System.NotImplementedException();
    }


}
