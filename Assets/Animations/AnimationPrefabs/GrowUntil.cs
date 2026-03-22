using UnityEngine;

public class GrowUntil : MonoBehaviour
{
    public Vector3 targetSize;
    public float growSpeed = 0.2f;

    // Update is called once per frame
    void Update()
    {
        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;
        
    }
}
