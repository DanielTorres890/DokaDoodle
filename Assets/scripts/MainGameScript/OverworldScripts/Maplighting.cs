using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;

public class Maplighting : MonoBehaviour
{
    public WorldLightingWrapper[] worldLightingWrappers;
    public Light thisLight;

    void Start()
    {
        CheckLighting();
        ClientChecks.Instance.onEventActivate.AddListener(CheckLightingActivate);
    }
    public void CheckLighting()
    {
        thisLight = GetComponent<Light>();
        foreach (var wrapper in worldLightingWrappers)
        {
            foreach (var wEvent in WorldEventManager.Instance.activeWorldEvents)
            {
                WorldEventBase thisEvent = WorldEventManager.Instance.worldDatabase.GetItem[wEvent.eventId];
                if (thisEvent == wrapper.WorldEvent)
                {
                    thisLight.color = wrapper.colorToBe;
                    return;
                }
            }

        }
    }
    public void CheckLightingActivate(int eventId)
    {
        thisLight = GetComponent<Light>();
        foreach (var wrapper in worldLightingWrappers)
        {
                WorldEventBase thisEvent = WorldEventManager.Instance.worldDatabase.GetItem[eventId];
                if (thisEvent == wrapper.WorldEvent)
                {
                    Tween.LightColor(thisLight, wrapper.colorToBe, 3f);
                    return;
                }
        }
    }
    
}
[System.Serializable]
public class WorldLightingWrapper
{
    public WorldEventBase WorldEvent;
    public Color colorToBe;


}