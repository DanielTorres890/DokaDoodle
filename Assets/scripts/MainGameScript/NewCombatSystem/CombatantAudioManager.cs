using UnityEngine;

public class CombatantAudioManager : MonoBehaviour
{
    [SerializeField]private AudioSource audioSource;
    [SerializeField]private AbilityManager abilityManager;
    private void Awake()
    {
        TryGetComponent(out audioSource);
        TryGetComponent(out abilityManager);
        SettingsManager.instance.onSFXVolumeChange.AddListener(UpdateVolume);
    }

    public void PlayCurrentSound()
    {
        
        audioSource.resource = abilityManager.currentAttack.attackSound;
        UpdateVolume();
        audioSource.Play();
    }
    public void UpdateVolume()
    {
        audioSource.volume = SettingsManager.instance.SFXVolume;
    }
    
}
