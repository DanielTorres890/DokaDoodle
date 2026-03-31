using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoDisplay : MonoBehaviour
{
    //long ahh names
    public TextMeshProUGUI className;
    public TextMeshProUGUI overworldAbilityDescription;
    public TextMeshProUGUI inCombatAbilityDescription;
    public TextMeshProUGUI classLevel;
    
    public TextMeshProUGUI weaponName;
    public Image weaponSprite;


    public Transform weaponAbilityDescriptionParent;
    public GameObject weaponAbilityDescriptionPrefab;
    public List<GameObject> spawnedPrefabs;

    public void OnEnable()
    {
        UpdateText(NetworkData.Instance.currentPlayer);
    }

    // Update is called once per frame
    public void UpdateText(int playerNumber)
    {
        var thisPlayer = NetworkData.Instance.players[playerNumber];
        var thisClass = NetworkData.Instance.classDataBase.GetItem[thisPlayer.playerClass];


        for(int i = spawnedPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedPrefabs[i]);
        }


        className.text = "Class: " + thisClass.className;
        overworldAbilityDescription.text = thisClass.overworldAbilityDescription;
        inCombatAbilityDescription.text = thisClass.combatAbility.description;
        classLevel.text = "Class Level: " + (NetworkData.Instance.players[playerNumber].playerClassProgress[NetworkData.Instance.classDataBase.GetId[thisClass]].level + 1).ToString();


        if (thisPlayer.equipItems[ItemType.Equipment] != -1)
        {
            var heldWeapon = (NetworkData.Instance.playerInventories[playerNumber][3].database.GetItem[thisPlayer.equipItems[ItemType.Equipment]] as WeaponItem);
            weaponName.text = "Weapon: " + heldWeapon.itemName;
            weaponSprite.sprite = heldWeapon.itemSprite;
            weaponSprite.gameObject.SetActive(true);
            for(int i = 0; i < heldWeapon.attack.Length; i++)
            {
                GameObject instance = Instantiate(weaponAbilityDescriptionPrefab, weaponAbilityDescriptionParent);
                spawnedPrefabs.Add(instance);
                instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = heldWeapon.attack[0].attackName;
                instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = heldWeapon.attack[0].description;

            }

        }
        else { weaponName.text = "Weapon: None"; weaponSprite.gameObject.SetActive(false); }

    }
}
