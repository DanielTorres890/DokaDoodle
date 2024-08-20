using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;



    private GameData gameData;
    public static DataPersistenceManager instance {  get; private set; }
    private List<IDataPersistance> dataPersistances;
    private FileDatahandler dataHandler;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("More than one persistance manager?");
        }
        instance = this;
    }
    private void Start()
    {
        this.dataHandler = new FileDatahandler(Application.persistentDataPath,fileName);
        this.dataPersistances = FindAllDataPersistanceObjects();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        this.gameData = dataHandler.Load();
        if (this.gameData == null)
        {
            Debug.Log("No Data was found. Initialized data to defaults");
            NewGame();
        }
        foreach (IDataPersistance persistance in dataPersistances)
        {
            persistance.LoadData(gameData);

        }


    }

    public void SaveGame()
    {
        this.gameData = new GameData();
        foreach (IDataPersistance persistance in dataPersistances)
        {
            persistance.SaveData(ref gameData);

        }
        dataHandler.Save(gameData);
    }

    private List<IDataPersistance> FindAllDataPersistanceObjects()
    {
        IEnumerable<IDataPersistance> dataPersitstanceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistance>();
        return new List<IDataPersistance>(dataPersitstanceObjects);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

}
