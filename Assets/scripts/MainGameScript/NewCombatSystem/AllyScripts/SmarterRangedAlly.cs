using Unity.Netcode;
using UnityEngine;

public class SmarterRangedAlly : SmarterRangedLogic
{
    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public override void AttackAnimRpc(int attackIndex)
    {
        selectedAttack = myManager.stats.attacks[attackIndex];

        Debug.Log("Is the ally animation playing? ");
        overrideController["DefaultStartUp"] = selectedAttack.startUpAnimation;

        overrideController["DefaultAttack"] = selectedAttack.attackAnimation;
        animator.SetFloat("AnimSpeed", selectedAttack.animationSpeed);
        animator.runtimeAnimatorController = overrideController;
        animator.SetBool("Walking", false);
    }
}
