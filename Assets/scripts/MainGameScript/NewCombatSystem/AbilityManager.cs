using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityManager : NetworkBehaviour
{
    [SerializeField] private PlayerInput actions;


    private Dictionary<AttackBase, AbilityStates> stateManager = new Dictionary<AttackBase, AbilityStates>();
    [SerializeField] private AttackBase testAttack;

    public EntityStats stats;
    public combatantStates combatantstate = combatantStates.Free;
    public float stateDuration;

    private AttackBase currentAttack = null;
    void Awake()
    {

        
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) { return; }
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
    

    public void AssignAbilities()
    {
        Debug.Log("Who owns" + OwnerClientId);
        Debug.Log("Who am i" + NetworkManager.Singleton.LocalClientId);
        Debug.Log("So im the owner?" + IsOwner);
        Debug.Log("And the server owns me? " + IsOwnedByServer);
        if (!IsOwner) { return; }
      
        actions.SwitchCurrentActionMap("Player");
        actions.actions["M1Attack"].performed += M1Attack;
        actions.actions["M1Attack"].canceled += M1AttackReleased;
        stateManager.Add(testAttack, new AbilityStates());

        for (int i = 1; i < 10; i++)
        {
            if (i >= stats.attacks.Count) { break; }

            actions.actions["Ability" + i.ToString()].performed += Ability1;
            actions.actions["Ability" + i.ToString()].canceled += Ability1Released;
            stateManager.Add(stats.attacks[i], new AbilityStates());

        }
    }
    private void M1Attack(InputAction.CallbackContext action)
    {
        if (!IsOwner) { return; }
        stateManager[testAttack].pressed = true;
        
    }
    private void M1AttackReleased(InputAction.CallbackContext action)
    {

        if (!IsOwner) { return; }
        stateManager[testAttack].pressed = false;

    }
    private void Ability1(InputAction.CallbackContext action)
    {
        if (!IsOwner) { return; }
        if (stats.attacks.Count <= 1) { return;  }
        stateManager[stats.attacks[1]].pressed = true;

    }
    private void Ability1Released(InputAction.CallbackContext action)
    {
        if (!IsOwner) { return; }
        if (stats.attacks.Count <= 1) { return; }
        stateManager[stats.attacks[1]].pressed = false;

    }
    public void OnTriggerEnter(Collider other)
    {
        
        if (!IsOwner) { return; }
        
        if (other.gameObject.TryGetComponent(out AbilityBase hitby))
        {
            if (gameObject == hitby.owner) { return; }
            ImHitRpc(hitby.DamageCalculator(stats));
        }

    }
    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    private void ImHitRpc(int damageAmt)
    {
        stats.stats[Attributes.Health] -= damageAmt;
        Debug.Log(stats.name + " got hit for " +  damageAmt + " ouchy");
    }

    [Rpc(SendTo.Everyone, RequireOwnership = true)]
    public void UpdateMaterialRpc(int playerNum)
    {
        var render = GetComponentInChildren<MeshRenderer>();
        render.material = NetworkData.Instance.playerSticks[playerNum].GetComponent<characterEditor>().myMaterial;
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