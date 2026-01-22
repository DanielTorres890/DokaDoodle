using TMPro;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    //IN COMBAT ONLY
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.LookAt(NewCombatManager.instance.cameras[NewCombatManager.instance.currentSpec].gameObject.transform.position);
        gameObject.transform.RotateAround(gameObject.transform.position, transform.up, 180f);
    }
}
