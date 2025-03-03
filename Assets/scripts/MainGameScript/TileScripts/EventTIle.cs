using Unity.Netcode;
using UnityEngine;

public class EventTIle : DefaultTile
{
    
    public override void TileEvent()
    {

        if (!NetworkManager.Singleton.IsServer) { return; }

        ClientChecks.Instance.SyncEventRpc(0);
    }
}
