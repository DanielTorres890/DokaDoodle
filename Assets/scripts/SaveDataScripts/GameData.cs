using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData 
{
    public List<playerData> players;
    public List<List<List<int>>> inventoryObjects = new List<List<List<int>>>();

    public List<WorldEventWrapper> worldEvents;
    public List<int> completedEvents = new List<int>();

    public List<int> seenPopsUps = new List<int>();


    public int currentPlayer = 0;
    public int maxPlayers = 4;
    public int turns = 0;
    public int days = 0;
    public int weeks = 0;




    public SpecialTileEventHold[][] tileEvents;

    
}
