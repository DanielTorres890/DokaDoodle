using UnityEngine;
using UnityEngine.Events;

public class LaserFireVisual : MonoBehaviour
{
    public Transform growingObject;
    public Vector3 growthDirection;
    public float growthSpeed;
    public float growthDuration;

    public float shrinkSpeed;
    public Vector3 shrinkDirection;
    public bool shrinking = false;

    public UnityEvent durationFinish;
    private bool finished = false;
    [SerializeField] private bool begin = false;

    // Update is called once per frame
    void Update()
    {
        if(!begin) { return; }

        if(!shrinking)
        {
            growthDuration -= Time.deltaTime;

            growingObject.localScale += growthDirection * growthSpeed;

            if (!finished && growthDuration <= 0)
            {
                finished = true;
                durationFinish.Invoke();
            }
        }
        else
        {
            growingObject.localScale -= shrinkDirection * shrinkSpeed;
            growingObject.localScale = new Vector3(Mathf.Clamp(growingObject.localScale.x, 0, 999), Mathf.Clamp(growingObject.localScale.y, 0, 999), Mathf.Clamp(growingObject.localScale.z, 0, 999));
        }



    }
    public void Begin()
    {
        begin = true;
    }
    public void Shrink()
    {
        shrinking = true;
    }
}
