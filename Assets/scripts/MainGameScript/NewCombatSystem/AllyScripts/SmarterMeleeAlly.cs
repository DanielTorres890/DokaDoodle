using UnityEngine;

public class SmarterMeleeAlly : SmarterMeleeEnemy
{
    public override void AttackAnimRpc(int attackIndex)
    {
        selectedAttack = myManager.stats.attacks[attackIndex];


        overrideController["DefaultStartUp"] = selectedAttack.startUpAnimation;

        overrideController["DefaultAttack"] = selectedAttack.attackAnimation;
        animator.SetFloat("AnimSpeed", selectedAttack.animationSpeed);
        animator.runtimeAnimatorController = overrideController;
        animator.SetBool("Walking", false);
    }
}
