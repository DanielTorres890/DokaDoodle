using UnityEngine;

public class SFXManager : MonoBehaviour
{

    public static SFXManager Instance;

    public AudioSource sfxPlayer;

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            sfxPlayer = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }


    }


    public void Start()
    {
        SettingsManager.instance.onSFXVolumeChange.AddListener(UpdateVolume);
        UpdateVolume();
    }

    private void UpdateVolume()
    {
       
        sfxPlayer.volume = SettingsManager.instance.SFXVolume;

    }
    public void PlaySFX(AudioClip clip)
    {
        sfxPlayer.clip = clip;
        sfxPlayer.Play();
    }
    
}
