using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashCdDisplay : MonoBehaviour
{

    public TextMeshProUGUI cdText;
    public CombatantMovement playerMovement;
    public AbilityManager manager;
    public Slider greyBox;

    public void Start()
    {
       
    }
    public void SetUp()
    {

        gameObject.SetActive(NetworkData.Instance.dashDexReq <= manager.stats.stats[Attributes.Dexterity]);

    }

    // Update is called once per frame
    void Update()
    {
        if(playerMovement && manager)
        {
            cdText.text = Mathf.Clamp(Mathf.RoundToInt(playerMovement.dashCd - playerMovement.dashCdTimer), 0, 10).ToString();
            greyBox.value = playerMovement.dashCdTimer / playerMovement.dashCd;
        }
        
    }
}
