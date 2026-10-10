using UnityEngine;
using UnityEngine.UI;

public class MainUIHealthbar : MonoBehaviour
{
    
    public Slider sliderBar;
    public Image greenBar;
    void Start()
    {
        UpdateBar();
        ClientChecks.Instance.onClassAbilityUse.AddListener(UpdateBar);
        ClientChecks.Instance.onItemUse.AddListener(UpdateBar);
        ClientChecks.Instance.onRoundStart.AddListener(UpdateBar);

    }

    // Update is called once per frame
    public void UpdateBar()
    {
        float healthDecimal = (float)NetworkData.Instance.GetCurrentPlayer().stats[Attributes.Health] / NetworkData.Instance.GetCurrentPlayer().stats[Attributes.MaxHealth];
        sliderBar.value = healthDecimal;
        greenBar.color = new Color(1 - healthDecimal, healthDecimal, 0);
    }
}
