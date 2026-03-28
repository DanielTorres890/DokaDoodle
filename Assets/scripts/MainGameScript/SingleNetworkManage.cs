using UnityEngine;

public class SingleNetworkManage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public static SingleNetworkManage instance;


    public void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}
