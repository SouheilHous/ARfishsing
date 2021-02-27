using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UniRx.Operators;
using UnityEngine.UI;

public class PlayGroundManager : MonoBehaviour
{
    [SerializeField] GameObject[] instanceFishes;
    [SerializeField] GameObject instanceSawFishes;
    [SerializeField] GameObject instanceSpecialFishes;
    public static bool canJump;

    public int randInstance;
    [SerializeField] Transform instancePointsParent;
    private Transform[] _points;
    List<GameObject> currentInstancedFishes=new List<GameObject>();
    List<GameObject> currentInstancedRimbowFishes = new List<GameObject>();
    public int numberOfSpecialFishedOnScene;
    List<GameObject> currentInstancedSawFishes = new List<GameObject>();
    public int numberOfSawFishedOnScene;
    public int numberOfFishedOnScene;
    public ReactiveProperty<int> fishInSceneReactive = new ReactiveProperty<int>();
    public ReactiveProperty<int> specialFishInSceneReactive = new ReactiveProperty<int>();
    public ReactiveProperty<int> sawFishInSceneReactive = new ReactiveProperty<int>();

    // Start is called before the first frame update
    void Start()
    {
        canJump = true;
           _points = instancePointsParent.GetComponentsInChildren<Transform>();
        instanceFishesAtStart();
        fishInSceneReactive.Value = currentInstancedFishes.Count;
        ObserveCurrentFishes();
        FindObjectOfType<FishingPresenter>().playGroundManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void instanceFishesAtStart()
    {
        for(int i = 0; i < numberOfFishedOnScene; i++)
        {
            int randFishes = UnityEngine.Random.Range(0, instanceFishes.Length);
            int randLocation = UnityEngine.Random.Range(0, _points.Length);
            GameObject clone = Instantiate(instanceFishes[randFishes], _points[randLocation].position, instanceFishes[randFishes].transform.rotation,transform);
            currentInstancedFishes.Add(clone);
        }
        for (int i = 0; i < numberOfSpecialFishedOnScene; i++)
        {
            int randLocation = UnityEngine.Random.Range(0, _points.Length);
            GameObject clone = Instantiate(instanceSpecialFishes, _points[randLocation].position, instanceSpecialFishes.transform.rotation, transform);
            currentInstancedRimbowFishes.Add(clone);
        }
        for (int i = 0; i < numberOfSawFishedOnScene; i++)
        {
            int randLocation = UnityEngine.Random.Range(0, _points.Length);
            GameObject clone = Instantiate(instanceSawFishes, _points[randLocation].position, instanceSawFishes.transform.rotation, transform);
            currentInstancedSawFishes.Add(clone);
        }

    }
        void ObserveCurrentFishes()
    {
        fishInSceneReactive
            .Where(_ => fishInSceneReactive.Value != numberOfFishedOnScene)
            .Do(_ => instanceSingleFish())
            .Subscribe()
            .AddTo(this);
        specialFishInSceneReactive
            .Where(_ => specialFishInSceneReactive.Value != numberOfSpecialFishedOnScene)
            .Do(_ => instanceSingleFishSpecial(instanceSpecialFishes, currentInstancedRimbowFishes, specialFishInSceneReactive.Value,numberOfSpecialFishedOnScene))
            .Subscribe()
            .AddTo(this);
        sawFishInSceneReactive
            .Where(_ => sawFishInSceneReactive.Value != numberOfSawFishedOnScene)
            .Do(_ => instanceSingleFishSpecial(instanceSawFishes, currentInstancedSawFishes, sawFishInSceneReactive.Value, numberOfSawFishedOnScene))
            .Subscribe()
            .AddTo(this);

    }
    void instanceSingleFish()
    {
        int toBeIns = numberOfFishedOnScene - fishInSceneReactive.Value;
        for (int i = 0; i < toBeIns; i++)
        {
            int randFishes = UnityEngine.Random.Range(0, instanceFishes.Length);
            int randLocation = UnityEngine.Random.Range(0, _points.Length);
            GameObject clone = Instantiate(instanceFishes[randFishes], _points[randLocation].position, instanceFishes[randFishes].transform.rotation, transform);
            currentInstancedFishes.Add(clone);
            fishInSceneReactive.Value += 1;
        }
    }
    void instanceSingleFishSpecial(GameObject fish, List<GameObject> fishlist,int fishnumber,int numberinscene)
    {
        int toBeIns = numberinscene - fishnumber;
        for (int i = 0; i < toBeIns; i++)
        {
            int randLocation = UnityEngine.Random.Range(0, _points.Length);
            GameObject clone = Instantiate(fish, _points[randLocation].position, fish.transform.rotation, transform);
            fishlist.Add(clone);
            fishnumber += 1;
        }
    }
}
