using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    public AudioDataBase BGMDataBase;
    public AudioSource BGMSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    public void Start()
    {
        BGMSource = GetComponent<AudioSource>();
        SettingsManager.instance.onBackgroundVolumeChange.AddListener(OnVolumeChange);
        OnVolumeChange();
        PlaySound(0);
        
    }

    private void OnVolumeChange()
    {
        BGMSource.volume = SettingsManager.instance.volume;
    }
    public void PlaySound(int soundID)
    {
        
        BGMSource.resource = BGMDataBase.GetItem[soundID];
        BGMSource.Play();
    }
    public void PlaySound(AudioClip soundClip)
    {
       
        BGMSource.resource = soundClip;
        BGMSource.Play();
    }
    public void StopSounds(Scene scene, LoadSceneMode type)
    {
        if(type == LoadSceneMode.Additive) { return; }
        BGMSource.Stop();
    }
    public void StopSounds()
    {
        BGMSource.Stop();
    }
}
