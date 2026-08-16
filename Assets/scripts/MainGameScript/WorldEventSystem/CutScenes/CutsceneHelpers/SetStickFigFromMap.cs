using UnityEngine;

public class SetStickFigFromMap : MonoBehaviour
{
    public characterEditor stickEditor;
    public int mapNum;


   
    void Awake()
    {
        foreach(var player in NetworkData.Instance.players)
        {
            if(player.curMap == mapNum)
            {
                stickEditor.setClass(player.playerClass);
                stickEditor.setFace(player.playerFace);
                stickEditor.setHair(player.playerHair);
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
