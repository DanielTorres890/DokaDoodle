using UnityEngine;
using UnityEngine.Events;

public class LaserFireVisual : MonoBehaviour
{
    public Transform growingObject;
    public Vector3 growthDirection;
    public float growthSpeed;
    public float growthDuration;

    public UnityEvent durationFinish;
    private bool finished = false;
    private bool begin = false;

    // Update is called once per frame
    void Update()
    {
        if(!begin) { return; }

        growthDuration -= Time.deltaTime;

        growingObject.localScale += growthDirection * growthSpeed;

        if(!finished && growthDuration <= 0)
        {
            finished = true;
            durationFinish.Invoke();
        }



    }
    public void Begin()
    {
        begin = true;
    }
}
