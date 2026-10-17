using UnityEngine;

public class ButtonGrow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float growthAmount = .1f;
    private Vector3 originalSize;


    public void Start()
    {
        originalSize = transform.localScale;
    }
    public void Grow()
    {
        transform.localScale = originalSize * (1 + growthAmount);
    }

    public void Return()
    {
        transform.localScale = originalSize;
    }
}
