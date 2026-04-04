using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class DiceRollerVisual : MonoBehaviour
{
    public Sprite[] diceRollFaces;
    public Image[] diceFaces;
    public float diceSpeed = 0.1f;
    public Vector3 spinSpeed;
    private int counter = 0;
    private bool complete = false;
    void Start()
    {
        
        counter = Random.Range(0, diceRollFaces.Length);
        //Tween.Rotation(transform, endValue: new Vector3(transform.rotation.eulerAngles.x + 90f, transform.rotation.eulerAngles.y + 90f, transform.rotation.eulerAngles.z + 90f), 1f, cycles: 9999999);
        
        NextRoll();
    }
    public void Update()
    {
        if(complete) { return; }
        transform.localRotation = transform.localRotation * Quaternion.Euler( spinSpeed.x * Time.deltaTime, spinSpeed.y * Time.deltaTime, spinSpeed.z * Time.deltaTime);
    }

    public void NextRoll()
    {
        if(complete) { return; }
        if(counter >= diceRollFaces.Length)
        {
            counter = 0;
        }
        foreach(var face in diceFaces)
        {
            face.sprite = diceRollFaces[counter];
        }
      
        counter++;
        Tween.Delay(diceSpeed, NextRoll);
    }
    public void Complete(int stopNum)
    {
        Tween.StopAll(onTarget: transform);
        
        complete = true;
        Tween.Rotation(transform, endValue: Quaternion.Euler(0, 0, 0), duration: .05f);
        counter = stopNum;
        foreach (var face in diceFaces)
        {
            face.sprite = diceRollFaces[counter];
        }
    }
}
