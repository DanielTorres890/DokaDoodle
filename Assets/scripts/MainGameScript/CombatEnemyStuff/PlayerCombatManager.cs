using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCombatManager : MonoBehaviour
{

    public static PlayerCombatManager Instance;
    public EnemyDataBase EnemyDataBase;
    public EnemyEncounterDataBase EnemyEncounterDataBase;

    public List<EntityStats> combatants = new List<EntityStats>();
    public EnemyEncounter currentEncounter;
    public EntityStats combatant1;//LEGACY STUFF RIGHT HERE
    public EntityStats combatant2;
    
    private void Awake()
    {
        if (Instance != null) { return; }
        Instance = this;
    }
    
}
