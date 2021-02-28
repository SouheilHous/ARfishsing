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
    private bool dragging = false;
    bool partState;
    private float distance;
    Vector3 initialPosition;
    [SerializeField] Sprite[] catchedFishImage;
    [SerializeField] Text catchedFishMessage;
    [SerializeField] Image catchedFishImagePanel;
    [SerializeField] GameObject sawFishPanelWarn;
    [SerializeField] GameObject catchedFishPanel;
    [SerializeField] SoundView soundManager;
    public ReactiveProperty<bool> destroyNet = new ReactiveProperty<bool>();
    // Start is called before the first frame update
    void Start()
    {
        destroyNet.Value = false;
        partEffectOff();
        netStartRotation = RodParent.transform.localEulerAngles;
        CatchFish();
        ObserveMouseDragRod();
    }
    void CatchFish()
    {
        hookObject.OnTriggerEnterAsObservable()
            .Where(_ => gameStart == true)
            .Where(_ => _.tag == "fish")
            .Where(_ => _.transform.parent.GetComponent<PathController>().IsJumping == true)
            .Do(_ => particuleParent.gameObject.SetActive(true))
            .Do(_ => particuleParent.position = hookObject.transform.position)
            .Do(_ => partEffectOn())
            .Do(_ => _.transform.parent.GetComponent<PathController>().catched = true)
            .Do(_ => _.transform.parent.position = hookObject.transform.position)
            .Do(_ => fishCatched = true)
            //.Do(_ => MainCam.GetComponent<CameraShake>().shakecamera())
            .Do(_ => _.GetComponent<Animator>().enabled = false)
            .Do(_ => _.transform.localPosition = Vector3.zero)
            .Do(_ => setFishCatched(_.transform.parent))
            .Delay(TimeSpan.FromMilliseconds(2000))
            .Do(_ => catchedFishPanel.SetActive(false))
            .Do(_ => catchedFishPanel.GetComponent<Animator>().enabled = false)
            .Do(_ => sawFishPanelWarn.SetActive(false))
            .Do(_=>particuleParent.gameObject.SetActive(false))
            .Where(_=>lastCatchedFish!=null)
            .Do(_=> clearFish())
            .Delay(TimeSpan.FromMilliseconds(1000))
            .Do(_=> PlayGroundManager.canJump = true)
            .Do(_=>soundManager.playCurrentActionSFX(soundManager.effectCatch))
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
        destroyNet
            .Where(_ => destroyNet.Value == true)
            .Do(_ => RodParent.transform.GetChild(0).gameObject.SetActive(false))
            .Delay(TimeSpan.FromMilliseconds(4000))
            .Do(_ => RodParent.transform.GetChild(0).gameObject.SetActive(true))
            .Do(_ => destroyNet.Value = false)
            .Subscribe()
            .AddTo(this);

    }
    void setFishCatched(Transform fish)
    {

        fish.SetParent(hookObject.transform);
        fish.GetComponent<PathController>().enabled = false;
        lastCatchedFish.Add (fish.gameObject);
        catchedFishShow(fish.name);
    }
    void clearFish()
    {
       
        for (int i = 0; i < lastCatchedFish.Count; i++)
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
            .Where(_=> playerDataModel.currentGameStatus.Value == playerDataModel.GameStatus.OnGame)
            .Where(_ => Input.GetMouseButton(0))
            .Do(_ => mouseClicked = true)
            .Subscribe(_ => moveRod())
            .AddTo(this);
        this.UpdateAsObservable()
            .Where(_ => playerDataModel.currentGameStatus.Value == playerDataModel.GameStatus.OnGame)
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => setRay())
            .AddTo(this);
        this.UpdateAsObservable()
            .Where(_ => playerDataModel.currentGameStatus.Value == playerDataModel.GameStatus.OnGame)
           .Where(_ => Input.GetMouseButtonUp(0))
           .Do(_ => mouseClicked = false)
           .Do(_ => dragging = false)
           .Subscribe()
           .AddTo(this);


       
            
    }
    void moveRod()
    {

        Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = ray.GetPoint(distance);
        RodParent.transform.position = rayPoint;



    }
    void setRay()
    {
        Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);
        initialPosition = transform.position;

        Vector3 rayPoint = ray.GetPoint(0);

        // Not sure but this might get you a slightly better value for distance
        distance = Vector3.Distance(transform.position, rayPoint);
    }
    public void changeRotation()
    {
        RodParent.transform.localEulerAngles = new Vector3(RodParent.transform.localEulerAngles.x, RodParent.transform.localEulerAngles.y, netStartRotation.z + sliderRot.value);
    }
    void partEffectOn()
    {
        soundManager.playCurrentActionSFX(soundManager.effectCatch);
        if (partState == true)
        {
            partState = false;
            for (int i = 0; i == particleEffectUI.Length; i++)
            {

                particleEffectUI[i].SetActive(true);


            }
        }
        

    }
    void partEffectOff()
    {
       
        for (int i = 0; i == particleEffectUI.Length; i++)
        {
           
                particleEffectUI[i].SetActive(false);
            

        }
        partState = true;
     

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
    void catchedFishShow(string name)
    {
        Debug.Log(name);
        catchedFishPanel.SetActive(true);
        catchedFishPanel.GetComponent<Animator>().enabled = true;
        switch (name)
        {
            default:
            case "blue(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[0];
                catchedFishMessage.text = "CATCHED FISH : BLUE&YELLOW";
                soundManager.playFishCatchedSfx(0);
                break;
            case "Red(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[1];
                catchedFishMessage.text = "CATCHED FISH : WHITE&PURPLE";
                soundManager.playFishCatchedSfx(0);
                break;
            case "green(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[2];
                catchedFishMessage.text = "CATCHED FISH : ORANGE&GREEN";
                soundManager.playFishCatchedSfx(0);

                break;
            case "rimbowFish(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[3];
                catchedFishMessage.text = "CATCHED FISH : RAINBOW FISH";
                soundManager.playFishCatchedSfx(1);

                break;
            case "SawFish(Clone)":
                catchedFishImagePanel.sprite = catchedFishImage[4];
                destroyNet.Value = true;
                catchedFishMessage.text = "CATCHED FISH : SAW FISH";
                sawFishPanelWarn.SetActive(true);
                soundManager.playFishCatchedSfx(2);

                break;
        }
    }

}
