using UnityEngine;

public class OnlyHost : MonoBehaviour
{

    public void UpdateCondition()
    {
        gameObject.SetActive(NetworkData.Instance.IsHost);
    }

}
