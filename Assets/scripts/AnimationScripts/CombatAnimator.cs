using Unity.Netcode;
using UnityEngine;

//FOR PLAYERS
public class CombatAnimator : NetworkBehaviour
{
    [SerializeField]private Animator animator;
    [SerializeField]private AbilityManager abilityManager;
    private AnimatorOverrideController overrideController;
    public void Start()
    {
        TryGetComponent(out animator);
        TryGetComponent(out abilityManager);
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

    }
    public void SelectCurrentAttackRpc()
    {
        
        //i would like to be the one to say that this is RIDICULOUS THAT ITS BASED ON THE CLIP NAME AND NOT THE STATE
        
        overrideController["IdleAnim"] = abilityManager.currentAttack.attackAnimation;
        
        animator.runtimeAnimatorController = overrideController;
        
        if(IsOwner)
        animator.SetBool("Attacking", true);
       
        
        
    }
   
    public void EndCurrentAttack()
    {
        animator.SetBool("Attacking", false);
        Debug.Log("this is where it would stop");
    }
}
