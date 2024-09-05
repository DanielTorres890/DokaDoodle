using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{

    public static PlayerCombatManager Instance;
    public EnemyDataBase EnemyDataBase;

    public EntityStats combatant1;
    public EntityStats combatant2;
    private void Awake()
    {
        Instance = this;
    }
    
}
