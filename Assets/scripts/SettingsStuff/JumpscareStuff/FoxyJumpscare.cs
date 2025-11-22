using UnityEngine;
using UnityEngine.Video;

public class FoxyJumpscare : MonoBehaviour
{
    public VideoPlayer foxyJumpScare;
    public float jumpscareTimer;
    [Tooltip("How often should it roll for a jumpscare 1 = every second")]
    public float jumpscareFrequency;

    [Tooltip("The odds over 1 of a jumpscare 5 = 1/5 chance every roll")]
    public int jumpscareOdds;

    public GameObject videoPlayerRender;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(SettingsManager.instance == null) { return; }
        if(!SettingsManager.instance.canJumpscare) { return; } 


         jumpscareTimer += Time.deltaTime;
        if( jumpscareTimer > jumpscareFrequency ) 
        { 
            jumpscareTimer = 0; 
            if( Random.Range(0,jumpscareOdds) == 0)
            {
                Instantiate(videoPlayerRender);
                
                foxyJumpScare.Play();
            }
        }
    }
}
