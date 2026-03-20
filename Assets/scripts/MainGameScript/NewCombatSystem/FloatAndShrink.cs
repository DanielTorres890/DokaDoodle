using TMPro;
using UnityEngine;

public class FloatAndShrink : MonoBehaviour
{
    public TextMeshPro text;
    public float speed;
    public float shrinkSpeed;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (shrinkSpeed * Time.deltaTime));
    }
}
