using UnityEngine;

public class SFXManager : MonoBehaviour
{

    public static SFXManager Instance;
    public AudioSource audioPlayer;


    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            audioPlayer = GetComponent<AudioSource>();
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
        audioPlayer.volume = SettingsManager.instance.SFXVolume;
        Debug.Log("Im mogging");
    }
    public void PlaySFX(AudioClip clip)
    {
        audioPlayer.clip = clip;
        audioPlayer.Play();
    }
}
