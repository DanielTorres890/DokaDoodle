using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EntityStats 
{
    public string name;

    public bool isDead = false;
    public List<string> loyaltyTags = new List<string>();

    public List<BuffHolder> statuses = new List<BuffHolder>();

    [JsonIgnore] public UnityEvent onStatusProgress = new UnityEvent();

    public bool stunImmune;

    public Dictionary<Attributes, int> stats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0 },
    


    };
    public Dictionary<Attributes, int> postStatusStats = new Dictionary<Attributes, int>
    {
        {Attributes.MaxHealth, 0 },
        {Attributes.Health, 0 },
        {Attributes.Attack, 0 },
        {Attributes.Defense, 0 },
        {Attributes.Magic, 0 },
        {Attributes.MDefense, 0 },
        {Attributes.Dexterity, 0},
        
        

    };
    public Dictionary<AttackTypes, int> dmgReduction = new Dictionary<AttackTypes, int> 
    {
        { AttackTypes.Physical, 0},
        { AttackTypes.Magic, 0}

    };
    public Dictionary<AttackTypes, int> postStatusDmgReduction = new Dictionary<AttackTypes, int>
    {
        { AttackTypes.Physical, 0},
        { AttackTypes.Magic, 0}

    };

    [JsonIgnore][SerializeField] public List<AttackBase> attacks = new List<AttackBase>();
    [JsonIgnore][SerializeField] public DefenseBase[] defenses = new DefenseBase[4];



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
            if(stati.buffId == NetworkData.Instance.buffDataBase.GetId[status] && !status.stackable)
            {
                stati.timeRemaining += status.GetDuration(this);
                alreadyAfflicted = true;
                break;
            }

        }
        if(!alreadyAfflicted)
        {
            statuses.Add(new BuffHolder(status.GetDuration(this), NetworkData.Instance.buffDataBase.GetId[status]));
            status.OnApply(this);
        }

        
    }
    public void ProgressStatuses(float timePassed)
    {
        for(int i = statuses.Count - 1; i >= 0; i--) 
        {
            if (statuses[i].ProgressStatus(timePassed))
            {
                NetworkData.Instance.buffDataBase.GetItem[statuses[i].buffId].OnRemove(this);
                statuses.RemoveAt(i);
                
            }
        }
        onStatusProgress.Invoke();
        PostStatusStatCalc();
    }

    public void RemoveStatus(int statusId)
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            if (statuses[i].buffId == statusId)
            {
                NetworkData.Instance.buffDataBase.GetItem[statuses[i].buffId].OnRemove(this);
                statuses.RemoveAt(i);
                PostStatusStatCalc();
                break;
            }
        }
        onStatusProgress.Invoke();
    }
    public BuffHolder GetStatus(int statusId)
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            if (statuses[i].buffId == statusId)
            {
                return statuses[i];
            }
        }

       return null;
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
        {Attributes.Dexterity, 0},
   
        };
        Dictionary<AttackTypes, int> GuardMultipliers = new Dictionary<AttackTypes, int>
        {
        {AttackTypes.Physical, 0 },
        {AttackTypes.Magic, 0 },
        
        };

        foreach (var status in statuses)
        {
            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId] is StatStatusEffect)
            {
                foreach (var buff in (NetworkData.Instance.buffDataBase.GetItem[status.buffId] as StatStatusEffect).GetStats(this)) 
                {
                    if(buff.attribute != Attributes.PDmgReduction && buff.attribute != Attributes.MDmgReduction)
                    StatusMultipliers[buff.attribute] += buff.value / 100f;

                    else if(buff.attribute == Attributes.PDmgReduction)
                        GuardMultipliers[AttackTypes.Physical] += buff.value;
                    else
                        GuardMultipliers[AttackTypes.Magic] += buff.value;

                }
            }
        }
        foreach (var attrib in StatusMultipliers.Keys)
        {
            
            postStatusStats[attrib] = Mathf.RoundToInt(stats[attrib] * (1 + StatusMultipliers[attrib]));
        }
        foreach(var attrib in GuardMultipliers.Keys)
        {
            postStatusDmgReduction[attrib] = dmgReduction[attrib] + GuardMultipliers[attrib];
        }
    }
    public bool healHp(int hp) //note this will work for dmg too ig
    {
        if (this.stats[Attributes.Health] + hp > this.stats[Attributes.MaxHealth])
        {
            this.stats[Attributes.Health] = this.stats[Attributes.MaxHealth];
        }
        else
        {
            this.stats[Attributes.Health] += hp;
        }
        PostStatusStatCalc();
        if (stats[Attributes.Health] <= 0)
        {
            return true;
        }
        return false;
    }
    public void ClearCombatStatuses()
    {
        for(int i = statuses.Count - 1; i >= 0; i--)
        {
            var status = statuses[i];

            if (NetworkData.Instance.buffDataBase.GetItem[status.buffId].combatOnly)
            {
                
                NetworkData.Instance.buffDataBase.GetItem[status.buffId].OnRemove(this);
                statuses.RemoveAt(i);
            }
        }
        onStatusProgress.Invoke();
        PostStatusStatCalc();
    }
}


