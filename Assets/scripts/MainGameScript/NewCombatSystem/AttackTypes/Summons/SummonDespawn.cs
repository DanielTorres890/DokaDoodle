using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SummonDespawn : AbilityBase
{
    private AbilityManager myManager;
    private EnemyCombat myCombat;
    public override void OnNetworkSpawn()
    {
        myManager = GetComponent<AbilityManager>();

        
        
        
        
    }
    public new void Update()
    {
        if (!IsServer) { return; }
        if (lifespan < lifetimer || ownerStats.isDead)
        {
            DespawnRpc();
            Destroy(gameObject);

        }
        lifetimer += Time.deltaTime;
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnInRpc(int entityId)
    {
        EnemyCombat entity = new EnemyCombat(PlayerCombatManager.Instance.EnemyDataBase.GetItem[entityId]);
        entity.loyaltyTags.Clear();
        
        myCombat = entity;
        PlayerCombatManager.Instance.combatants.Add(entity);
        Debug.Log("I've been added");
        
        if (IsServer)
        {
            entity.loyaltyTags = new List<string>(ownerStats.loyaltyTags);
            myManager.UpdateStatsRpc(PlayerCombatManager.Instance.combatants.IndexOf(entity));
            var ownerManager = owner.GetComponent<AbilityManager>();
            for (int i = 0; i < NewCombatManager.instance.allCombatants.Count; i++)
            {
                if (NewCombatManager.instance.allCombatants[i] == ownerManager)
                {
                    SyncOwnerRpc(i);
                    break;
                }
            }
        }
        

    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SyncOwnerRpc(int ownerIndex)
    {
        owner = NewCombatManager.instance.allCombatants[ownerIndex].gameObject;
        ownerStats = NewCombatManager.instance.allCombatants[ownerIndex].stats;
        myCombat.name += "(" + ownerStats.name + ")";
        myManager.UpdateUI();
        

    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
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
