using Unity.Netcode;
using UnityEngine;

public class SummonDespawn : AbilityBase
{
    private AbilityManager myManager;
    public override void OnNetworkSpawn()
    {
        myManager = GetComponent<AbilityManager>();
        
        
        
    }
    public new void Update()
    {
        if (!IsServer) { return; }
        if (lifespan < lifetimer)
        {
            DespawnRpc();
            Destroy(gameObject);

        }
        lifetimer += Time.deltaTime;
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void SpawnInRpc(int entityId)
    {
        EnemyCombat entity = new EnemyCombat(PlayerCombatManager.Instance.EnemyDataBase.GetItem[entityId]);
        entity.loyaltyTags.Clear();
        

        PlayerCombatManager.Instance.combatants.Add(entity);
        Debug.Log("I've been added");
        if (IsServer)
        {
            entity.loyaltyTags = ownerStats.loyaltyTags;
            myManager.UpdateStatsRpc(PlayerCombatManager.Instance.combatants.IndexOf(entity));
        }
        

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void DespawnRpc()
    {
        NewCombatManager.instance.fricku.Remove(gameObject);
        NewCombatManager.instance.allCombatants.Remove(myManager); 
        PlayerCombatManager.Instance.combatants.Remove(myManager.stats);
    }
    public override void OnTriggerEnter(Collider other)
    {
        
    }
}
