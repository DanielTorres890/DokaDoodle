using UnityEngine;

public class DesyncWalk : MonoBehaviour
{
    public Animator animator;
    public string AnimName = "Walk";
    void Start()
    {
        Desync();
    }

    // Update is called once per frame
    public void Desync()
    {
        animator.Play(AnimName, 0, Random.Range(0,1f));
    }
}
