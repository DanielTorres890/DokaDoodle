using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NewCombatManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    // Start is called before the first frame update
    public void Start()
    {
        foreach (var entity in PlayerCombatManager.Instance.combatants)
        {
            
            if (!NetworkManager.Singleton.IsHost) { return; }
            if (entity is playerData)
            {
                var player = entity as playerData;
                var playerfab = Instantiate(playerPrefab);
                var editor = playerfab.GetComponent<characterEditor>();
                var abilitiyManage = playerfab.GetComponent<AbilityManager>();
                
                abilitiyManage.stats = player;
                
                abilitiyManage.AssignAbilities();
                editor.setClass(player.playerClass);
                editor.setFace(player.playerFace);
                editor.setHair(player.playerHair);
                playerfab.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)player.playerNumber);

            }
            else
            {
                var npc = entity as EnemyCombat;
                var npcfab = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[npc.enemyId].enemyPrefab);
             
                npcfab.GetComponent<NetworkObject>().Spawn();

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
