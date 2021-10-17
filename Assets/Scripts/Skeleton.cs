using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    public void tryToDestroy(bool forceDestroy = false)
    {
        if (constuctedBones == 3 || forceDestroy)
        {
            // We die here...
            Vector3 attackPosition = transform.position - Vector3.down * 0.2f;
            var bonePtr = mainBones.First;
            for (int i = 0; i < mainBones.Count; ++i)
            {
                bonePtr.Value.distanceToMainBone = 0;
                bonePtr.Value.spriteRenderer.color = Color.white;
                bonePtr = bonePtr.Next;
            }
            bonePtr = mainBones.First;
            for (int i = 0; i < mainBones.Count; ++i)
            {
                if (bonePtr != null)
                {
                    bonePtr.Value.SendFlying(bonePtr.Value.transform.position - attackPosition);
                    bonePtr = bonePtr.Next;
                }
            }
            while (bones.Count != 0){
                if (bones.First.Value.combiner == this){
                    SetBoneToBeNoLongerWithUs(bones.First.Value);
                }else{
                    bones.RemoveFirst();
                }
            }
            Search.singleton.clock -= startWalking;
            Destroy(gameObject);
        }
    }
    LinkedList<Bone> mainBones = new LinkedList<Bone>();
    public void AssignFirstBones(LinkedList<Bone> bones)
    {
        this.bones = bones;
        foreach (var bone in bones)
        {
            bone.GetCombined(this);
            if (mainBones.Count < 3)
            {
                bone.distanceToMainBone = -1;
                mainBones.AddLast(bone);
            }
        }
        StartCoroutine(constructBeggining());
    }
    const float maxDistance = 1.5f, minDistance = 0.1f, walkingSpeed = 10f;
    IEnumerator actualWalk(float toWalk, Vector3 dir)
    {   
        if (toWalk < 0)
        toWalk = - toWalk;
        dir.Normalize();
        while (toWalk > 0)
        {
            float dist = Time.deltaTime * walkingSpeed;
            dist = Mathf.Min(dist, toWalk);
            toWalk -= dist;
            foreach (var bone in mainBones)
            {
                if (bone.combiner == this)
                {
                    bone.transform.Translate(dir*dist, Space.World);
                }
            }
            transform.Translate(dir*dist, Space.World);
            yield return null;
        }
    }
    void startWalking()
    {
        float distance = Random.Range(minDistance, maxDistance);
        float heartSkeletonDistance = Mathf.Abs(transform.position.x - Heart.x);
        if (heartSkeletonDistance < Heart.heartSafeZone + maxDistance)
        {
            distance = Mathf.Max((heartSkeletonDistance - Heart.heartSafeZone), 0f);
        }
        if (distance < minDistance){
            Heart.singleton.GetDamaged();
        }
        if (transform.position.x > Heart.x)
            distance *= -1;
        Vector3 moveVector = Vector3.right * distance;
        StartCoroutine(actualWalk(distance,moveVector));
    }
    bool toBeDestroyed = false;
    public void ForceDestroy(){
        toBeDestroyed = true;
    }
    public int assumingStatus = 0; Dictionary<Transform, Bone> nodesForAddingBones = new Dictionary<Transform, Bone>();
    IEnumerator autoConstructor()
    {
        while (true)
        {
            while (constuctedBones == bones.Count || bones.Count == 0) { yield return null; }
            constuctedBones = Mathf.Min(constuctedBones, bones.Count);
            var boneNode = bones.First;
            for (int i = 0; i < constuctedBones; ++i)
            {
                if (boneNode == null)
                {
                    Debug.Log("I: " + i.ToString());
                    Debug.Log("constuctedBones: " + constuctedBones.ToString());
                    Debug.Log("mainBones.Count: " + mainBones.Count.ToString());
                }
                boneNode = boneNode.Next;
            }
            var newBone = boneNode.Value;
            int count = nodesForAddingBones.Count;
            count = Random.Range(0, count);
            foreach (var node in nodesForAddingBones)
            {
                if (count-- == 0)
                {
                    newBone.distanceToMainBone = node.Value.distanceToMainBone + 1;
                    bool areWeUpFromIt = node.Key == node.Value.up;
                    Vector2 dir = Random.onUnitSphere;
                    dir.Normalize();
                    int saveLayer = node.Value.gameObject.layer;
                    //  node.Value.gameObject.layer = gameObject.layer;
                    if (Physics2D.Raycast(node.Key.position + (Vector3)dir * 0.3f, dir,  1).collider != null)
                    {
                        break;
                    }
                    node.Value.gameObject.layer = saveLayer;
                    assumingStatus = 0;
                    mainBones.AddLast(newBone);
                    newBone.startAssumingThePosition(node.Key, dir, node.Value, areWeUpFromIt);
                    while (assumingStatus == 0) { yield return null; }
                    if (assumingStatus == -1)
                    {
                        mainBones.Remove(newBone);
                        // Failed
                        break;
                    }
                    nodesForAddingBones.Add(newBone.up, newBone);
                    nodesForAddingBones.Add(newBone.down, newBone);
                    constuctedBones++;
                    break;
                }
            }
            yield return null;




        }
    }
    int constuctedBones = 3;
    IEnumerator constructBeggining()
    {assumingStatus = 0;
        mainBones.First.Value.startAssumingThePosition(transform, Vector3.up);
        while (assumingStatus == 0){yield return null;}
        mainBones.First.Value.spriteRenderer.color = Color.red;
        nodesForAddingBones.Add(mainBones.First.Value.up, mainBones.First.Value);
        Vector3 rightLeg = (Vector3.right + Vector3.down).normalized;
        Vector3 leftLeg = (Vector3.left + Vector3.down).normalized;
    assumingStatus = 0;
        mainBones.First.Next.Value.startAssumingThePosition(mainBones.First.Value.down, rightLeg, mainBones.First.Value, false);
        while (assumingStatus == 0){yield return null;}
        mainBones.First.Next.Value.spriteRenderer.color = Color.red;
    assumingStatus = 0;
        mainBones.Last.Value.startAssumingThePosition(mainBones.First.Value.down, leftLeg, mainBones.First.Value, false);
        while (assumingStatus == 0){yield return null;}
        mainBones.Last.Value.spriteRenderer.color = Color.red;
        Search.singleton.clock += startWalking;
        yield return autoConstructor();
    }
    LinkedList<Bone> bones;
    IEnumerator watchAfterBone(Bone bone)
    {
        while (bone.flying) { yield return null; }
        if (bone.combiner == null && bonesToWatch.Contains(bone))
        {
            bonesToWatch.Remove(bone);
            bones.AddLast(bone);
            bone.GetCombined(this);
        }
    }
    LinkedList<Bone> bonesToWatch = new LinkedList<Bone>();

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        Bone bone = collider2D.GetComponent<Bone>();
        if (bone != null && !bones.Contains(bone) && bone.combiner == null)
        {
            if (bone.flying)
            {
                bonesToWatch.AddLast(bone);
                StartCoroutine(watchAfterBone(bone));
                return;
            }
            bones.AddLast(bone);
            bone.GetCombined(this);
        }
    }
    void OnTriggerExit2D(Collider2D collider2D)
    {
        Bone bone = collider2D.GetComponent<Bone>();
        if (bone != null && !bones.Contains(bone))
        {
            bonesToWatch.Remove(bone);
        }
    }
    public void SetBoneToBeNoLongerWithUs(Bone bone)
    {
        bones.Remove(bone);
        mainBones.Remove(bone);
        bone.combiner = null;
        if (nodesForAddingBones.ContainsKey(bone.up) || nodesForAddingBones.ContainsKey(bone.down))
        {
            constuctedBones--;
            nodesForAddingBones.Remove(bone.up);
            nodesForAddingBones.Remove(bone.down);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (toBeDestroyed){
            tryToDestroy(true);
        }
    }
}
