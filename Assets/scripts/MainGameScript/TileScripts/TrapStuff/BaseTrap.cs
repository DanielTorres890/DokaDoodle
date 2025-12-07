using UnityEngine;

public abstract class BaseTrap : ScriptableObject
{
    public string ActivateText;
    public abstract void TrapEffect(playerData whom);

    public virtual void DeployTrap(int tileId)
    {
        if(tileId == -1) { return; }

        Debug.Log("LEGALLY DEPLOYED");
        if(!NetworkData.Instance.IsServer) { return; }

        ClientChecks.Instance.DeployTrapRpc(tileId, NetworkData.Instance.trapDataBase.GetId[this]);
        

    }
    public abstract string TrapString(playerData whom);
}
