using Unity.Netcode;
using UnityEngine;

public class OnlyHost : MonoBehaviour
{

    public void UpdateCondition()
    {
        
        gameObject.SetActive(NetworkData.Instance.IsHost);
    }
    public void Update()
    {
        if (NetworkManager.Singleton != null)
            UpdateCondition();
    }
}
