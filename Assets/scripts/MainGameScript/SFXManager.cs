using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SFXManager : MonoBehaviour
{

    public static SFXManager Instance;

    public AudioSource sfxPlayer;

    public void Awake()
    {
        Instance = this;
        sfxPlayer = GetComponent<AudioSource>();
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
    public void PlaySFXSeperate(AudioClip clip)
    {
        GameObject tempGO = new GameObject("TempAudioPool"); 
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = sfxPlayer.volume;
        aSource.spatialBlend = 0;
        aSource.Play();

        Destroy(tempGO, clip.length);
    }
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
    {
        GameObject tempGO = new GameObject("TempAudioPool");
        tempGO.transform.position = position;

      
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = sfxPlayer.volume;
        

       
        aSource.spatialBlend = 1.0f;
        aSource.rolloffMode = AudioRolloffMode.Logarithmic;
        aSource.minDistance = 1f;
        aSource.maxDistance = 50f;
        aSource.Play();
        Destroy(tempGO, clip.length);
    }
    public void PlaySfxDeplayed(AudioClip clip, float delay)
    {
        sfxPlayer.clip = clip;
        sfxPlayer.PlayDelayed(delay);
    }

}
