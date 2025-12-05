using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomItemSelect : MonoBehaviour
{
    public ItemBase[] items;
    public GameObject[] itemDisplay;
    public int numberOfJumps;
    public float TimeBetweenJumps;
    public float finalJump;
    //now that i think about it it doesn't have to be generated at run time ..... :)

    public void ShuffleDisplay(ItemBase[] newItems, int selectedItemId, int inventoryType)
    {
        ItemBase selectedItem = NetworkData.Instance.playerInventories[0][inventoryType].database.GetItem[selectedItemId];
        gameObject.SetActive(true);
        items = newItems;
        for (int i = 0; i < itemDisplay.Length; i++)
        {
            int randomIndex = Random.Range(0, items.Length - 1);
            itemDisplay[i].GetComponentInChildren<TextMeshProUGUI>().text = items[randomIndex].itemName;
            itemDisplay[i].transform.GetChild(0).GetComponent<Image>().sprite = items[randomIndex].itemSprite;
        //ill be honest this is getting nasty but i gotta do what i gotta do
        }
        int randoIndex = Random.Range(0, itemDisplay.Length - 1);
        itemDisplay[randoIndex].GetComponentInChildren<TextMeshProUGUI>().text = selectedItem.itemName;
        itemDisplay[randoIndex].transform.GetChild(0).GetComponent<Image>().sprite = selectedItem.itemSprite;
        StartCoroutine(ShuffleInRealTime(randoIndex, selectedItem));


    }


    private IEnumerator ShuffleInRealTime(int randoIndex,ItemBase selectedItem)
    {
        int counter = numberOfJumps;
        while (counter > 0)
        {
            int previousRandom = Random.Range(0, itemDisplay.Length - 1);
            itemDisplay[previousRandom].transform.SetAsLastSibling();
            yield return new WaitForSeconds(TimeBetweenJumps);
            itemDisplay[previousRandom].transform.SetAsFirstSibling();
            counter--;
        }

        itemDisplay[randoIndex].transform.SetAsLastSibling();
        yield return new WaitForSeconds(finalJump);
        itemDisplay[randoIndex].transform.SetAsFirstSibling();
        gameObject.SetActive(false);
        int itemId = NetworkData.Instance.playerInventories[0][selectedItem.determineType()].database.GetId[selectedItem];
        ClientChecks.Instance.ConfirmItemPickup(NetworkData.Instance.currentPlayer, itemId, selectedItem.determineType());
    }
}
