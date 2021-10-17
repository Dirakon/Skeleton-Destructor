using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bone : MonoBehaviour
{
    // Start is called before the first frame update
    //   LinkedListNode<Bone> dis=null;
    public bool isScull = false;
    public SpriteRenderer spriteRenderer;

    void Awake()
    {
        if (null == freeNonFlyingBones || freeNonFlyingBones.Count == 0 || freeNonFlyingBones.First.Value == null)
        {
            freeNonFlyingBones = new LinkedList<Bone>();
        }
        FindPlaceInList();
    }
    void FindPlaceInList()
    {
        if (freeNonFlyingBones.Count == 0 || transform.position.x < freeNonFlyingBones.First.Value.transform.position.x)
        {
            freeNonFlyingBones.AddFirst(this);
            //     dis = freeNonFlyingBones.First;
            return;
        }
        for (LinkedListNode<Bone> it = freeNonFlyingBones.First; ;)
        {
            it = it.Next;
            if (it == null)
                break;
            if (transform.position.x < it.Value.transform.position.x)
            {
                freeNonFlyingBones.AddBefore(it, this);
                //           dis = it.Previous;
                return;
            }
        }
        freeNonFlyingBones.AddLast(this);
        //    dis = freeNonFlyingBones.Last;
    }
    void Start()
    {

    }
    Quaternion rightGoal = Quaternion.identity;
    Vector2 vectorRightGoal = Vector2.right;


    Quaternion rightGoalNonFlight = Quaternion.identity;
    Vector2 vectorRightGoalNonFlight = Vector2.right;
    Quaternion maxRotation = Quaternion.identity, minRotation = Quaternion.identity, midRotation = Quaternion.identity;
    IEnumerator actualRotateNonFlight()
    {
        if (!orderToStopRotating && bonesDown.First != null)
        {
            float t = 0;
            Quaternion startRotation = transform.rotation;
            Transform neededTransform = areWeUpFromFatherBone ? bonesDown.First.Value.up : bonesDown.First.Value.down;
            while (t < 1 && !orderToStopRotating)
            {
                t += Time.deltaTime * coolRotationSpeed;
                transform.rotation = Quaternion.Lerp(startRotation, rightGoalNonFlight, t);
                transform.position = neededTransform.position + transform.right;
                yield return new WaitForFixedUpdate();
            }
        }
    }
    void rotateNonFlight()
    {
        rightGoalNonFlight = Quaternion.Inverse(Quaternion.Lerp(minRotation, maxRotation, Random.Range(0f, 1f)));

        StartCoroutine(actualRotateNonFlight());
    }
    IEnumerator autoRotate()
    {
        while (true)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rightGoal, Time.deltaTime * rotationSpeed);
            yield return new WaitForFixedUpdate();
        }
    }

    void InstantlySetNewRight(Vector2 newRight)
    {
        SetNewRight(newRight);
        transform.rotation = rightGoal;
    }
    void SetNewRight(Vector2 newRight)
    {
        vectorRightGoal = newRight;
        rightGoal = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, newRight));
    }
    void TakeGravity()
    {
        SetNewRight(Vector2.MoveTowards(transform.right, Vector2.down, Time.deltaTime * gravityStrenght).normalized);
    }
    public static LinkedList<Bone> freeNonFlyingBones;
    const float minForce = 4.5f, maxForce = 8.0f, floorLevel = 0f, gravityStrenght = 1.5f, coolRotationSpeed = 10f, rotationSpeed = 100f, landingRandom = 0.2f,
    assumingSpeed = 3f, awaitingAfterAssumption = 0.25f, angleChange = 50f;
    public int distanceToMainBone = 0;
    public Search beingSearched = null;
    LinkedList<Bone> bonesUp = new LinkedList<Bone>(), bonesDown = new LinkedList<Bone>();
    [SerializeField] public Transform up, down;
    public bool flying = false;
    public Skeleton combiner = null;
    public void GetCombined(Skeleton combiner)
    {
        this.combiner = combiner;
        freeNonFlyingBones.Remove(this);
    }
    public bool orderToStopRotating = true;

    bool currentlyAssumingThePosition = false;
    bool areWeUpFromFatherBone = false;
    public void startAssumingThePosition(Transform nodeTransform, Vector3 newRight, Bone downBone = null, bool areWeUpFromIt = true){

        if (areWeUpFromIt != true && downBone == null)
        {
            Debug.LogError("ERRORRR!");
        }
        currentlyAssumingThePosition = true;
        areWeUpFromFatherBone = areWeUpFromIt;
        if (downBone != null)
        {
            if (downBone.flying)
            {
                // what to do???
                Debug.LogError("trying to save!!!");
                combiner.ForceDestroy();
                if (distanceToMainBone ==-1){
                    Debug.LogError("I'm a main bone!!!");
                }
                combiner.assumingStatus = -1;
                SendOnlySelfFlying(Vector3.down,true);
                
                return;
                Debug.LogError("after break???");
            }
            downBone.orderToStopRotating = true;
            Search.singleton.clock -= downBone.rotateNonFlight;
            if (distanceToMainBone != -1)
            {
                distanceToMainBone = downBone.distanceToMainBone + 1;
            }
            if (areWeUpFromIt)
                downBone.bonesUp.AddLast(this);
            else
                downBone.bonesDown.AddLast(this);
            if (downBone.flying)
            {
                Debug.LogError("You are right!");
            }
            bonesDown.AddLast(downBone);
        }
        StartCoroutine(assumeThePosition(nodeTransform,newRight,downBone,areWeUpFromIt));
    }
    IEnumerator assumeThePosition(Transform nodeTransform, Vector3 newRight, Bone downBone = null, bool areWeUpFromIt = true)
    {
        float t = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.back, newRight));
        minRotation = endRotation;
        maxRotation = endRotation;
        minRotation *= Quaternion.Euler(Vector3.back * angleChange);
        maxRotation *= Quaternion.Euler(-Vector3.back * angleChange);
        while (t < 1)
        {
            t += Time.deltaTime * assumingSpeed;
            if (t > 1)
                t = 1;
            transform.position = Vector3.Lerp(startPosition, nodeTransform.position + newRight, t);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
            yield return null;
        }
        yield return new WaitForSeconds(awaitingAfterAssumption);
        Search.singleton.clock += rotateNonFlight;
        orderToStopRotating = false;

        combiner.assumingStatus = 1;
        currentlyAssumingThePosition = false;
    }
    public void InterruptCombining()
    {
        if (combiner == null)
            return;
        if (currentlyAssumingThePosition)
        {
            currentlyAssumingThePosition = false;
            combiner.assumingStatus = -1;
        }
        if (!orderToStopRotating)
        {
            orderToStopRotating = true;
            Search.singleton.clock -= rotateNonFlight;
        }
        combiner.SetBoneToBeNoLongerWithUs(this);
    }
    IEnumerator fly(Vector3 dir, float force)
    {
        var angle1 = Mathf.Abs(Vector3.Angle(transform.right, dir));
        var angle2 = Mathf.Abs(Vector3.Angle(-transform.right, dir));
        if (angle2 < angle1)
        {
            InstantlySetNewRight(-transform.right);
        }
        StartCoroutine(autoRotate());
        while (transform.position.y > floorLevel || Vector2.Dot(Vector2.up, transform.right) > 0)
        {
            if (bonesDown.Count + bonesUp.Count != 0)
            {
                Debug.LogError(bonesDown.Count.ToString() + "," + bonesUp.Count.ToString());
            }
            TakeGravity();
            transform.Translate(transform.right * Time.deltaTime * force, Space.World);
            yield return null;
        }
        FindPlaceInList();
        flying = false;
        InstantlySetNewRight(((Vector2)Random.onUnitSphere).normalized);
        transform.position += new Vector3(0, Random.Range(-landingRandom, landingRandom), 0);
        StopAllCoroutines();
    }
    void SendOnlySelfFlying(Vector3 dir, bool dontStopCourutines = false)
    {
        flying = true;
        InterruptCombining();
        if (!dontStopCourutines)
            StopAllCoroutines();
        SetNewRight(dir);
        beingSearched?.ForceRemoveFromCurrentPack(this);
        freeNonFlyingBones.Remove(this);
        StartCoroutine(fly(dir, Random.Range((minForce), maxForce)));
    }
    /*public void ForSureSendFlying(Vector3 dir)
    {
        flying = true;
        foreach (var bone in bonesUp)
        {
            if (!bone.flying)
                bone.ForSureSendFlying(dir);
        }
        bonesUp.Clear();
        bonesDown.Clear();
        SendOnlySelfFlying(dir);
    }*/
    public void SendFlying(Vector3 dir)
    {
        if (flying)
            return;
        flying = true;
        dir.Normalize();
        foreach (var bone in bonesUp)
        {
            if (bone.distanceToMainBone >= distanceToMainBone)
            {
                bone.SendFlying(dir);
            }
            else
            {
                if (bone.bonesUp.Count + bone.bonesDown.Count == 1 && bone.distanceToMainBone != -1)
                {
                    Search.singleton.clock += bone.rotateNonFlight;
                    bone.orderToStopRotating = false;
                }
                //    bone.bonesUp.Remove(this);
                //   bone.bonesDown.Remove(this);
            }
        }
        foreach (var bone in bonesDown)
        {
            if (bone.distanceToMainBone >= distanceToMainBone)
            {
                bone.SendFlying(dir);
            }
            else
            {
                if (bone.bonesUp.Count + bone.bonesDown.Count == 1 && bone.distanceToMainBone != -1)
                {
                    Search.singleton.clock += bone.rotateNonFlight;
                    bone.orderToStopRotating = false;
                }
                //   bone.bonesUp.Remove(this);
                //   bone.bonesDown.Remove(this);
            }
        }
        bonesUp.Clear();
        bonesDown.Clear();
        SendOnlySelfFlying(dir);
    }
    // Update is called once per frame
    void Update()
    {

    }
    void OnMouseOver()
    {
        if (distanceToMainBone == -1)
        {
            combiner.tryToDestroy();
            return;
        }
        //Vector3 mousePos = ;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;
        if (bonesUp.Count == 0 && bonesDown.Count <= 1)
        {
            Vector3 dir = transform.position - mousePos;
            if (bonesDown.Count == 0)
            {
                if (dir.y < 0)
                    dir.y = -dir.y;
            }
            else
            {
                bonesDown.First.Value.bonesDown.Remove(this);
                bonesDown.First.Value.bonesUp.Remove(this);
            }
            bonesDown.Clear();
            SendOnlySelfFlying(dir.normalized);
            return;
        }
        float distanceToUp = (up.position - mousePos).sqrMagnitude;
        float distanceToDown = (down.position - mousePos).sqrMagnitude;
        if (distanceToUp < distanceToDown)
        {
            foreach (var upper in bonesUp)
            {
                if (upper.distanceToMainBone >= distanceToMainBone)
                {
                    upper.SendFlying(upper.transform.position - transform.position);
                }
                else
                {
                    Debug.LogError("FATHER IN UP LIST! WHAAAAT!");
                }
            }
            Bone father = null;
            foreach (var upper in bonesDown)
            {
                if (upper.distanceToMainBone >= distanceToMainBone)
                {
                    upper.SendFlying(upper.transform.position - transform.position);
                }
                else
                {
                    father = upper;
                }
            }
            bonesUp.Clear();
            bonesDown.Clear();
            if (father != null)
                bonesDown.AddLast(father);
            else
                Debug.LogError("What?");
        }
        else
        {
            foreach (var upper in bonesDown)
            {
                if (upper.distanceToMainBone < distanceToMainBone)
                {
                    upper.bonesUp.Remove(this);
                    upper.bonesDown.Remove(this);
                    break;
                }
            }
            SendFlying(transform.position - mousePos);

        }
    }
}
