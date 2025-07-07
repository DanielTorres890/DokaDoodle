using UnityEngine;

public class CombatantAudioManager : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;
    [SerializeField]private AbilityManager abilityManager;
    private void Awake()
    {
        TryGetComponent(out audioSource);
        TryGetComponent(out abilityManager);

    }

    public void PlayCurrentSound()
    {
        
        audioSource.resource = abilityManager.currentAttack.attackSound;
        audioSource.Play();
    }
    
}
