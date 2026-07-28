using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllySkillButton : MonoBehaviour
{
    public Image skillImage;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDescription;
    public void SetUp(WeaponItem skill)
    {
        skillImage.sprite = skill.itemSprite;
        skillName.text = skill.attack[0].attackName;
        skillDescription.text = skill.attack[0].description;

    }
}
