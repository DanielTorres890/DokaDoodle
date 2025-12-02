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

    public AttackBase currentAttack = null;
    private GameObject spawnedAttack;
    private GameObject startUpEffects;

    private Dictionary<InputControl,int> inputToInt = new Dictionary<InputControl,int>();

    [SerializeField] private EntityUIUpdate nameText;
    [SerializeField] private EntityUIUpdate hpText;


    public UnityEvent onStatus;
    public UnityEvent onAttack;
    public UnityEvent onSpawnAttack; //bc im dumb and dont feel like changing the labels rn
    public UnityEvent onEndAttack;
    public UnityEvent onHit;
    public UnityEvent onEnergyChange;
    private Animator animator;

    public float maxEnergy = 100f;
    public float currentEnergy = 100f;
    public float energyRegen = 1f;
    public float chargeDuration = 0f;

    private float updateStatsTimer = 0f;
    private float whenToUpdate = 1f;
    private void Awake()
    {
       

    }
    public override void OnNetworkSpawn()
    {
        NewCombatManager.instance.fricku.Add(gameObject);
        NewCombatManager.instance.allCombatants.Add(this);
        
        TryGetComponent(out animator);


    }

    // Update is called once per frame
    void Update()
    {
        if(NewCombatManager.instance.fightOver || stats.isDead) { return; }
        //i got mixed opinions on this being out here but w/e
        for (int i = stats.statuses.Count - 1; i >= 0; i--)
        {
            var status = stats.statuses[i];
            stats.ProgressStatuses(Time.deltaTime);

        }
        updateStatsTimer += Time.deltaTime;
        if (updateStatsTimer > whenToUpdate)
        {
            onStatus.Invoke();
            updateStatsTimer = 0f;
        }

        if (!IsOwner) { return; }
        
        
        

        
        if (combatantstate != combatantStates.Free)
        {
            stateDuration -= Time.deltaTime;
            if (currentAttack != null && stateManager[currentAttack].pressed && currentAttack.chargeable && stateDuration < .1f)
            {
                stateDuration = 0.01f;
                currentEnergy -= Time.deltaTime * currentAttack.energyDrain;
                chargeDuration += Time.deltaTime;
                if (currentEnergy <= 0f)
                {
                    currentEnergy = 0f;
                    stateDuration = 0f;
                }
                onEnergyChange.Invoke();
            }
        }
        else
        {
            currentEnergy += Time.deltaTime * energyRegen;
            if(currentEnergy > maxEnergy)
            {
                currentEnergy = maxEnergy;
            }
            else
            {
                onEnergyChange.Invoke();
            }

        }

        foreach (var atk in  stateManager.Keys) 
        {
            stateManager[atk].cooldown -= Time.deltaTime;
            if (stateManager[atk].pressed && combatantstate == combatantStates.Free && stateManager[atk].cooldown <= 0)
            {
                currentAttack = atk;
                combatantstate = atk.stateToBe;
                stateDuration = currentAttack.startUp;
                
                if (currentAttack.startUpPrefab != null && startUpEffects == null)
                {
                    SpawnStartFabUpRpc(GetCurrentAtkNum());
                }
            }
        }

        
        
        if (stateDuration <= 0 && combatantstate != combatantStates.Free) //time for state to progress
        {
            if(combatantstate == combatantStates.Attacking) { combatantstate = combatantStates.Endlag; } //checking if the attack has active time IE dash attack

            else if (currentAttack != null && InStartUp())
            {

                /* if(stats is EnemyCombat) // this existed to check enemies since for some reason they were acting weird
                 {
                     Debug.Log(" IM WALLOPPING A BIT TOO FAST I THINKS");
                 }*/
                DestroyStartUpFabRpc();
                spawnedAttack = currentAttack.WeaponEffect(gameObject);

                
                onSpawnAttack.Invoke();
                PerformAttackRpc(GetCurrentAtkNum(), NetworkManager.Singleton.LocalTime.TimeAsFloat, gameObject.transform.position,gameObject.transform.eulerAngles);

                stateDuration = currentAttack.attackDuration;
                combatantstate = combatantStates.Attacking;
                if (stateDuration <= 0) //  i'd like to point out that i COULD do a list of states to progress through, then for loop through them but i dont see any usecase for that 
                {
                    combatantstate = combatantStates.Endlag;
                    stateDuration = currentAttack.endLag;
                    onEndAttack.Invoke();
                    if (stateDuration <= 0  ) { combatantstate = combatantStates.Free; }
                }
                
                stateManager[currentAttack].cooldown = currentAttack.cooldown;
                
            }
            else if (combatantstate == combatantStates.Attacking)
            {
                currentAttack = null;
                combatantstate = combatantStates.Endlag;
                stateDuration = currentAttack.endLag;
                onEndAttack.Invoke();
                if (stateDuration <= 0) { combatantstate = combatantStates.Free; }
            }
            else
            {
                onEndAttack.Invoke();
                combatantstate = combatantStates.Free;
            }
        }
        

    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void SpawnStartFabUpRpc(int whom)
    {
        
        currentAttack = stats.attacks[whom];
        onAttack.Invoke();
        currentAttack.OnStartUp(gameObject);
        if(currentAttack.startUpPrefab != null && startUpEffects == null)
        {
            startUpEffects = Instantiate(currentAttack.startUpPrefab);
            startUpEffects.transform.position = gameObject.transform.position;
            startUpEffects.transform.SetParent(gameObject.transform);

        }
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    private void DestroyStartUpFabRpc()
    {
        if(startUpEffects != null)
        {
            
            Destroy(startUpEffects);
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void PerformAttackRpc(int whom, float time, Vector3 wherewasyou, Vector3 whereyoulookin)
    {
        currentAttack = stats.attacks[whom];
        
        currentAttack.WeaponEffect(gameObject,time, wherewasyou, whereyoulookin, chargeDuration);
        chargeDuration = 0f;
        InvokeOnSpawnAttackRpc();
    }
    [Rpc(SendTo.Server,RequireOwnership = false)]
    private void InvokeOnSpawnAttackRpc()
    {
        onSpawnAttack.Invoke();
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
        

        actions.actions["M1Attack"].performed += M1Attack;
        actions.actions["M1Attack"].canceled += M1AttackReleased;

        actions.actions["Move"].performed += StartWalking;
        actions.actions["Move"].canceled += StopWalking;

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
            orderedAttacks.Add(stats.attacks[i]);

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
        
        stats.PostStatusStatCalc();
        hpText.UpdateText();
        if (stats.stats[Attributes.Health] <= 0 && !stats.isDead)
        {
            stats.isDead = true;
            NewCombatManager.instance.KILL(this);
        }
        onHit.Invoke();
        
        
    }
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void IGainedBuffRpc(int[] buffId)
    {
        foreach(int i in buffId)
        {
            stats.GainStatus(NetworkData.Instance.buffDataBase.GetItem[i]);
        }
        Debug.Log("I gained buffs i think");
        stats.PostStatusStatCalc();
        onStatus.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void UpdateMaterialRpc(int playerNum)
    {
        /*var render = GetComponentInChildren<MeshRenderer>(); skip for now maybe?
        render.material = NetworkData.Instance.playerSticks[playerNum].GetComponent<characterEditor>().myMaterial;
       */
        //im pretty sure i have to put this stuff here bc the method below has to work for all enemy types and they're not all player datas
        characterEditor characterEdit = GetComponent<characterEditor>();
        characterEdit.setClass(NetworkData.Instance.players[playerNum].playerClass);
        characterEdit.setFace(NetworkData.Instance.players[playerNum].playerFace);
        characterEdit.setHair(NetworkData.Instance.players[playerNum].playerHair);
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


    private int GetCurrentAtkNum()
    {
        int foundu = 0;

        for (int i = 0; i < stats.attacks.Count; i++)
        {
            if (stats.attacks[i] == currentAttack) { foundu = i; break; }
        }
        return foundu;
    }
    public bool CanMove()
    {
        return (combatantstate == combatantStates.Free || combatantstate == combatantStates.StartUpFree || combatantstate == combatantStates.Dashing || combatantstate == combatantStates.Attacking) && !stats.isDead;
    }
    public bool InStartUp()
    {
        return combatantstate == combatantStates.StartUp || combatantstate == combatantStates.StartUpFree;
    }
    public bool CanMoveNotAct()
    {
        return (combatantstate == combatantStates.Attacking || combatantstate == combatantStates.Dashing);
    }
    public bool CanAct()
    {
        return combatantstate == combatantStates.Free;
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
    private void StartWalking(InputAction.CallbackContext action)
    {
        animator.SetBool("Walking", true);
    }
    private void StopWalking(InputAction.CallbackContext action)
    {
        
        animator.SetBool("Walking", false);
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
    Dashing,
    Attacking,
    Free

}



