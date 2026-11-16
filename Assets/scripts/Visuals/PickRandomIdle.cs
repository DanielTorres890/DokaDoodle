using System.Linq;
using UnityEngine;

public class PickRandomIdle : MonoBehaviour
{
    public Animator animator;

    public float idleFrequency;
    public float idleVariance;
    private float timer;

    public int[] animationWeights;

    [Tooltip("Play an animation immediately?")]
    public bool initialAnimation = false;

    public bool dontStackActions = false;
    void Start()
    {
        animator = GetComponent<Animator>();
        timer = Random.Range(0, idleFrequency * 2);
        if(initialAnimation) { timer = 0; }
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if( timer < 0)
        {
            if (dontStackActions && !animator.GetCurrentAnimatorStateInfo(0).IsName("default")) { return; }
            timer = idleFrequency + Random.Range(-idleVariance, idleVariance);
            
            animator.SetTrigger("action" + (pickAnimationFromWeight() + 1).ToString());
        }

    }

    private int pickAnimationFromWeight()
    {
        
        

        int totalWeight = 0;
        foreach(var single in animationWeights)
        {
            totalWeight += single;
        }

        int weight = 0;
        int rando = Random.Range(0, totalWeight);

        for (int i = 0; i < animationWeights.Length; i++)
        {
            weight += animationWeights[i];
            if(rando < weight)
            {
                return i;
            }
            
        }
        return 0;
    }
}
