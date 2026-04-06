using UnityEngine;

public class TrackLocation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (transform.parent == null)
            Debug.Log(gameObject.name + " Weapon lost parent");

        Debug.Log(gameObject.name + " " + transform.localPosition);
    }
}
