using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData 
{
    public List<playerData> players;
    public List<List<List<int>>> inventoryObjects = new List<List<List<int>>>();

    public List<WorldEventWrapper> worldEvents;
    public List<int> seenPopsUps = new List<int>();

    public SpecialTileEventHold[][] tileEvents;

    public int currentPlayer = 0;
    public int maxPlayers = 4;
}
