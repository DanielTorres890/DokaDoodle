using UnityEngine;

public class PickRandomIdle : MonoBehaviour
{
    public Animator animator;

    public float idleFrequency;
    public float idleVariance;
    private float timer;
    void Start()
    {
        animator = GetComponent<Animator>();
        timer = Random.Range(0, idleFrequency * 2);
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if( timer < 0)
        {
            timer = idleFrequency + Random.Range(-idleVariance, idleVariance);
            animator.SetTrigger("action" + (Random.Range(0, animator.parameterCount) + 1).ToString());
        }

    }
}
