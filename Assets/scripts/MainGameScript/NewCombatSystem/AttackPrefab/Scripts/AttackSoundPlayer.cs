using Unity.Netcode;
using UnityEngine;

public class AttackSoundPlayer : NetworkBehaviour
{
    public AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateVolume();
        SettingsManager.instance.onSFXVolumeChange.AddListener(UpdateVolume);
    }

    private void UpdateVolume()
    {
        audioSource.volume = SettingsManager.instance.SFXVolume;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void PlaySoundRpc(int audioId)
    {
        if(!audioSource) {  return; }
        if(audioId == -1) { return; }
        audioSource.clip = NetworkData.Instance.audioDataBase.GetItem[audioId];
        audioSource.Play();
    }
}
