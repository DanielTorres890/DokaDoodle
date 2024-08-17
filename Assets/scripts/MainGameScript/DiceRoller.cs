using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    // Start is called before the first frame update
    public int[] DiceValues;
    public int DiceTotal;
    void Awake()
    {
        DiceValues = new int[4];
    }

    public void RollTheDice()
    {

        for (int i = 0; i < DiceValues.Length; i++)
        {
            DiceValues[i] = Random.Range(0, 2);
            DiceTotal += DiceValues[i];
        }
    }
}
