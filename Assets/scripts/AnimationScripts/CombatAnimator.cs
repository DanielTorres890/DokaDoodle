using Unity.Netcode;
using UnityEngine;

//FOR PLAYERS
public class CombatAnimator : NetworkBehaviour
{
    [SerializeField]private Animator animator;
    [SerializeField]private AbilityManager abilityManager;
    private AnimatorOverrideController overrideController;
    [SerializeField] private SpriteRenderer weaponSprite;
    [SerializeField] private SpriteRenderer shieldSprite;
    private playerData playerInfo;

   
    public void Start()
    {
        TryGetComponent(out animator);
        TryGetComponent(out abilityManager);
        playerInfo = abilityManager.stats as playerData;
      
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void SelectCurrentAttackRpc()
    {
        
        //i would like to be the one to say that this is RIDICULOUS THAT ITS BASED ON THE CLIP NAME AND NOT THE STATE
        
        overrideController["IdleAnim"] = abilityManager.currentAttack.attackAnimation;
        for (int i = 0; i < NetworkData.Instance.playerInventories[playerInfo.playerNumber][1].container.Count; i++)
        {
            var weapon = (NetworkData.Instance.playerInventories[playerInfo.playerNumber][1].container[i].item as WeaponItem);
            if (abilityManager.currentAttack == weapon.attack)
            {
                if(weapon.type == ItemType.Weapon)
                {
                    weaponSprite.gameObject.SetActive(true);
                    weaponSprite.sprite = weapon.itemSprite;
                }
                if (weapon.type == ItemType.Shield) 
                {
                    shieldSprite.gameObject.SetActive(true);
                    shieldSprite.sprite = weapon.itemSprite;
                }

            }
        }


        animator.runtimeAnimatorController = overrideController;
        
        
        animator.SetBool("Attacking", true);
        
        
        
    }
   
    public void EndCurrentAttack()
    {
        animator.SetBool("Attacking", false);
        
    }
}
