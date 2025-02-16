using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewCombatManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    public List<GameObject> fricku = new List<GameObject>();
    // Start is called before the first frame update
    public static NewCombatManager instance;
  
    public override void OnNetworkSpawn()
    {


        if (instance == null)
        {
            instance = this;
        }
            
        if (IsServer)
        {
          
                //b UT WHY DOES IT SPAWN DOUBLE IF A CLIENT HASNT LOADED IN YET BC ONLY THE SERVER SHOULDVE BEEN ALLOWED TO SPAWNS STUFF IN AND THE HOST AND SERVER AR ETHE SAME HOW DOES THAT EVEN MAKE SENSE SMD FRICK U EMA I AHTE U LMB EXPLODE
                //shoutout to onloadcomplete tho brother fixed that stupid problem
                NetworkManager.Singleton.SceneManager.OnLoadComplete += SetUp;
    
            
            base.OnNetworkSpawn();

        }
            
        
         
    }

    private void SetUp(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        SetUpRpc();
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SetUpRpc()
    {
        
      
        foreach (var entity in PlayerCombatManager.Instance.combatants)
        {


            Debug.Log("FINAL PROOOF WHO AM I: " + NetworkManager.Singleton.LocalClientId);
            if (entity is playerData)
            {
                
                var player = entity as playerData;
                var playerfab = Instantiate(playerPrefab);
                playerfab.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)player.playerNumber);
                //var editor = NetworkData.Instance.playerSticks[player.playerNumber].GetComponent<characterEditor>();
                //editor.UpdateMaterial();
                /*playerfab.GetComponentInChildren<MeshRenderer>().material = editor.myMaterial;*/
               
                var abilitiyManage = playerfab.GetComponent<AbilityManager>();
                abilitiyManage.UpdateMaterialRpc(player.playerNumber);
                fricku.Add(playerfab);
                abilitiyManage.stats = player;

             

                
                abilitiyManage.AssignAbilities();
                Debug.Log("PlayerSpawn: " + playerfab.GetComponent<NetworkObject>().NetworkObjectId);

            }
            else
            {
                var npc = entity as EnemyCombat;
                var npcfab = Instantiate(PlayerCombatManager.Instance.EnemyDataBase.GetEnemies[npc.enemyId].enemyPrefab);
                fricku.Add(npcfab);
                npcfab.GetComponent<NetworkObject>().Spawn();
                Debug.Log("Enemy Spawn: " + npcfab.GetComponent<NetworkObject>().NetworkObjectId);

            }
        }
        base.OnNetworkSpawn();
    }
    private void UpdatePlayerLook()
    {

    }
}
