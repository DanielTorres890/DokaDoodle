using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class characterEditor : MonoBehaviour
{
    private List<FixedString32Bytes> defaultNames= new List<FixedString32Bytes>()
    {
        "Nicky", "Daniel", "Poopyhead", "Anthony", "Pat", "Ethan", "Vero"
    };
    // Start is called before the first frame update
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private SpriteLibraryAsset library;
    [SerializeField] private TMP_InputField inputField;

    [HideInInspector]public FixedString32Bytes playerName = "";
    [HideInInspector] public int playerClass = 0;
    [HideInInspector] public int playerFace = 0;
    [HideInInspector] public int playerHair = 0;

    public void setSprite(string spriteCat, string spriteName)
    {
        characterPrefab.transform.Find(spriteCat).GetComponent<SpriteRenderer>().sprite = library.GetSprite(spriteCat, spriteName);
    }
    public void setClass (int classNum)
    {
        
        playerClass = classNum;
        
        setSprite("outfit", library.GetCategoryLabelNames("outfit").ToList()[playerClass]);

        setSprite("rightLeg", library.GetCategoryLabelNames("rightLeg").ToList()[playerClass]);
        setSprite("leftLeg", library.GetCategoryLabelNames("leftLeg").ToList()[playerClass]);

    }
    public void setFace (int faceNum)
    {
        playerFace = faceNum;
        setSprite("face", library.GetCategoryLabelNames("face").ToList()[playerFace]);
    }
    public void setHair (int hairNum)
    {
        playerHair = hairNum;
        setSprite("hair", library.GetCategoryLabelNames("hair").ToList()[playerHair]);
    }

    public void nextFace()
    {
        List<string> faces = library.GetCategoryLabelNames("face").ToList();

        if (playerFace < faces.Count - 1) { playerFace++; }
        else {playerFace = 0; }
        setSprite("face", faces[playerFace]);
    }

    public void nextHair()
    {
        List<string> hairs = library.GetCategoryLabelNames("hair").ToList();

        if (playerHair < hairs.Count - 1) { playerHair++; }
        else { playerHair = 0; }

        setSprite("hair", hairs[playerHair]);
    }

    public void prevFace ()
    {
        List<string> faces = library.GetCategoryLabelNames("face").ToList();

        if (playerFace > 0) { playerFace--; }
        else { playerFace = faces.Count - 1; }

        setSprite("face", faces[playerFace]);
    }

    public void prevHair()
    {
        List<string> hairs = library.GetCategoryLabelNames("hair").ToList();

        if (playerHair > 0) { playerHair--; }
        else { playerHair = hairs.Count - 1; }

        setSprite("hair", hairs[playerHair]);
    }

    public void setName()
    {
        playerName = inputField.text;
    }

    public void sendData()
    {
        if (playerName == "" )
        {
            playerName = defaultNames[Random.Range(0, defaultNames.Count)];
        }
        playerData data = new playerData(playerClass, playerName, playerFace, playerHair);
        NetworkData.Instance.sendPlayerDataServerRpc(playerName,playerClass,playerFace,playerHair, new ServerRpcParams());
        
    }
}
