using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityManager : MonoBehaviour
{
    [SerializeField] private PlayerInput actions;


    private Dictionary<AttackBase, AbilityStates> stateManager = new Dictionary<AttackBase, AbilityStates>();
    [SerializeField] private AttackBase testAttack;

    public EntityStats stats;
    public combatantStates combatantstate;
    public float stateDuration;

    private AttackBase currentAttack = null;
    void Awake()
    {
        combatantstate = combatantStates.Free;

        
        actions.SwitchCurrentActionMap("Player");

        actions.actions["M1Attack"].performed += M1Attack;
        actions.actions["M1Attack"].canceled += M1AttackReleased;
        stateManager.Add(testAttack, new AbilityStates());
    }

    // Update is called once per frame
    void Update()
    {
        
        foreach (var state in  stateManager.Keys) 
        {
            stateManager[state].cooldown -= Time.deltaTime;
            if (stateManager[state].pressed && combatantstate == combatantStates.Free && stateManager[state].cooldown <= 0)
            {
                currentAttack = state;
                combatantstate = state.stateToBe;
                stateDuration = currentAttack.startUp;
            }
        }

        if (combatantstate != combatantStates.Free)
        {
            stateDuration -= Time.deltaTime;
            if (currentAttack != null && stateManager[currentAttack].pressed && currentAttack.chargeable && stateDuration < .1f)
            {
                stateDuration = 0.01f;
            }
        }
        
        if (stateDuration <= 0)
        {
            if (currentAttack != null)
            {
                currentAttack.WeaponEffect(gameObject);

                stateDuration = currentAttack.endLag;
                combatantstate = combatantStates.Endlag;
                if (stateDuration >= 0)
                {
                    combatantstate = combatantStates.Free;
                }
                
                stateManager[currentAttack].cooldown = currentAttack.cooldown;
                currentAttack = null;
            }
            else
            {
                combatantstate = combatantStates.Free;
            }
        }
        

    }

    private void M1Attack(InputAction.CallbackContext action)
    {
        stateManager[testAttack].pressed = true;
        
    }
    private void M1AttackReleased(InputAction.CallbackContext action)
    {
        stateManager[testAttack].pressed = false;

    }
}
public class AbilityStates
{
    public bool pressed;
    public float cooldown;
}

[System.Serializable]
public enum combatantStates
{
    StartUp,
    StartUpFree,
    Endlag,
    Free

}