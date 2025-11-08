using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{

    public static SettingsManager instance;
    public float volume;
    public float SFXVolume;
    public InputActionAsset settings;
    public UnityEvent onSFXVolumeChange;
    public UnityEvent onBackgroundVolumeChange;

    private bool settingsOpen = false;
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
    private void Start()
    {
        settings.actionMaps[1].FindAction("Settings").performed += SettingsScene;
        settings.actionMaps[2].FindAction("Settings").performed += SettingsScene;
        settings.actionMaps[3].FindAction("Settings").performed += SettingsScene;
    }
    public void SettingsScene(InputAction.CallbackContext context)
    {
        if(settingsOpen)
        {
            settingsOpen = false;
            SceneManager.UnloadSceneAsync("Settings");
        }
        else
        {
            settingsOpen = true;
            SceneManager.LoadSceneAsync("Settings", LoadSceneMode.Additive);
        }
    }
    public void ChangeSFXVolume(int value)
    {
        SFXVolume = value / 100f;
        onSFXVolumeChange.Invoke();
    }
    public void ChangeBGMVolume(int value)
    {
        volume = value / 100f;
        onBackgroundVolumeChange.Invoke();
    }
}
