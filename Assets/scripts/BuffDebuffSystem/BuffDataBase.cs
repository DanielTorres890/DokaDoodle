using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Database", menuName = "StatusEffects/Database")]
public class BuffDataBase : ScriptableObject, ISerializationCallbackReceiver
{
    public BuffBase[] buffs;

    public Dictionary<BuffBase, int> GetId = new Dictionary<BuffBase, int>();
    public Dictionary<int, BuffBase> GetBuff = new Dictionary<int, BuffBase>();

    

    public void OnAfterDeserialize()
    {
        GetId = new Dictionary<BuffBase, int>();
        GetBuff = new Dictionary<int, BuffBase>();
        for (int i = 0; i < buffs.Length; i++)
        {
            GetId.Add(buffs[i], i);
            GetBuff.Add(i, buffs[i]);
        }

    }

    public void OnBeforeSerialize()
    {

    }
}
