using UnityEngine;

[CreateAssetMenu(fileName = "New Area Event", menuName = "WorldEvents/Temp Area Event")]
public class TemporaryAreaEvent : WorldEventBase
{
    public int eventDuration;
    public int mapId;
    public int tileId;

    public override void OnActivate(int randomNum)
    {

        int playerToSend = randomNum % NetworkData.Instance.players.Count;
        
        NetworkData.Instance.players[playerToSend].curMap = mapId;
        NetworkData.Instance.players[playerToSend].curTileId = tileId;
  
        base.OnActivate(randomNum);
    }
    public override bool Condition(int turns)
    {
        return eventDuration <= turns;
    }
    public override void OnDeactivate()
    {
        Debug.Log("did i deactivate? ");
        foreach (var player in NetworkData.Instance.players)
        {
            if (player.curMap == mapId)
            {
                player.curMap = 0;
                player.curTileId = 0;

            }
        }
        base.OnDeactivate();
    }

}
