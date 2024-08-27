using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

[CreateAssetMenu(fileName = "New Default Object", menuName = "PlayerClass/DataBase")]
public class ClassDataBase : ScriptableObject, ISerializationCallbackReceiver
{
    //
    public PlayerClassBase[] Classes;

    public Dictionary<PlayerClassBase, int> GetId = new Dictionary<PlayerClassBase, int>();
    public Dictionary<int, PlayerClassBase> GetClass = new Dictionary<int, PlayerClassBase>();

    public void OnAfterDeserialize()
    {
        GetId = new Dictionary<PlayerClassBase, int>();
        GetClass = new Dictionary<int, PlayerClassBase>();
        for (int i = 0; i < Classes.Length; i++)
        {
            GetId.Add(Classes[i], i);
            GetClass.Add(i, Classes[i]);
        }

    }

    public void OnBeforeSerialize()
    {
        
    }
}
