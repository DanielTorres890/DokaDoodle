using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    //wrapper to use playermovemanager bc i made many mistakes with my early set up
    public void RollTheDice()
    {

        PlayerMoveManager.Instance.rollDice();
    }
}
