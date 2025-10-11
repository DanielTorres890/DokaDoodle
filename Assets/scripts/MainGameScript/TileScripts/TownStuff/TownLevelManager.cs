using UnityEngine;
using UnityEngine.UI;

public class TownLevelManager : MonoBehaviour
{
    public GameObject MoneySlider;
    public GameObject DefenseSlider;
    public GameObject UnitSlider;

    private void Start()
    {
        UpdateMoneySlider();
        UpdateDefenseSlider();
        UpdateUnitSlider();
    }
    public void UpdateMoneySlider()
    {
        var slide = MoneySlider.GetComponent<Slider>();
        slide.value = MapTileSpecialEvents.Instance.GetCurrentTile().townMoneyLevel;
    }
    public void UpdateDefenseSlider()
    {
        var slide = DefenseSlider.GetComponent<Slider>();
        slide.value = MapTileSpecialEvents.Instance.GetCurrentTile().defenseLevel;
    }
    public void UpdateUnitSlider()
    {
        var slide = UnitSlider.GetComponent <Slider>();
        slide.value = MapTileSpecialEvents.Instance.GetCurrentTile().unitLevel;
    }

}
