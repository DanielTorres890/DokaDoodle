using PrimeTween;
using UnityEngine;

public class ArrowMove : MonoBehaviour
{
    public float highest = 2;
    public float speed = 1.5f;

    void Start()
    {

        Tween.Position(transform, transform.position + Vector3.up * highest, speed, ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
    }

    
}
