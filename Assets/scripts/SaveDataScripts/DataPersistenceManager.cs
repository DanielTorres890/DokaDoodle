using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    public List<string> fileNames;



    private GameData gameData;
    public static DataPersistenceManager instance {  get; private set; }
    public List<IDataPersistance> dataPersistances;
    private List<FileDatahandler> dataHandler = new List<FileDatahandler>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

    }
    private void Start()
    {

        
        foreach (var filename in fileNames)
        {
            this.dataHandler.Add(new FileDatahandler(Application.persistentDataPath, filename));
        }
        
        this.dataPersistances = FindAllDataPersistanceObjects();
        Debug.Log("How many persistances are there? " + this.dataPersistances.Count);
        foreach(var data in this.dataPersistances)
        {
            
            Debug.Log((data as MonoBehaviour).name + " " + (data as MonoBehaviour).GetInstanceID());
       
        }

        

        //LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public bool LoadGame(int fileNumber)
    {

        this.dataPersistances = FindAllDataPersistanceObjects();
        Debug.Log("How many persistances are there? " + this.dataPersistances.Count);
        foreach (var data in this.dataPersistances)
        {

            Debug.Log((data as MonoBehaviour).name + " " + (data as MonoBehaviour).GetInstanceID());

        }

        this.gameData = dataHandler[fileNumber].Load();
        if (this.gameData == null)
        {
            Debug.Log("No Data was found...");
            return false;
        }
        
        foreach (IDataPersistance persistance in dataPersistances)
        {
            
            persistance.LoadData(gameData);

        }
        return true;

    }

    public void SaveGame(int fileNumber)
    {
        this.dataPersistances = FindAllDataPersistanceObjects();
        Debug.Log("How many persistances are there? " + this.dataPersistances.Count);
        foreach (var data in this.dataPersistances)
        {

            Debug.Log((data as MonoBehaviour).name + " " + (data as MonoBehaviour).GetInstanceID());

        }

        this.gameData = new GameData();
        foreach (IDataPersistance persistance in dataPersistances)
        {
            persistance.SaveData(ref gameData);

        }

        dataHandler[fileNumber].Save(gameData);
    }
    public bool DataExists(int fileNumber)
    {
        GameData exists = dataHandler[fileNumber].Load();
        if(exists == null) { return false; }
        return true;
    }
    public GameData LoadTempData(int fileNumber)
    {
        return dataHandler[fileNumber].Load();
    }
    
    public void LoadDataFromString(string jsonString)
    {

        Debug.Log("I shouldnt be loading from here...");
         gameData = JsonConvert.DeserializeObject<GameData>(jsonString);
        if (this.gameData == null)
        {
            Debug.Log("No Data was found...");
        }
        foreach (IDataPersistance persistance in dataPersistances)
        {
            persistance.LoadData(gameData);

        }

    }
    private List<IDataPersistance> FindAllDataPersistanceObjects()
    {

        IEnumerable<IDataPersistance> dataPersitstanceObjects =
        FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .OfType<IDataPersistance>()
        .Where(x => x != null);
        return new List<IDataPersistance>(dataPersitstanceObjects);
    }
    
    private void OnApplicationQuit()
    {
        //SaveGame();
    }
    public GameData GetCurrentGameData()
    {
        return gameData;
    }
}
