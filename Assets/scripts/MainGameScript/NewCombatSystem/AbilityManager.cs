using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
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
    [SerializeField] private GameObject myHealthbar;
    [SerializeField] private GameObject damageNumber;
    public UnityEvent onStatus;
    public UnityEvent onAttack;
    public UnityEvent onSpawnAttack; //bc im dumb and dont feel like changing the labels rn
    public UnityEvent onEndAttack;
    public UnityEvent onHit;
    public UnityEvent onEnergyChange;
    public UnityEvent onSetUp;
    public UnityEvent onItemUse;

    private Animator animator;

    public float maxEnergy = 100f;
    public float currentEnergy = 100f;
    public float energyRegen = 1f;
    public float chargeDuration = 0f;

    public GameObject visualsParent;

    
    private float updateStatsTimer = 0f;
    private float whenToUpdate = 1f;

    private float onEverySecondTimer = 1f;
    private float minFontSize = 5;
    private float maxFontSize = 30;
    
    private void Awake()
    {

        

    }
    public override void OnNetworkSpawn()
    {
        //NEWCOMBAT MANAGER HAS TO EXIST ALREADY IT DOES NOT MAKE SENSE IF MANAGER SPAWNS THIS IN HOW COULD IT NOT ALREADY EXIST (we also already wait for everyone to load in)

        NewCombatManager.instance.fricku.Add(gameObject);
        NewCombatManager.instance.allCombatants.Add(this);

        TryGetComponent(out animator);

    }
    public void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if(stats.isDead)
        {
            if (startUpEffects != null)
                Destroy(startUpEffects);
        }
        if(NewCombatManager.instance.fightOver || stats.isDead) { return; }
        //i got mixed opinions on this being out here but w/e

        for (int i = stats.statuses.Count - 1; i >= 0; i--)
        {
            var status = stats.statuses[i];
            stats.ProgressStatuses(Time.deltaTime);
            NetworkData.Instance.buffDataBase.GetItem[status.buffId].OnEveryTick(this);
            

        }
        onEverySecondTimer -= Time.deltaTime;
        if(onEverySecondTimer < 0f)
        {
            onEverySecondTimer = 1;
            foreach(var status in stats.statuses)
            {
                NetworkData.Instance.buffDataBase.GetItem[status.buffId].OnEverySecond(this);
            }

        }

        updateStatsTimer += Time.deltaTime;
        if (updateStatsTimer > whenToUpdate)
        {
            onStatus.Invoke();
            updateStatsTimer = 0f;
        }

        if (!IsOwner) { return; }

        Ray rayer = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(rayer.origin, rayer.direction * 100, Color.blue);



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
            bool meetsConditions = CheckCondition(atk);
            
            if (stateManager[atk].pressed && combatantstate == combatantStates.Free && stateManager[atk].cooldown <= 0 && meetsConditions && atk.initialEnergyCost <= currentEnergy)
            {
                currentEnergy -= atk.initialEnergyCost;
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

                
                

                
                PerformAttackRpc(GetCurrentAtkNum(), NetworkManager.Singleton.LocalTime.TimeAsFloat, gameObject.transform.position,spawnedAttack.transform.eulerAngles, rayer.origin,rayer.direction);
                
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
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void SpawnStartFabUpRpc(int whom)
    {
      
        currentAttack = stats.attacks[whom];

        if(currentAttack.startUpNoise)
        AudioSource.PlayClipAtPoint(currentAttack.startUpNoise, transform.position);

        
        onAttack.Invoke();
        currentAttack.OnStartUp(gameObject);
        if(currentAttack.startUpPrefab != null && startUpEffects == null)
        {
            startUpEffects = Instantiate(currentAttack.startUpPrefab);
            startUpEffects.transform.position = gameObject.transform.position;
            startUpEffects.transform.SetParent(gameObject.transform);

        }
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    private void DestroyStartUpFabRpc()
    {
        if(startUpEffects != null)
        {
            
            Destroy(startUpEffects);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void PerformAttackRpc(int whom, float time, Vector3 wherewasyou, Vector3 whereyoulookin, Vector3 origin, Vector3 direction)
    {
        currentAttack = stats.attacks[whom];
        
        currentAttack.WeaponEffect(gameObject,time, wherewasyou, whereyoulookin, chargeDuration, origin, direction);
        chargeDuration = 0f;
        InvokeOnSpawnAttackRpc();
    }
    [Rpc(SendTo.ClientsAndHost,InvokePermission = RpcInvokePermission.Everyone)]
    private void InvokeOnSpawnAttackRpc()
    {
        onSpawnAttack.Invoke();
    }

    [Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Everyone)]
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

        actions.actions["BattleItem"].performed += UseItem;

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
            stateManager[stats.attacks[i]].cooldown = stats.attacks[i].startCooldown;
            orderedAttacks.Add(stats.attacks[i]);
        }

        actions.actions["ClassAbility"].performed += M1Attack;
        actions.actions["ClassAbility"].canceled += M1AttackReleased;
        inputToInt.Add(actions.actions["ClassAbility"].controls[0], stats.attacks.Count-1);
        stateManager.Add(stats.attacks[stats.attacks.Count - 1], new AbilityStates());
        stateManager[stats.attacks[stats.attacks.Count - 1]].cooldown = stats.attacks[stats.attacks.Count - 1].startCooldown;
        orderedAttacks.Add(stats.attacks[stats.attacks.Count - 1]);
        maxEnergy = 100;
        energyRegen = 1;
    }

    public void AssignStateManager()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i >= stats.attacks.Count) { break; }
            stateManager.Add(stats.attacks[i], new AbilityStates());
            orderedAttacks.Add(stats.attacks[i]);
            stateManager[stats.attacks[i]].cooldown = stats.attacks[i].startCooldown;
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

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void ImHitRpc(int damageAmt)
    {
        stats.stats[Attributes.Health] -= damageAmt;
        
        stats.PostStatusStatCalc();

        for(int i = 0; i < stats.statuses.Count; i++)
        {
            BuffBase currentBuff = NetworkData.Instance.buffDataBase.GetItem[stats.statuses[i].buffId];
            if (currentBuff is GuardBuff)
            {
                Debug.Log("I was guarding mf");
                GuardBuff guard = (GuardBuff)currentBuff;
                if(guard.withinParryWindow(stats))
                {
                    AudioSource.PlayClipAtPoint(guard.parryNoise, transform.position, SettingsManager.instance.SFXVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(guard.guardNoise, transform.position, SettingsManager.instance.SFXVolume);
                }
                break;
            }
        }
        hpText.UpdateText();
        if (stats.stats[Attributes.Health] <= 0 && !stats.isDead)
        {
            stats.isDead = true;
            stats.stats[Attributes.Health] = 0;
            if(NewCombatManager.instance.allCombatants.Contains(this))
            NewCombatManager.instance.KILL(this);
        }
        onHit.Invoke();
        GameObject dmgNum = Instantiate(damageNumber);
        dmgNum.transform.position = transform.position;
        dmgNum.transform.position += new Vector3(UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(-.5f, .5f), UnityEngine.Random.Range(-.5f, .5f));
        var textComponent = dmgNum.GetComponent<TextMeshPro>();
        textComponent.text = damageAmt.ToString();
        textComponent.fontSize = Mathf.Clamp(100f * damageAmt / stats.stats[Attributes.MaxHealth], minFontSize, maxFontSize);
        if(NetworkData.Instance.players[NetworkData.Instance.ClientNumToPlayerNum(NetworkManager.Singleton.LocalClientId)] == stats)
        {
            textComponent.color = Color.red;
        }
        
        
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void IGainedBuffRpc(int[] buffId)
    {
        
        foreach(int i in buffId)
        {
            stats.GainStatus(NetworkData.Instance.buffDataBase.GetItem[i]);
        }
       
        stats.PostStatusStatCalc();
        onStatus.Invoke();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void ILostBuffRpc(int buffId)
    {
        Debug.Log("Did i lose a buff");
        stats.RemoveStatus(buffId);
        onStatus.Invoke();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
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


        
        
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateMyLooksRpc()
    {
        characterEditor characterEdit = GetComponent<characterEditor>();
        PartyMember myStats = (stats as PartyMember);

        

        characterEdit.setClass(myStats.allyClass);
        characterEdit.setFace(myStats.allyFace);
        characterEdit.setHair(myStats.allyHair);


        if(TryGetComponent(out CinemachineCamera cam )&& !NewCombatManager.instance.cameras.Contains(cam))
        NewCombatManager.instance.cameras.Add(cam);
        
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateStatsRpc(int combatantNum)
    {

        
        stats = PlayerCombatManager.Instance.combatants[combatantNum];
       
        nameText.AbilityManager = this;
        hpText.AbilityManager = this;
        stats.PostStatusStatCalc();
        nameText.UpdateText();
        hpText.UpdateText();

        CinemachineCamera cam = gameObject.GetComponentInChildren<CinemachineCamera>();
        if (cam && !NewCombatManager.instance.cameras.Contains(cam))
        {
            NewCombatManager.instance.cameras.Add(cam);
        }

        if (myHealthbar && stats is playerData && NetworkData.Instance.IsAllowed((stats as playerData).playerNumber, NetworkManager.Singleton.LocalClientId))
        {
            myHealthbar.SetActive(false);
            nameText.gameObject.SetActive(false);
            
        }

        foreach (var status in stats.statuses)
        {

            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId].buffFx)
            {
                var fx = Instantiate(NetworkData.Instance.buffDataBase.GetItem[status.buffId].buffFx, transform);

                onStatus.AddListener(delegate
                {
                    foreach (var status2 in stats.statuses)
                    {
                        if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == NetworkData.Instance.buffDataBase.GetItem[status2.buffId])
                        {
                            return;
                        }
                    }

                    Destroy(fx);
                });
            }
        }
       
        
        if (IsOwner) 
        { 
            if (gameObject.TryGetComponent(out BaseEnemyBehavior ai))
            {
             
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

    private void UseItem(InputAction.CallbackContext action)
    {
        if(!IsOwner) { return; }
        if((stats as playerData).battleSlotItemId == -1) { return; }
        if(combatantstate != combatantStates.Free) { return; }

        UseItemRpc();


    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void UseItemRpc()
    {
        
        playerData thisPlayer = (stats as playerData);
        ItemBase ourItem = NetworkData.Instance.playerInventories[thisPlayer.playerNumber][0].database.GetItem[thisPlayer.battleSlotItemId];
        ourItem.InCombatAction(this);
        NetworkData.Instance.playerInventories[thisPlayer.playerNumber][0].RemoveItem(ourItem);
        thisPlayer.battleSlotItemId = -1;
        onItemUse.Invoke();
    }
    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public void TeleportMeRpc(Vector3 location, Vector3 eulerAngles, NetworkObjectReference whom)
    {
        
        transform.position = location;
        if(IsOwner)
        {
            Debug.Log("I own this? " + gameObject.name);
            transform.GetComponent<NetworkTransform>().Teleport(location, Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z), transform.localScale);
            if(whom.TryGet(out NetworkObject obj) && !obj.IsOwner)
            {
                if (obj.gameObject == transform.gameObject) { return;}

                AbilityManager other = obj.GetComponent<AbilityManager>();
                if(other.visualsParent)
                other.DelayActive();

            }
        }
        
        
    }

    public void DelayActive()
    {
        float delay = .25f;
        if(NetworkManager.Singleton.IsHost) { delay = .175f; }
        if(OwnerClientId == 0) { delay = .175f; }
        if(NetworkManager.Singleton.IsHost && OwnerClientId == 0) { delay = .03f; }


        StartCoroutine(delayShowBack(delay));
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
    public bool CanWalk()
    {
        return combatantstate == combatantStates.Free || combatantstate == combatantStates.StartUpFree;
    }
   
    public void ChangeEnergy(float change)
    {
        currentEnergy += change;
        if(currentEnergy > maxEnergy) { currentEnergy= maxEnergy; }
        if(currentEnergy < 0) { currentEnergy = 0; }
        onEnergyChange.Invoke();
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
        //i really dont understand why this would be null but okay
        if(animator)
        animator.SetBool("Walking", true);
    }
    private void StopWalking(InputAction.CallbackContext action)
    {
        if (animator)
            animator.SetBool("Walking", false);
    }
    private bool CheckCondition(AttackBase attack)
    {
        bool condition = true;
        foreach(var conditions  in attack.conditions)
        {
            condition = conditions.Condition(this);
        }
        return condition;
    }
    private IEnumerator delayShowBack(float duration = .15f)
    {
        visualsParent.SetActive(false);
        yield return new WaitForSeconds(duration);
        visualsParent.SetActive(true);
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



