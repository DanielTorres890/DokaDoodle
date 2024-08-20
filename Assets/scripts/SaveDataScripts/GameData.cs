using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData 
{
    public List<playerData> players;
    public List<List<List<int>>> inventoryObjects = new List<List<List<int>>>();


    public int currentPlayer = 0;
}
