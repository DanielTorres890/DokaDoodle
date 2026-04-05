
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileInfoDisplay : MonoBehaviour
{

    public TextMeshProUGUI tileDescription;

    public GameObject enemyText;

    public Transform textParent;

    public List<GameObject> spawnedChildren;
    public void UpdateText(TileScript lookedAtTile)
    {
        tileDescription.text = lookedAtTile.tileDescription;
        
        for(int i = spawnedChildren.Count - 1 ; i >= 0; i--)
        {
            Destroy(spawnedChildren[i]);
        }
        var thisTile = MapTileSpecialEvents.Instance.mapTiles[PlayerMoveManager.Instance.mapNumber][lookedAtTile.tileId];



        for (int i = 0; i < thisTile.tileEnemy.Count; i++)
        {
            
            var thisEnemy = PlayerCombatManager.Instance.EnemyDataBase.GetItem[thisTile.tileEnemy[i].enemyId];
            GameObject instance =  Instantiate(enemyText, textParent);

            instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "LVL " + thisEnemy.enemyLevel + ": " + thisEnemy.enemyName;


            
            var enemyAttributes = thisTile.tileEnemy[i].stats;
            
            //i could probably figture out some loop with ordered attributes but it aint worth it to me personally
            if(NetworkData.Instance.seenEnemies.Contains(thisTile.tileEnemy[i].enemyId))
            {

                instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "HP: " + enemyAttributes[Attributes.Health].ToString() + "/" + enemyAttributes[Attributes.MaxHealth].ToString();
                instance.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "ATK: " + enemyAttributes[Attributes.Attack].ToString();
                instance.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "DEF: " + enemyAttributes[Attributes.Defense].ToString();
                instance.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "MAG: " + enemyAttributes[Attributes.Magic].ToString();
                instance.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "MDEF: " + enemyAttributes[Attributes.MDefense].ToString();
                instance.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "DEX: " + enemyAttributes[Attributes.Dexterity].ToString();

            }

            spawnedChildren.Add(instance);
        }

        for (int i = 0; i < thisTile.players.Count; i++)
        {

            var thisPlayer = NetworkData.Instance.players[thisTile.players[i]];
            GameObject instance = Instantiate(enemyText, textParent);

            instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "LVL " + thisPlayer.playerInfo[PlayerInfo.level] + ": " + thisPlayer.name;



            var enemyAttributes = thisPlayer.stats;

            
            instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "HP: " + enemyAttributes[Attributes.Health].ToString() + "/" + enemyAttributes[Attributes.MaxHealth].ToString();
            instance.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "ATK: " + enemyAttributes[Attributes.Attack].ToString();
            instance.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "DEF: " + enemyAttributes[Attributes.Defense].ToString();
            instance.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "MAG: " + enemyAttributes[Attributes.Magic].ToString();
            instance.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "MDEF: " + enemyAttributes[Attributes.MDefense].ToString();
            instance.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "DEX: " + enemyAttributes[Attributes.Dexterity].ToString();

            

            spawnedChildren.Add(instance);
        }

        for (int i = 0; i < thisTile.partyMembers.Count; i++)
        {

            var thisPartyMemeber = thisTile.partyMembers[i];
            GameObject instance = Instantiate(enemyText, textParent);

            instance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "LVL " + thisPartyMemeber.allyInfo[PlayerInfo.level] + ": " + thisPartyMemeber.name;



            var enemyAttributes = thisPartyMemeber.stats;


            instance.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "HP: " + enemyAttributes[Attributes.Health].ToString() + "/" + enemyAttributes[Attributes.MaxHealth].ToString();
            instance.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "ATK: " + enemyAttributes[Attributes.Attack].ToString();
            instance.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "DEF: " + enemyAttributes[Attributes.Defense].ToString();
            instance.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "MAG: " + enemyAttributes[Attributes.Magic].ToString();
            instance.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "MDEF: " + enemyAttributes[Attributes.MDefense].ToString();
            instance.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "DEX: " + enemyAttributes[Attributes.Dexterity].ToString();



            spawnedChildren.Add(instance);
        }
    }
}
