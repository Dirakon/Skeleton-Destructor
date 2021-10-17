using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGmer : MonoBehaviour
{
    public static BGmer singleton;
    // Start is called before the first frame update
    void Start()
    {
        if (singleton != null)
            Destroy(gameObject);
        else{
            singleton = this;
            GetComponent<AudioSource>().Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
