using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WorldEvent Database", menuName = "WorldEvents/Database")]
public class WorldEventDataBase : ScriptableObject, ISerializationCallbackReceiver
{
    public WorldEventBase[] Items;

    public Dictionary<WorldEventBase, int> GetId = new Dictionary<WorldEventBase, int>();
    public Dictionary<int,WorldEventBase> GetEvent = new Dictionary<int, WorldEventBase>();

    //

    public void OnAfterDeserialize()
    {
        GetId = new Dictionary<WorldEventBase, int>();
        GetEvent = new Dictionary<int, WorldEventBase>();
        for (int i = 0; i < Items.Length; i++)
        {
            GetId.Add(Items[i], i);
            GetEvent.Add(i, Items[i]);
        }

    }

    public void OnBeforeSerialize()
    {

    }
}
