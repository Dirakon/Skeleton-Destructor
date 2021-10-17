using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using TMPro;
using UnityEngine.SceneManagement;
public class Heart : MonoBehaviour
{
 public TextMeshProUGUI tmp;
    // Start is called before the first frame update
    float hp = 10f;
    public static  float x=0f;
    public static Heart singleton;
    public AudioSource audioSource, damage;
    public static float heartSafeZone = 1.5f;
    public float maxScale = 1.5f;
    public float scalingSpeed = 50f;
    IEnumerator autoHeal(){
        while (true){
            yield return new WaitForSeconds(4);
            if (hp != 10)
            {
                hp += 1;
            heartBeatInterval=Mathf.Max(0.25f,heartBeatInterval-0.05f);
                StartCoroutine(fixColor());
            }
        }
    }
    IEnumerator autoDifficultyIncrease(){
        while (true){
            yield return new WaitForSeconds(10);

            heartBeatInterval=Mathf.Max(0.25f,heartBeatInterval-0.05f);
        }
    }
    int heartBeats = 0;
    IEnumerator heartBeatAnimation(){
        float t = 0;
        Vector3 startingScale = new Vector3(1,1,1);
        Vector3 endScale = startingScale*maxScale;
        audioSource.Play();
        while (t < 1){
            t+=Time.deltaTime*scalingSpeed;
            transform.localScale = Vector3.Lerp(startingScale,endScale,t);
            yield return null;
        }
        t = 0;
        while (t < 1){
            t+=Time.deltaTime*scalingSpeed;
            transform.localScale = Vector3.Lerp(endScale,startingScale,t);
            yield return null;
        }
    }
    public float heartBeatInterval = 0.5f;
    public SpriteRenderer spriteRenderer;

    void HeartBeat(){
        heartBeats+=1;
        tmp.text = "Heartbeats: " + heartBeats.ToString();
StartCoroutine(heartBeatAnimation());
    }
    IEnumerator fixColor(){
        yield return new WaitForSeconds(0.2f);
spriteRenderer.color = Color.Lerp(Color.black,Color.white,hp/10f);

    }
    public void GetDamaged(){
        hp-=1f;
        damage.Play();
        spriteRenderer.color = Color.red;
        heartBeatInterval+=0.05f;
        StartCoroutine(fixColor());
        if (hp == 0){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex,LoadSceneMode.Single);
        }
    }
    void Start()
    {
        StartCoroutine(autoDifficultyIncrease());
        StartCoroutine(autoHeal());
        singleton = this;
        Search.singleton.clock += HeartBeat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
