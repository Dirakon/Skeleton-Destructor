using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonRotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    float secondsPassed = 0f;
    public SpriteRenderer spriteRenderer;
    public float seconds_for_full_rotation;
    // Update is called once per frame
    void Update()
    {
        secondsPassed += Time.deltaTime;
        spriteRenderer.color = Color.Lerp(Color.white,Color.red,secondsPassed/seconds_for_full_rotation);
        transform.localRotation = transform.localRotation *  Quaternion.Euler(Vector3.back *Time.deltaTime*0.5f);
    }
}
