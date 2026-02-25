using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
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
        if (!abilityManager.currentAttack)
        {
            Debug.Log("something weird is happening here ");
            return;
        }
        if (abilityManager.currentAttack.startUpAnimation)
        {
            overrideController["DefaultStartUp"] = abilityManager.currentAttack.startUpAnimation;
        }
        
        if(abilityManager.currentAttack.attackAnimation)
        {
            
            overrideController["DefaultAttack"] = abilityManager.currentAttack.attackAnimation;
            animator.SetFloat("AnimationSpeed", abilityManager.currentAttack.animationSpeed);
        }
        else
        {
            Debug.LogWarning(abilityManager.currentAttack.attackName + " Does not have a player animation you might care about that");
        }
        for (int i = 0; i < NetworkData.Instance.playerInventories[playerInfo.playerNumber][1].container.Count; i++)
        {
            var weapon = (NetworkData.Instance.playerInventories[playerInfo.playerNumber][1].container[i].item as WeaponItem);
            
            if (weapon.attack.Contains(abilityManager.currentAttack))
            {
                if (weapon.itemSprite == null) 
                {
                    weaponSprite.gameObject.SetActive(false);
                    shieldSprite.gameObject.SetActive(false);
                    break; 
                }
                if (weapon.type == ItemType.Weapon)
                {
                    weaponSprite.gameObject.SetActive(true);
                    weaponSprite.sprite = weapon.itemSprite;
                    shieldSprite.gameObject.SetActive(false);
                }
                else if (weapon.type == ItemType.Shield) 
                {
                    shieldSprite.gameObject.SetActive(true);
                    shieldSprite.sprite = weapon.itemSprite;
                    weaponSprite.gameObject.SetActive(false);
                }
                else
                {
                    weaponSprite.gameObject.SetActive(false);
                    shieldSprite.gameObject.SetActive(false);

                }

            }
        }
        

        animator.runtimeAnimatorController = overrideController;
        animator.SetBool("StartUp", true);
        
        EndCurrentAttack();
        
        
    }
   public void StartUpAnimState(bool stateToBe)
    {
        animator.SetBool("StartUp", stateToBe);
    }
    public void EndCurrentAttack()
    {
        animator.SetBool("Attacking", false);
        
    }
    public void WalkingState(bool stateToBe)
    {
        animator.SetBool("Walking", stateToBe);
    }
}
