using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search : MonoBehaviour
{
    public LinkedList<Bone> currentBonePack = new LinkedList<Bone>();
    LinkedListNode<Bone> currentPlace = null;
    public float allowedDistance = 5f;
    public float minAmountToCombine = 4f;
    public GameObject combiningPrefab;
    public static Search singleton;
    public System.Action clock;
    IEnumerator autoSync(){
        while (true){
            clock?.Invoke();
            yield return new WaitForSeconds(Heart.singleton == null? 0.5f : Heart.singleton.heartBeatInterval);
        }
    }
    IEnumerator passiveSearch()
    {
        while (true)
        {
            while (Bone.freeNonFlyingBones.Count == 0){yield return null;}
            if (currentPlace == null)
            {
                currentBonePack.Clear();
                currentPlace = Bone.freeNonFlyingBones.First;
            }
            if (currentBonePack.Count != 0)
            {
                currentBonePack.First.Value.beingSearched = null;
                currentBonePack.RemoveFirst();
            }
            if (currentBonePack.Count == 0)
            {
                currentBonePack.AddFirst(currentPlace.Value);
                currentBonePack.First.Value.beingSearched = this;
                currentPlace = currentPlace.Next;
            }
            while (currentPlace != null && currentPlace.Value.transform.position.x
            - currentBonePack.First.Value.transform.position.x < allowedDistance)
            {
                currentBonePack.AddLast(currentPlace.Value);
                currentPlace.Value.beingSearched = this;
                currentPlace = currentPlace.Next;
            }
            if (currentBonePack.Count >= minAmountToCombine)
            {
                // COMBINING STARTS!
                float combiningX = (currentBonePack.Last.Value.transform.position.x
                + currentBonePack.First.Value.transform.position.x) * 0.5f;
                if (combiningX < Heart.x){
                    if (combiningX > Heart.x-Heart.heartSafeZone){
                        combiningX = Heart.x-Heart.heartSafeZone;
                    }
                }else{
                    if (combiningX < Heart.x+Heart.heartSafeZone){
                        combiningX = Heart.x+Heart.heartSafeZone;
                    }
                }
                Skeleton skeleton = Instantiate(combiningPrefab, new Vector3(combiningX, 1.2f, 0), Quaternion.identity)
                .GetComponent<Skeleton>();
                foreach (var bone in currentBonePack)
                {
                    bone.beingSearched = null;
                }
                skeleton.AssignFirstBones(currentBonePack);
                currentBonePack = new LinkedList<Bone>();
                if (currentPlace != null)
                {
                    currentPlace = currentPlace.Next;
                }
            }
            yield return null;
        }
    }
    // Start is called before the first frame update
    void Awake(){
        singleton = this;
        StartCoroutine(autoSync());
    }
    void Start()
    {
        StartCoroutine(passiveSearch());
    }
    public void ForceRemoveFromCurrentPack(Bone bone)
    {
        currentBonePack.Remove(bone);
        if (currentPlace != null && bone == currentPlace.Value)
        {

            currentPlace = currentPlace.Previous == null ? currentPlace.Next : currentPlace.Previous;
        }
        bone.beingSearched = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
