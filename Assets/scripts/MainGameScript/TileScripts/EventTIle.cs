using Unity.Netcode;
using UnityEngine;

public class EventTIle : TileScript
{
    public EventBase[] tileEvent;
    public override void TileEvent()
    {

        if (!NetworkManager.Singleton.IsServer) { return; }

        ClientChecks.Instance.SyncEventRpc(0);
    }
}
