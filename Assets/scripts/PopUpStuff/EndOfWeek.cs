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
            foreach (var townid in player.ownedTowns)
            {
                var curTile = MapTileSpecialEvents.Instance.mapTiles[0][townid.Value];
                
                madeMoney += (curTile.townMoneyLevel + 1) * NetworkData.Instance.TownInfoDataBase.GetItem[townid.Key].baseMoneyGeneration;
                player.playerInfo[PlayerInfo.money] += madeMoney;
                
                Debug.Log("Gained Money from town");
            }
            popUpString += madeMoney.ToString();
            popUpString += " money from their towns (Total Gold: " + player.playerInfo[PlayerInfo.money] + ")" + "\n\n";
        }
        text.text = popUpString;
    }

}
