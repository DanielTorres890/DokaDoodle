using UnityEngine;

public class SunshineUpdate : MonoBehaviour
{
    public ParticleSystem particles;
    public BuffBase release;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        WorldEventManager.Instance.onDayChange.AddListener(UpdateParticles);
        UpdateParticles();
        if (NewCombatManager.instance)
        {
            Debug.Log("I should subscribe");
            transform.parent.GetComponent<AbilityManager>().onStatus.AddListener(UpdateParticles);
            
        }
    }

    public void UpdateParticles()
    {
        var color = particles.main.startColor.color;
        var main = particles.main;
        
        if(NewCombatManager.instance)
        {
            EntityStats owner = transform.parent.GetComponent<AbilityManager>().stats;
            foreach(var status in owner.statuses)
            {
                if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] == release) 
                {
                    Debug.Log("YO IM UPDATING HELLLLLOOO");
                    main.startColor = new Color(color.r, color.g, color.b, 1);
                    
                    return; 
                
                }
            }
        }

        main.startColor = new Color(color.r, color.g, color.b, (-Mathf.Cos((float)WorldEventManager.Instance.days / (WorldEventManager.Instance.daysPerWeek - 1) * 2 * Mathf.PI) + 1)/2f);
    }

    private void FindOwner()
    {

    }
}
