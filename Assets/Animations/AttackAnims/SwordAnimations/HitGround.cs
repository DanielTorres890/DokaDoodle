using UnityEngine;

public class HitGround : MonoBehaviour
{
    public Animator animator;
    public PickUp obj;
    public void Start()
    {
        animator = GetComponent<Animator>();
        obj.onStopMove.AddListener(SetSpin);
    }
    private void SetSpin()
    {
        animator.SetBool("Hit Ground", true);
    }
}
