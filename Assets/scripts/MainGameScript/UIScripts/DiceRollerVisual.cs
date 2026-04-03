using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollerVisual : MonoBehaviour
{
    public Sprite[] diceRollFaces;
    public Image image;
    public float diceSpeed = 0.1f;
    private int counter = 0;
    private bool complete = false;
    void Start()
    {
        
        counter = Random.Range(0, diceRollFaces.Length);
        NextRoll();
    }

    public void NextRoll()
    {
        if(complete) { return; }
        if(counter >= diceRollFaces.Length)
        {
            counter = 0;
        }
        image.sprite = diceRollFaces[counter];
        counter++;
        Tween.Delay(diceSpeed, NextRoll);
    }
    public void Complete(int stopNum)
    {
        Debug.Log("I should've stopped");
        complete = true;
        counter = stopNum;
        image.sprite = diceRollFaces[counter];
    }
}
