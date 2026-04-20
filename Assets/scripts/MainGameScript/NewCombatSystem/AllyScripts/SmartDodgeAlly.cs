using Unity.Netcode;
using UnityEngine;

public class SmartDodgeAlly : SmartDodgeBehavior
{
    [SerializeField] private GameObject spawnedWeapon;
    [SerializeField] private Transform WeaponTransform;

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Everyone)]
    public override void AttackAnimRpc(int attackIndex)
    {
        selectedAttack = myManager.stats.attacks[attackIndex];

        if (spawnedWeapon != null) { Destroy(spawnedWeapon); }

        if (selectedAttack.weaponPrefab != null && WeaponTransform)
        {
            spawnedWeapon = Instantiate(selectedAttack.weaponPrefab, WeaponTransform);
            spawnedWeapon.transform.localPosition = selectedAttack.weaponPosition;
        }
        overrideController["DefaultStartUp"] = selectedAttack.startUpAnimation;

        overrideController["DefaultAttack"] = selectedAttack.attackAnimation;
        animator.SetFloat("AnimSpeed", selectedAttack.animationSpeed);
        animator.runtimeAnimatorController = overrideController;



        if (IsOwner)
            animator.SetBool("Walking", false);
    }
}
