using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SaveDataDisplay : MonoBehaviour
{
    [SerializeField] private GameObject[] saveFileDisplays;
    [SerializeField] private TextMeshProUGUI[] playerInfoDisplay;
    [SerializeField] private GameObject parentSaveFile;
    [SerializeField] private GameObject confirmSave;
    private int currentLook;

    private GameData[] tempData;

    public UnityEvent onDataLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnEnable()
    {
        tempData = new GameData[saveFileDisplays.Length];
        SetUpDisplay();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUpDisplay()
    {
        foreach(var text in playerInfoDisplay)
        {
            text.text = "";
        }
        for(int i = 0; i < DataPersistenceManager.instance.fileNames.Count; i++)
        {
            if (DataPersistenceManager.instance.DataExists(i))
            {
                saveFileDisplays[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = "File " + (i+1).ToString();
                tempData[i] = DataPersistenceManager.instance.LoadTempData(i);
            }
            else
            {
                saveFileDisplays[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = "Empty File";
            }
        }
    }

    public void ConfirmSaveOption(int fileNumber)
    {
        currentLook = fileNumber;
        confirmSave.SetActive(true);
        parentSaveFile.SetActive(false);
    }
    public void ReturnSaveOption()
    {
        confirmSave.SetActive(false);
        parentSaveFile.SetActive(true);
    }
    public void ConfirmLoadOption(int fileNumber)
    {
        if (tempData[fileNumber] == null) { return; }

        ConfirmSaveOption(fileNumber);
    }


     public void SaveData()
    {
        DataPersistenceManager.instance.SaveGame(currentLook);
        SetUpDisplay();
        ReturnSaveOption();
    }

    public void DisplayGameInfo(int dataNumebr)
    {
        if (tempData[dataNumebr] == null) { return; }


        for(int i = 0; i < tempData[dataNumebr].players.Count; i++)
        {
            playerData thisPlayer = tempData[dataNumebr].players[i];
            playerInfoDisplay[i].text = thisPlayer.name + ": LVL " + thisPlayer.playerInfo[PlayerInfo.level].ToString(); 
        }

    }

    public void LoadData()
    {
        DataPersistenceManager.instance.LoadGame(currentLook);
        confirmSave.SetActive(false);
        parentSaveFile.SetActive(false);
        onDataLoad.Invoke();
    }
}
