using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MapTileSpecialEvents : NetworkBehaviour
{
    public SpecialTileEventHold[][] mapTiles = new SpecialTileEventHold[10][];
  

    public static MapTileSpecialEvents Instance;
    // Start is called before the first frame update
    public void Awake()
    {
        Debug.Log("I should be setup");
        Instance = this;
        this.mapTiles = new SpecialTileEventHold[10][];
    }

  
}
