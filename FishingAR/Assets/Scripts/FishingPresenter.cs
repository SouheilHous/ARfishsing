using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UniRx.Operators;
using UnityEngine.UI;
public class FishingPresenter : MonoBehaviour
{
    [SerializeField] GameObject hookObject;
    [SerializeField] Camera MainCam;
    [SerializeField] GameObject RodParent;
    public PlayGroundManager playGroundManager;
    private List<GameObject> lastCatchedFish=new List<GameObject>();
    [SerializeField] Slider sliderRot;
    private bool mouseClicked;
    private bool reachCatchPoint;
    private bool fishCatched;
    public Text[] fishScore;
    [SerializeField] GameObject[] particleEffectUI;
    [SerializeField] Transform particuleParent;
    [SerializeField] Transform targetPos;
    public float Timer;
    public bool gameStart;
    private Vector3 netStartRotation;
    [SerializeField] Text timerText;
    // Start is called before the first frame update
    void Start()
    {
        partEffectOff();
        netStartRotation = RodParent.transform.localEulerAngles;
        CatchFish();
        //ObserveMouseDragRod();
    }
    void CatchFish()
    {
        hookObject.OnTriggerEnterAsObservable()
            .Where(_=>gameStart==true)
            .Where(_ => _.tag == "fish")
            .Do(_ => partEffectOn())
            .Do(_ => particuleParent.gameObject.SetActive(true))
            .Do(_=> particuleParent.position=hookObject.transform.position)
            .Where(_=>_.transform.parent.GetComponent<PathController>().IsJumping==true)
            .Do(_ => _.transform.parent.GetComponent<PathController>().catched = true)
            .Do(_=>_.transform.parent.position= hookObject.transform.position)
            .Do(_ => fishCatched=true)
            .Do(_ => MainCam.GetComponent<CameraShake>().shakecamera())
            .Do(_ => _.GetComponent<Animator>().enabled=false)
            .Do(_ => _.transform.localPosition=Vector3.zero)
            .Do(_ => setFishCatched(_.transform.parent))
            .Delay(TimeSpan.FromMilliseconds(2000))
            .Do(_=>particuleParent.gameObject.SetActive(false))
            .Where(_=>lastCatchedFish!=null)
            .Do(_=> clearFish())
            .Delay(TimeSpan.FromMilliseconds(1000))
            .Do(_ => partEffectOff())
            .Subscribe()
            .AddTo(this);
        this.UpdateAsObservable()
            .Where(_ => fishCatched == true)
            .Do(_ => reachCatchPoint = false)  
            .Where(_=> lastCatchedFish[lastCatchedFish.Count - 1]!=null)
            .Subscribe(_ => moveTocatchPost(lastCatchedFish[lastCatchedFish.Count - 1].transform, hookObject.transform.GetChild(0).transform))
            .AddTo(this);
        this.UpdateAsObservable()
          .Where(_ => fishCatched == true)
          .Where(_=> targetPos!=null)
          .Subscribe(_=> moveParticule(targetPos.position))
          .AddTo(this);

    }
    void setFishCatched(Transform fish)
    {

        fish.SetParent(hookObject.transform);
        fish.GetComponent<PathController>().enabled = false;
        lastCatchedFish.Add (fish.gameObject);
    }
    void clearFish()
    {
        for(int i = 0; i < lastCatchedFish.Count; i++)
        {
            if (lastCatchedFish[i] != null)
            {
                checkFish(lastCatchedFish[i]);
                Destroy(lastCatchedFish[i]);
                if (FindObjectOfType<PlayGroundManager>() != null)
                {
                    playGroundManager = FindObjectOfType<PlayGroundManager>();
                }
                if (playGroundManager != null)
                {
                    
                    playGroundManager.fishInSceneReactive.Value -= 1;

                }
            }
        }
        fishCatched = false;

    }
    void moveTocatchPost(Transform fish,Transform targetpos)
    {
        if (fish.position != targetpos.position)
        {
            fish.position = Vector3.MoveTowards(fish.position, targetpos.position, 0.08f * Time.deltaTime);
        }
        else
        {
            reachCatchPoint = true;
            
        }
    }
    // Update is called once per frame
    void Update()
    {
      if (playerDataModel.currentGameStatus.Value != playerDataModel.GameStatus.OnTimeUp)
        {
            if (gameStart == true)
            {
                playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnGame;
                Timer -= Time.deltaTime;
                timerText.text = "Time Left : "+ (int)Timer;
                if (Timer <= 0)
                {
                    playerDataModel.lastGameScore = 0;
                    timerText.text = "Time Left : 00" ;
                    for (int i= 0; i < fishScore.Length; i++)
                    {
                        playerDataModel.lastGameScore += int.Parse(fishScore[i].text);
                    }
                    gameStart = false;
                    playerDataModel.currentGameStatus.Value = playerDataModel.GameStatus.OnTimeUp;
                }
            }
          
        }
    }
    void ObserveMouseDragRod()
    {
        
       this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .Do(_ => mouseClicked = true)
            .Subscribe(_ => moveRod())
            .AddTo(this);
       this.UpdateAsObservable()
           .Where(_ => Input.GetMouseButtonUp(0))
           .Do(_ => mouseClicked = false)
           .Subscribe()
           .AddTo(this);


       
            
    }
    void moveRod()
    {
        Vector3 mouse = Input.mousePosition;
        Ray castPoint = MainCam.ScreenPointToRay(mouse);
        RaycastHit hit;
        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity))
        {
            Vector3 rodPos = new Vector3(hit.point.x, hit.point.y, RodParent.transform.position.z);

            RodParent.transform.position = rodPos;
        }
     
    }
    public void changeRotation()
    {
        RodParent.transform.localEulerAngles = new Vector3(RodParent.transform.localEulerAngles.x, RodParent.transform.localEulerAngles.y, netStartRotation.z + sliderRot.value);
    }
    void partEffectOn()
    {
        for (int i = 0; i == particleEffectUI.Length; i++)
        {
           
                particleEffectUI[i].SetActive(true);
            
            
        }

    }
    void partEffectOff()
    {
       
        for (int i = 0; i == particleEffectUI.Length; i++)
        {
           
                particleEffectUI[i].SetActive(false);
            

        }

    }

    void moveParticule(Vector3 uiTargetPos)
    {
        if(particuleParent.position!= uiTargetPos)
        particuleParent.position = Vector3.MoveTowards(particuleParent.position, uiTargetPos, 1f * Time.deltaTime);
       
        

    }
    void checkFish(GameObject fish)
    {
        int score = new int();
        switch (fish.name)
        {
            default:
            case "blue(Clone)":
                targetPos = fishScore[0].gameObject.transform;
                score = int.Parse(fishScore[0].text) + 1;
                fishScore[0].text = score.ToString();
                break;
            case "Red(Clone)":
                targetPos = fishScore[1].gameObject.transform;
                score = int.Parse(fishScore[1].text) + 1;
                fishScore[1].text = score.ToString();
                break;
            case "green(Clone)":
                targetPos = fishScore[2].gameObject.transform;
                score = int.Parse(fishScore[2].text) + 1;
                fishScore[2].text = score.ToString();
                break;
            case "rimbowFish(Clone)":
                for(int i = 0; i < fishScore.Length; i ++)
                {
                    score = int.Parse(fishScore[i].text) + 2;
                    fishScore[i].text = score.ToString();
                }
                targetPos = fishScore[UnityEngine.Random.Range(0, 2)].gameObject.transform;
                break;
            case "SawFish(Clone)":
                for (int i = 0; i < fishScore.Length; i++)
                {
                    score = 0;
                    fishScore[i].text = score.ToString();
                }
                targetPos = fishScore[UnityEngine.Random.Range(0, 2)].gameObject.transform;
                break;
        }
    }
    public void resetScore()
    {
        int score = new int();

        for (int i = 0; i < fishScore.Length; i++)
        {
            score = 0;
            fishScore[i].text = score.ToString();
        }
    }

}
