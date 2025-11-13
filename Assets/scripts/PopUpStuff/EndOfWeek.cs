using TMPro;
using UnityEngine;

public class EndOfWeek : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Start()
    {
        string popUpString = "End of week gold gains \n";
        for (int i = 0; i < NetworkData.Instance.maxPlayers; i++)
        {
            playerData player = NetworkData.Instance.players[i];
            popUpString += player.name + " made ";
            int madeMoney = 0;
            foreach (int tileid in player.ownedTowns)
            {
                var curTile = MapTileSpecialEvents.Instance.mapTiles[0][tileid];
                
                madeMoney += (curTile.townMoneyLevel + 1) * NetworkData.Instance.TownInfoDataBase.GetItem[curTile.townId].baseMoneyGeneration;
                player.playerInfo[PlayerInfo.money] += madeMoney;
                
                Debug.Log("Gained Money from town");
            }
            popUpString += madeMoney.ToString();
            popUpString += " money from their towns" + "\n\n";
        }
        text.text = popUpString;
    }

}
