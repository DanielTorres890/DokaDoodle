using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public float Timer = 10f;
    [SerializeField] private Animator m_Animator;
    private List<string> animationNames = new List<string> {"DoAnimation1", "DoAnimation2"};

    private void Start()
    {
        Timer = Random.Range(7f, 15f);
    }
    private void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer < 0f)
            
        {
            m_Animator.SetTrigger(animationNames[Random.Range(0,animationNames.Count)]);

            Timer = Random.Range(7f,15f);


        }
        
        

    }

    
}
