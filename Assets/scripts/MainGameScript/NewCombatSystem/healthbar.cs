using JetBrains.Annotations;
using UnityEngine;

public class healthbar : MonoBehaviour
{
    public GameObject greenBar; 
    public AbilityManager manager;
    public float offset = 75f;
    private Vector3 startingSpot;

    public void SetUp()
    {
        gameObject.SetActive(true);
        manager.onHit.AddListener(UpdateSize);
        startingSpot = greenBar.transform.position;
        UpdateSize();
    }
    //this seems to work completely find for the main editor but stops working on the multiplay one (which im not sure why at all) should check if that happens in final version
    public void UpdateSize()
    {
        Debug.Log("do i be updating");
        greenBar.transform.localScale = new Vector3((float)manager.stats.stats[Attributes.Health] / manager.stats.stats[Attributes.MaxHealth],1,1);
        greenBar.transform.position = new Vector3( startingSpot.x, Mathf.Sin(Mathf.Deg2Rad * greenBar.transform.rotation.z) * offset * (1-((float)manager.stats.stats[Attributes.Health] / manager.stats.stats[Attributes.MaxHealth])) + startingSpot.y, startingSpot.z);
    }

}
