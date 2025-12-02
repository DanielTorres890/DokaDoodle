using UnityEngine;
using UnityEngine.UI;

public class energybar : MonoBehaviour
{
    
    public Slider slider; //THIS IS WHY WE DO HW INSTEAD OF TRYING TO RAW DOG IT BRO
    public AbilityManager manager;
    public void SetUp()
    {
        slider = GetComponent<Slider>();
        gameObject.SetActive(true);
        manager.onEnergyChange.AddListener(UpdateSize);
        UpdateSize();
    }
    //this seems to work completely find for the main editor but stops working on the multiplay one (which im not sure why at all) should check if that happens in final version
    public void UpdateSize()
    {
        slider.value = manager.currentEnergy / manager.maxEnergy;
        
    }

}
