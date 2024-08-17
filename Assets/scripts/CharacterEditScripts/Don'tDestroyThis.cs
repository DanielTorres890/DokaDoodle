using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyThis : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
