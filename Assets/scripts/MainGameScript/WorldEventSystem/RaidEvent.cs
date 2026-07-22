using PrimeTween;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Money Event", menuName = "WorldEvents/RaidEvent")]
public class RaidEvent : WorldEventBase
{
    public RaidInfo[] raids;
    public override void OnActivate()
    {
        

        int statTotal = 0;
        foreach(var player in NetworkData.Instance.players)
        {

            player.loyaltyTags.Add("Raid");
            foreach(var stat in player.stats)
            {
                if(stat.Key == Attributes.MaxHealth) { statTotal += stat.Value / 10; }
                else if( stat.Key != Attributes.Health)
                    statTotal += stat.Value;
            }
            foreach(var member in player.partyMembers)
            {
                member.loyaltyTags.Add("Raid");
            }

        }
        float normalized = Mathf.InverseLerp(0, 25, statTotal);
        
        normalized = Mathf.Clamp(0, raids.Length - 1, normalized);
        int raidIndex = Mathf.FloorToInt(normalized);

     
        foreach (var player in NetworkData.Instance.players)
        {
            while(player.isDead)
            {
                player.progressDeath();
            }
            player.TeleportPlayer(raids[raidIndex].mapId, raids[raidIndex].tileId);
            player.stats[Attributes.Health] = player.stats[Attributes.MaxHealth];
         
        }
        PlayerCombatManager.Instance.isRaid = true;
        if(NetworkManager.Singleton.IsHost)
        {
            Tween.Delay(0.25f, delegate { ClientChecks.Instance.SyncEnemyRpc(PlayerCombatManager.Instance.EnemyEncounterDataBase.GetId[raids[raidIndex].enemyEncounter]); });
        }
        


        base.OnActivate();
    }
    public override void OnDeactivate()
    {
        PlayerCombatManager.Instance.isRaid = false;
        foreach (var player in NetworkData.Instance.players)
        {
            player.loyaltyTags.Remove("Raid");
            foreach (var member in player.partyMembers)
            {
                member.loyaltyTags.Remove("Raid");
            }
        }
            base.OnDeactivate();
    }
    public override bool Condition(int turns)
    {
        return true;
    }
}

[System.Serializable]
public class RaidInfo
{
    public int tileId;
    public int mapId;
    public EnemyEncounter enemyEncounter;
}
