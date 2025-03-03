using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AbilityManager : NetworkBehaviour
{
    [SerializeField] public PlayerInput actions;


    public Dictionary<AttackBase, AbilityStates> stateManager = new Dictionary<AttackBase, AbilityStates>();
    public List<AttackBase> orderedAttacks = new List<AttackBase>();

    [SerializeField] private AttackBase testAttack;

    public EntityStats stats;
    public combatantStates combatantstate = combatantStates.Free;
    public float stateDuration;

    private AttackBase currentAttack = null;
    private GameObject spawnedAttack;

    private Dictionary<InputControl,int> inputToInt = new Dictionary<InputControl,int>();

    [SerializeField] private EntityUIUpdate nameText;
    [SerializeField] private EntityUIUpdate hpText;

    public UnityEvent onStatus;
    public override void OnNetworkSpawn()
    {
        NewCombatManager.instance.fricku.Add(gameObject);
        NewCombatManager.instance.allCombatants.Add(this);
        
      
        
    }

    // Update is called once per frame
    void Update()
    {

        if(!IsOwner || NewCombatManager.instance.fightOver || stats.isDead) { return; }
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
                int foundu = 0;
                
                for(int i = 0; i < stats.attacks.Count; i++ )
                {
                    if (stats.attacks[i] == currentAttack) {  foundu = i; break; }
                }
                if(stats is EnemyCombat)
                {
                    Debug.Log(" IM WALLOPPING A BIT TOO FAST I THINKS");
                }
                
                spawnedAttack = currentAttack.WeaponEffect(gameObject);
                PerformAttackRpc(foundu, NetworkManager.Singleton.LocalTime.TimeAsFloat, gameObject.transform.position,gameObject.transform.eulerAngles);

                stateDuration = currentAttack.endLag;
                combatantstate = combatantStates.Endlag;
                if (stateDuration <= 0)
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

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void PerformAttackRpc(int whom, float time, Vector3 wherewasyou, Vector3 whereyoulookin)
    {
        currentAttack = stats.attacks[whom];
       
        currentAttack.WeaponEffect(gameObject,time, wherewasyou, whereyoulookin);

    }
    [Rpc(SendTo.SpecifiedInParams, RequireOwnership = false)]
    public void RealAttackRpc(RpcParams rpcStuff)
    {
        if (spawnedAttack == null) { return ; }
        Destroy(spawnedAttack);
    }
    public void AssignAbilities()
    {
       
        if (!IsOwner) { return; }
      

        actions.SwitchCurrentActionMap("Player");
        onStatus = new UnityEvent();


        actions.actions["M1Attack"].performed += M1Attack;
        actions.actions["M1Attack"].canceled += M1AttackReleased;
        inputToInt.Add(actions.actions["M1Attack"].controls[0], 0);
        stateManager.Add(stats.attacks[0], new AbilityStates());
        orderedAttacks.Add(stats.attacks[0]);

        for (int i = 1; i < stats.attacks.Count-1; i++)
        {
            if (i >= stats.attacks.Count) { break; }

            if (stateManager.ContainsKey(stats.attacks[i])) { continue; }

          
            actions.actions["Ability" + i.ToString()].performed += M1Attack;
            actions.actions["Ability" + i.ToString()].canceled += M1AttackReleased;
            inputToInt.Add(actions.actions["Ability" + i.ToString()].controls[0], i);
            stateManager.Add(stats.attacks[i], new AbilityStates());
            orderedAttacks.Add(stats.attacks[i]);
        }

        actions.actions["ClassAbility"].performed += M1Attack;
        actions.actions["ClassAbility"].canceled += M1AttackReleased;
        inputToInt.Add(actions.actions["ClassAbility"].controls[0], stats.attacks.Count-1);
        stateManager.Add(stats.attacks[stats.attacks.Count - 1], new AbilityStates());
        orderedAttacks.Add(stats.attacks[stats.attacks.Count - 1]);
    }

    public void AssignStateManager()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i >= stats.attacks.Count) { break; }
            stateManager.Add(stats.attacks[i], new AbilityStates());

        }
    }
   /* 
    public void OnTriggerEnter(Collider other)
    {
        
        if (!IsServer) { return; }
        
        if (other.gameObject.TryGetComponent(out AbilityBase hitby))
        {
            if (gameObject == hitby.owner) { return; }
            hitby.OnHit();
            Debug.Log("ERRRR" + other.GetType());
            
            ImHitRpc(hitby.DamageCalculator(stats));
        }

    }
   */

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void ImHitRpc(int damageAmt)
    {
        stats.stats[Attributes.Health] -= damageAmt;
        Debug.Log("did i get hit twice or did that just hurt alot " + damageAmt);
        stats.PostStatusStatCalc();
        hpText.UpdateText();
        if (stats.stats[Attributes.Health] <= 0 && !stats.isDead)
        {
            stats.isDead = true;
            NewCombatManager.instance.KILL(this);
        }
        
        
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void IGainedBuffRpc(int[] buffId)
    {
        foreach(int i in buffId)
        {
            stats.GainStatus(NetworkData.Instance.buffDataBase.GetBuff[i]);
        }
        Debug.Log("I gained buffs i think");
        stats.PostStatusStatCalc();
        onStatus.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void UpdateMaterialRpc(int playerNum)
    {
        var render = GetComponentInChildren<MeshRenderer>();
        render.material = NetworkData.Instance.playerSticks[playerNum].GetComponent<characterEditor>().myMaterial;
      
     
        NewCombatManager.instance.cameras.Add(gameObject.GetComponentInChildren<CinemachineCamera>());
       
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void UpdateStatsRpc(int combatantNum)
    {

        stats = PlayerCombatManager.Instance.combatants[combatantNum];
        nameText.AbilityManager = this;
        hpText.AbilityManager = this;
        stats.PostStatusStatCalc();
        nameText.UpdateText();
        hpText.UpdateText();
        
        if(IsOwner) 
        { 
            if (gameObject.TryGetComponent(out BaseEnemyBehavior ai))
            {
                Debug.Log("BUNGA ASSIGN");
                AssignStateManager();
            }
            else
            {
                AssignAbilities();
            }
           
        }
    }
    private void M1Attack(InputAction.CallbackContext action)
    {
        if (!IsOwner) { return; }

        stateManager[stats.attacks[inputToInt[action.control]]].pressed = true;
        

    }
    private void M1AttackReleased(InputAction.CallbackContext action)
    {

        if (!IsOwner) { return; }
        stateManager[stats.attacks[inputToInt[action.control]]].pressed = false;

    }

   /* private void Ability1(InputAction.CallbackContext action)
    {
        if (!IsOwner) { return; }
        if (stats.attacks.Count <= 1) { return; }
        stateManager[stats.attacks[1]].pressed = true;

    } //I hate this and my life but i really don't know how else to go about this bc how else would you assign these
    private void Ability1Released(InputAction.CallbackContext action)
    {
        if (!IsOwner) { return; }
        if (stats.attacks.Count <= 1) { return; }
        stateManager[stats.attacks[1]].pressed = false;

    }
    */

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
    Dashing,
    Free

}



