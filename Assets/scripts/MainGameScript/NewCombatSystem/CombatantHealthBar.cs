using Unity.Netcode;
using UnityEngine;

public class CombatantHealthBar : MonoBehaviour
{
    public AbilityManager myManager;
    public GameObject greenBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myManager.onHit.AddListener(UpdateBar);
        UpdateBar();
        
    }

    private void UpdateBar()
    {
        float healthRatio = (float)myManager.stats.stats[Attributes.Health] / myManager.stats.stats[Attributes.MaxHealth];
        if(healthRatio < 0) healthRatio = 0;

        greenBar.transform.localScale = new Vector3(healthRatio, greenBar.transform.localScale.y, greenBar.transform.localScale.z);
    }
}
