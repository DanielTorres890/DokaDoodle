using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class EntityStats 
{
    public string name;

    public bool isDead = false;
    public List<string> loyaltyTags = new List<string>();

    public List<BuffHolder> statuses = new List<BuffHolder>();

    public Dictionary<Attributes, int> stats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0 }


    };
    public Dictionary<Attributes, int> postStatusStats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0}
        

    };
    public Dictionary<AttackTypes, int> dmgReduction = new Dictionary<AttackTypes, int> 
    {
        { AttackTypes.Physical, 0},
        { AttackTypes.Magic, 0}

    };
    [SerializeField] public List<AttackBase> attacks = new List<AttackBase>();
    [SerializeField] public DefenseBase[] defenses = new DefenseBase[4];



    public float speedFormula()
    {
        return Mathf.Sqrt(stats[Attributes.Dexterity])/2;
    }
    public float dashFormula()
    {
        return Mathf.Sqrt(stats[Attributes.Dexterity]) * 20;
    }

    public void ChangeBaseStat(Attributes attr, int amt)
    {
        stats[attr] += amt;
        PostStatusStatCalc();
        
    }
    public void SetBaseStat(Attributes attr, int amt)
    {
        stats[attr] = amt;
        PostStatusStatCalc();
    }
    public void GainStatus(BuffBase status)
    {
        bool alreadyAfflicted = false;
        foreach (var stati in statuses)
        {
            if(stati.buffId == NetworkData.Instance.buffDataBase.GetId[status])
            {
                stati.timeRemaining += status.duration;
                alreadyAfflicted = true;
                break;
            }

        }
        if(!alreadyAfflicted)
        {
            statuses.Add(new BuffHolder(status.duration, NetworkData.Instance.buffDataBase.GetId[status]));
        }
        PostStatusStatCalc();
    }
    public void ProgressStatuses()
    {
        for(int i = statuses.Count - 1; i >= 0; i--) 
        {
            if (statuses[i].ProgressStatus())
            {
                statuses.RemoveAt(i);
                
            }
        }
        PostStatusStatCalc();
    }

    public void PostStatusStatCalc()
    {
        Dictionary<Attributes, float> StatusMultipliers = new Dictionary<Attributes, float>
        {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0}
        };

        foreach (var status in statuses)
        {
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] is StatStatusEffect)
            {
                foreach (var buff in (NetworkData.Instance.buffDataBase.GetItem[status.buffId] as StatStatusEffect).stats) 
                {
                    StatusMultipliers[buff.attribute] += buff.value / 100f;
                }
            }
        }
        foreach (var attrib in StatusMultipliers.Keys)
        {
            
            postStatusStats[attrib] = Mathf.RoundToInt(stats[attrib] * (1 + StatusMultipliers[attrib]));
        }
    }   
   public void ClearCombatStatuses()
    {
        for(int i = statuses.Count - 1; i >= 0; i--)
        {
            var status = statuses[i];
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId].combatOnly)
            {
                statuses.RemoveAt(i);
            }
        }
        PostStatusStatCalc();
    }
}


