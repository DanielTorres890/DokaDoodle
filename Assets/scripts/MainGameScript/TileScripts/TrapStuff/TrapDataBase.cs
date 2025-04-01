using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//

[CreateAssetMenu(fileName = "New Item Database", menuName = "TileSystem/Traps/TrapDataBase")]
public class TrapDataBase : ScriptableObject, ISerializationCallbackReceiver
{
    public BaseTrap[] Items;

    public Dictionary<BaseTrap, int> GetId = new Dictionary<BaseTrap, int>();
    public Dictionary<int, BaseTrap> GetTrap = new Dictionary<int, BaseTrap>();

    //

    public void OnAfterDeserialize()
    {
        GetId = new Dictionary<BaseTrap, int>();
        GetTrap = new Dictionary<int, BaseTrap>();
        for (int i = 0; i < Items.Length; i++)
        {
            GetId.Add(Items[i], i);
            GetTrap.Add(i, Items[i]);
        }

    }

    public void OnBeforeSerialize()
    {

    }
}
