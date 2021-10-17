using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airship : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public float minX,maxX;
    public float speed;
    public float minInterval,maxInterval;
    public GameObject BonePrefab, spawnHole;
    IEnumerator autoSpawner(){
        while (true){
            yield return new WaitForSeconds(Random.Range(minInterval,maxInterval));
            Bone bone = Instantiate(BonePrefab,spawnHole.transform.position,spawnHole.transform.rotation).GetComponent<Bone>();
            bone.SendFlying(new Vector3(Random.Range(-1f,1f),Random.Range(0.1f,1f),0));
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(autoSpawner());
    }
    

    // Update is called once per frame
    void Update()
    {
        if ((transform.position.x < minX && speed < 0) || (transform.position.x > maxX && speed > 0)){
            speed = -speed;
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
        transform.Translate(Time.deltaTime*speed*Vector3.right,Space.World);
    }
}
