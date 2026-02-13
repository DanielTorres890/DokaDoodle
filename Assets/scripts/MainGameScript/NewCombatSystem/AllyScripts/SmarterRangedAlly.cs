using Unity.Netcode;
using UnityEngine;

public class SmarterRangedAlly : SmarterRangedLogic
{
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public override void AttackAnimRpc(int attackIndex)
    {
        selectedAttack = myManager.stats.attacks[attackIndex];

        
        overrideController["DefaultStartUp"] = selectedAttack.startUpAnimation;

        overrideController["DefaultAttack"] = selectedAttack.attackAnimation;
        animator.SetFloat("AnimSpeed", selectedAttack.animationSpeed);
        animator.runtimeAnimatorController = overrideController;

        if(IsOwner)
        animator.SetBool("Walking", false);
    }
}
