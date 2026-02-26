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
    public RenderTexture myTexture;
    public Material myMaterial;
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

    //i made it a const to be cool 
    private const int defaultHairCount = 7;

    public GameObject previewLoaded;
    public GameObject editor;
    public void UpdateMaterial()
    {
        myMaterial.mainTexture = myTexture;

    }
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

        
        if (playerHair < defaultHairCount - 1) { playerHair++; }
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
        else { playerHair = defaultHairCount - 1; }

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
        NetworkData.Instance.sendPlayerDataServerRpc(playerName,playerClass,playerFace,playerHair, new ServerRpcParams());
        
    }
    
}
