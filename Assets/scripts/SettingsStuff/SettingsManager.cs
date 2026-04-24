using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SettingsManager : MonoBehaviour
{

    public static SettingsManager instance;
    public float volume;
    public float SFXVolume;
    public float mouseSense;
    public InputActionAsset settings;
    public UnityEvent onSFXVolumeChange;
    public UnityEvent onBackgroundVolumeChange;



    public CursorLockMode previousMode;
    public bool settingsOpen = false;

    public bool canJumpscare;





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
        settings.actionMaps[0].FindAction("Settings").performed += SettingsScene;
        settings.actionMaps[1].FindAction("Settings").performed += SettingsScene;
        settings.actionMaps[2].FindAction("Settings").performed += SettingsScene;
        
    }
    public void SettingsScene(InputAction.CallbackContext context)
    {
        

        if(settingsOpen)
        {
            settingsOpen = false;
            Cursor.lockState = previousMode;
            SceneManager.UnloadSceneAsync("Settings");
        }
        else
        {

            settingsOpen = true;
            previousMode = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadSceneAsync("Settings", LoadSceneMode.Additive);
        }

    }

    public void OnApplicationFocus(bool focus)
    {
       

        if(!settingsOpen) { previousMode = Cursor.lockState; }

        if(!focus) { return; }
        if(settingsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = previousMode;
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
