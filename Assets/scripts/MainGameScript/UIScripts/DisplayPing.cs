using TMPro;
using Unity.Netcode;
using UnityEngine;

public class DisplayPing : MonoBehaviour
{
    public TextMeshProUGUI pingText;

    public float timer;
    public float refreshSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pingText = GetComponent<TextMeshProUGUI>();
        if(NetworkManager.Singleton.IsHost) { gameObject.SetActive(false); }
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            timer = refreshSpeed;
            pingText.text = "ms: " + ((int)(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId))).ToString();
        }
    }
}
