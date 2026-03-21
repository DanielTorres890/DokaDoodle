using UnityEngine;

public class SunshineUpdate : MonoBehaviour
{
    public ParticleSystem particles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        WorldEventManager.Instance.onDayChange.AddListener(UpdateParticles);
        UpdateParticles();
    }

    public void UpdateParticles()
    {
        var color = particles.main.startColor.color;
        var main = particles.main;
        
        main.startColor = new Color(color.r, color.g, color.b, (-Mathf.Cos((float)WorldEventManager.Instance.days / (WorldEventManager.Instance.daysPerWeek - 1) * 2 * Mathf.PI) + 1)/2f);
    }

    private void FindOwner()
    {

    }
}
