using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomItemSelect : MonoBehaviour
{
    public ItemTileRewards[] items;
    public GameObject[] itemDisplay;
    public int numberOfCycles = 5;
    public float TimeBetweenJumps;
    public float finalJump;
    //now that i think about it it doesn't have to be generated at run time ..... :)

    public void ShuffleDisplay(ItemTileRewards[] newItems, int selectedItemId)
    {
        ItemTileRewards selectedItem = newItems[selectedItemId];
        gameObject.SetActive(true);
        items = newItems;
        for (int i = 0; i < itemDisplay.Length; i++)
        {
            int randomIndex = Random.Range(0, items.Length );
            itemDisplay[i].GetComponentInChildren<TextMeshProUGUI>().text = items[randomIndex].rewardName;
            itemDisplay[i].transform.GetChild(0).GetComponent<Image>().sprite = items[randomIndex].rewardSprite;
        //ill be honest this is getting nasty but i gotta do what i gotta do
        }
        int randoIndex = Random.Range(0, itemDisplay.Length );
        itemDisplay[randoIndex].GetComponentInChildren<TextMeshProUGUI>().text = selectedItem.rewardName;
        itemDisplay[randoIndex].transform.GetChild(0).GetComponent<Image>().sprite = selectedItem.rewardSprite;
        StartCoroutine(ShuffleInRealTime(randoIndex, selectedItem));


    }


    private IEnumerator ShuffleInRealTime(int randoIndex, ItemTileRewards selectedReward)
    {
        int counter = 0;
        int cycleCounter = 0;
        while (counter != randoIndex || cycleCounter != numberOfCycles)
        {
            
            itemDisplay[counter].transform.SetAsLastSibling();
            yield return new WaitForSeconds(TimeBetweenJumps);
            itemDisplay[counter].transform.SetAsFirstSibling();
            counter++;
            if(counter >= itemDisplay.Length)
            {
                counter = 0;
                cycleCounter += 1;
            }
        }

        itemDisplay[randoIndex].transform.SetAsLastSibling();
        yield return new WaitForSeconds(finalJump);
        itemDisplay[randoIndex].transform.SetAsFirstSibling();
        gameObject.SetActive(false);
        selectedReward.GiveReward();
        //int itemId = NetworkData.Instance.playerInventories[0][selectedItem.determineType()].database.GetId[selectedItem];
        //ClientChecks.Instance.ConfirmItemPickup(NetworkData.Instance.currentPlayer, itemId, selectedItem.determineType());
    }
}
