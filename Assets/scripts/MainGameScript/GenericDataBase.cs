using System.Collections.Generic;
using UnityEngine;

public class DataBase : ScriptableObject
{

}
//i'd just like to say that WHY ARE THERE 0 ONLINE RESOURCES ABOUT THE ORDER TO APPLY THINGS IN FMCL
public abstract class GenericDataBase<T> : DataBase, ISerializationCallbackReceiver where T : ScriptableObject
{
    public T[] items;

    public Dictionary<T, int> GetId = new Dictionary<T, int>();
    public Dictionary<int, T> GetItem = new Dictionary<int, T>();
    public void OnAfterDeserialize()
    {
        GetId = new Dictionary<T, int>();
        GetItem = new Dictionary<int, T>();
        for (int i = 0; i < items.Length; i++)
        {
            GetId.Add(items[i], i);
            GetItem.Add(i, items[i]);
        }

    }

    public void OnBeforeSerialize()
    {

    }
}
